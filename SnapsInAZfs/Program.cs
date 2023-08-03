#region MIT LICENSE

// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/

#endregion

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using PowerArgs;
using SnapsInAZfs.Interop.Libc.Enums;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Settings.Logging;
using SnapsInAZfs.Settings.Settings;
using LogLevel = NLog.LogLevel;
using Monitor = SnapsInAZfs.Monitoring.Monitor;

namespace SnapsInAZfs;

[UsedImplicitly]
internal class Program
{
    // Note that logging will be at whatever level is defined in SnapsInAZfs.nlog.json until configuration is initialized, regardless of command-line parameters.
    // Desired logging parameters should be set in SnapsInAZfs.nlog.json
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );
    internal static readonly Monitor ServiceObserver = new( );
    internal static SnapsInAZfsSettings? Settings;

    internal static IZfsCommandRunner? ZfsCommandRunnerSingleton;

    [ExcludeFromCodeCoverage(Justification = "Largely un-testable")]
    public static async Task<int> Main( string[] argv )
    {
        LoggingSettings.ConfigureLogger( );

        DateTimeOffset currentTimestamp = DateTimeOffset.Now;

        Logger.Trace( "Parsing command-line arguments" );
        CommandLineArguments? args = await Args.ParseAsync<CommandLineArguments>( argv ).ConfigureAwait( true );

        // The nullability context in PowerArgs is wrong, so this absolutely can be null
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if ( args is null || args.Help )
        {
            Logger.Trace( "Help argument provided. Exiting." );
            return (int)Errno.ECANCELED;
        }

        if ( args.Version )
        {
            // ReSharper disable once ExceptionNotDocumented
            string versionString = $"SnapsInAZfs Version: {Assembly.GetEntryAssembly( )?.GetCustomAttribute<AssemblyInformationalVersionAttribute>( )?.InformationalVersion}";
            Console.WriteLine( versionString );
            Logger.Debug( versionString );
            Logger.Trace( "Version argument provided. Exiting." );
            return (int)Errno.ECANCELED;
        }

        SetCommandLineLoggingOverride( args );

        if ( !LoadConfigurationFromConfigurationFiles( ref Settings ) )
        {
            return (int)Errno.EFTYPE;
        }

        ApplyCommandLineArgumentOverrides( in args, Settings );

        if ( args.ConfigConsole )
        {
            try
            {
                if ( TryGetZfsCommandRunner( Settings, out IZfsCommandRunner? zfsCommandRunner ) )
                {
                    ConfigConsole.ConfigConsole.RunConsoleInterface( zfsCommandRunner );
                }
            }
            catch ( Exception e )
            {
                Logger.Fatal( e, "Error in configuration console - Exiting" );
                return (int)Errno.GenericError;
            }

            return 0;
        }

        SiazService.Timestamp = currentTimestamp;
        SiazService? serviceInstance = null;
        try
        {
            serviceInstance = GetSiazServiceInstance( Settings );
            if ( serviceInstance is null )
            {
                Logger.Fatal( "Failed to create service instance - exiting" );
                return (int)Errno.ENOATTR;
            }

            WebApplicationBuilder serviceBuilder = WebApplication.CreateBuilder( );

            // Disposal happens after service shutdown, so this inspection can be ignored here
            // ReSharper disable once AccessToDisposedClosure
            serviceBuilder.Host.UseSystemd( ).ConfigureServices( ( _, services ) => { services.AddHostedService( _ => serviceInstance ); } );
            WebApplication svc;
            if ( Settings.Monitoring.TcpListenerEnabled || Settings.Monitoring.UnixSocketEnabled )
            {
                serviceBuilder.WebHost.UseKestrel( ConfigureKestrelOptions );
                svc = serviceBuilder.Build( );
                RouteGroupBuilder statusGroup = svc.MapGroup( "/" );
                statusGroup.MapGet( "/state", ServiceObserver.GetApplicationState );
                statusGroup.MapGet( "/workingset", ServiceObserver.GetWorkingSet );
                statusGroup.MapGet( "/version", ServiceObserver.GetVersion );
            }
            else
            {
                svc = serviceBuilder.Build( );
            }

            using CancellationTokenSource tokenSource = new( );
            CancellationToken masterToken = tokenSource.Token;
            await svc.StartAsync( masterToken ).ConfigureAwait( true );
            await svc.WaitForShutdownAsync( masterToken ).ConfigureAwait( true );
            return SiazService.ExitStatus;
        }
        finally
        {
            serviceInstance?.Dispose( );
        }
    }

    /// <summary>
    ///     Overrides configuration values specified in configuration files or environment variables with arguments supplied on
    ///     the CLI
    /// </summary>
    /// <param name="args"></param>
    /// <param name="programSettings">
    ///     A reference to an instance of a <see cref="SnapsInAZfsSettings" /> object to modify
    /// </param>
    internal static void ApplyCommandLineArgumentOverrides( in CommandLineArguments args, SnapsInAZfsSettings programSettings )
    {
        Logger.Debug( "Overriding settings using arguments from command line." );

        programSettings.DryRun |= args.DryRun;
        programSettings.TakeSnapshots = ( programSettings.TakeSnapshots || args.TakeSnapshots || args.Cron ) && !args.NoTakeSnapshots;
        programSettings.PruneSnapshots = ( programSettings.PruneSnapshots || args.PruneSnapshots || args.ForcePrune || args.Cron ) && !args.NoPruneSnapshots;
        programSettings.Daemonize = ( programSettings.Daemonize || args.Daemonize ) && args is { NoDaemonize: false, ConfigConsole: false };
        if ( args.DaemonTimerInterval > 0 )
        {
            programSettings.DaemonTimerIntervalSeconds = Math.Clamp( args.DaemonTimerInterval, 1u, 60u );
        }
    }

    private static bool LoadConfigurationFromConfigurationFiles( [NotNullWhen( true )]ref  SnapsInAZfsSettings? settings )
    {
        // Configuration is built in the following order from various sources.
        // Configurations from all sources are merged, and the final configuration that will be used is the result of the merged configurations.
        // If conflicting items exist in multiple configuration sources, the configuration of the configuration source added latest will
        // override earlier values.
        // Note that nlog-specific configuration is separate, in SnapsInAZfs.nlog.json, and is not affected by the configuration specified below,
        // and is loaded/parsed FIRST, before any configuration specified below.
        // See the SnapsInAZfs.Settings.Logging.LoggingSettings class for nlog configuration details.
        // See documentation for a more detailed explanation with examples.
        // Configuration order:
        // 1. /usr/local/share/SnapsInAZfs/SnapsInAZfs.json   #(Required - Base configuration - Should not be modified by the user)
        // 2. /etc/SnapsInAZfs/SnapsInAZfs.local.json
        // 6. Command-line arguments passed on invocation of SnapsInAZfs
        Logger.Debug( "Getting base configuration from files" );
        IConfigurationRoot rootConfiguration = new ConfigurationBuilder( )
                                           #if ALLOW_ADJACENT_CONFIG_FILE
                                               .AddJsonFile( "SnapsInAZfs.json", true, false )
                                               .AddJsonFile( "SnapsInAZfs.local.json", true, false )
                                           #endif
                                           #if !WINDOWS
                                               .AddJsonFile( "/usr/local/share/SnapsInAZfs/SnapsInAZfs.json", true, false )
                                               .AddJsonFile( "/etc/SnapsInAZfs/SnapsInAZfs.local.json", true, false )
                                           #endif
                                               .Build( );

        Logger.Trace( "Building settings objects from IConfiguration" );
        try
        {
            settings = rootConfiguration.Get<SnapsInAZfsSettings>( ) ?? throw new InvalidOperationException( );
        }
        catch ( Exception ex )
        {
            Logger.Fatal( ex, "Unable to parse settings from JSON" );
            return false;
        }

        return true;
    }

    internal static bool TryGetZfsCommandRunner( SnapsInAZfsSettings settings, [NotNullWhen( true )] out IZfsCommandRunner? zfsCommandRunner, bool reuseSingleton = true )
    {
        if ( reuseSingleton && ZfsCommandRunnerSingleton is { } singleton )
        {
            zfsCommandRunner = singleton;
            return true;
        }

        Logger.Trace( "Getting ZFS command runner for the current environment" );
        try
        {
            GetZfsCommandRunner( settings, out zfsCommandRunner );
        }
        catch ( ArgumentNullException ex )
        {
            Logger.Fatal( ex, "Null or empty string provided for ZfsPath or ZpoolPath - Cannot continue" );
            zfsCommandRunner = null;
            return false;
        }
        catch ( FileNotFoundException ex )
        {
            Logger.Fatal( ex, ex.Message );
            zfsCommandRunner = null;
            return false;
        }

        if ( reuseSingleton )
        {
            ZfsCommandRunnerSingleton = zfsCommandRunner;
        }

        return true;
    }

    private static void ConfigureKestrelOptions( WebHostBuilderContext builderContext, KestrelServerOptions kestrelOptions )
    {
        kestrelOptions.Configure( ).Load( );
        if ( Settings?.Monitoring.UnixSocketEnabled ?? false )
        {
            if ( !string.IsNullOrWhiteSpace( Settings.Monitoring.UnixSocketPath ) )
            {
                kestrelOptions.ListenUnixSocket( Settings.Monitoring.UnixSocketPath );
            }
            else
            {
                Logger.Error( "UnixSocketPath must be a valid path" );
            }
        }

        if ( Settings?.Monitoring.TcpListenerEnabled ?? false )
        {
            kestrelOptions.ListenAnyIP( Settings.Monitoring.TcpListenerPort );
        }
    }

    private static SiazService? GetSiazServiceInstance( SnapsInAZfsSettings settings )
    {
        if ( !TryGetZfsCommandRunner( settings, out IZfsCommandRunner? zfsCommandRunner ) )
        {
            return null;
        }

        SiazService service = new( settings!, zfsCommandRunner, ServiceObserver, ServiceObserver );
        return service;
    }

    private static void GetZfsCommandRunner( SnapsInAZfsSettings settings, out IZfsCommandRunner zfsCommandRunner )
    {
        // This conditional is to avoid compiling the DummyZfsCommandRunner class if it isn't needed
    #if INCLUDE_DUMMY_ZFSCOMMANDRUNNER
        zfsCommandRunner = Environment.OSVersion.Platform switch
        {
            PlatformID.Unix => new ZfsCommandRunner( settings.ZfsPath, settings.ZpoolPath ),
            _ => new DummyZfsCommandRunner( settings.ZfsPath, settings.ZpoolPath )
        };
    #else
            zfsCommandRunner = new ZfsCommandRunner( settings!.ZfsPath, settings.ZpoolPath );
    #endif
    }

    private static void SetCommandLineLoggingOverride( CommandLineArguments args )
    {
        if ( args.Debug )
        {
            LoggingSettings.OverrideConsoleLoggingLevel( LogLevel.Debug );
        }

        if ( args.Quiet )
        {
            LoggingSettings.OverrideConsoleLoggingLevel( LogLevel.Warn );
        }

        if ( args.ReallyQuiet )
        {
            LoggingSettings.OverrideConsoleLoggingLevel( LogLevel.Off );
        }

        if ( args.Trace )
        {
            LoggingSettings.OverrideConsoleLoggingLevel( LogLevel.Trace );
        }

        if ( args.Verbose )
        {
            LoggingSettings.OverrideConsoleLoggingLevel( LogLevel.Info );
        }
    }
}
