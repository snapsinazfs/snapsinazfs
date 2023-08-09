#region MIT LICENSE

// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/

#endregion

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using NLog.Config;
using NLog.Extensions.Logging;
using PowerArgs;
using SnapsInAZfs.Interop.Libc.Enums;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Monitoring;
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
    private static IConfigurationRoot? _configurationRoot;
    internal static readonly IMonitor ServiceObserver = new Monitor( );
    internal static SnapsInAZfsSettings? Settings;

    internal static IZfsCommandRunner? ZfsCommandRunnerSingleton;

    [ExcludeFromCodeCoverage( Justification = "Largely un-testable" )]
    public static async Task<int> Main( string[] argv )
    {
        CommandLineArguments? args = await Args.ParseAsync<CommandLineArguments>( argv ).ConfigureAwait( true );

        // The nullability context in PowerArgs is wrong, so this absolutely can be null
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if ( args is null || args.Help )
        {
            LogManager.Shutdown( );
            return (int)Errno.ECANCELED;
        }

        if ( !LoadConfigurationFromConfigurationFiles( ref Settings, out _configurationRoot, in args ) )
        {
            LogManager.Shutdown( );
            return (int)Errno.EFTYPE;
        }

        SetCommandLineLoggingOverride( args );

        if ( args.Version )
        {
            // ReSharper disable once ExceptionNotDocumented
            string versionString = $"SnapsInAZfs Version: {Assembly.GetEntryAssembly( )?.GetCustomAttribute<AssemblyInformationalVersionAttribute>( )?.InformationalVersion}";
            Console.WriteLine( versionString );
            Logger.Debug( versionString );
            Logger.Trace( "Version argument provided. Exiting." );
            LogManager.Shutdown( );
            return (int)Errno.ECANCELED;
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
                LogManager.Shutdown( );
                return (int)Errno.GenericError;
            }

            LogManager.Shutdown( );
            return 0;
        }

        SiazService.Timestamp = DateTimeOffset.Now;
        SiazService? serviceInstance = null;
        try
        {
            serviceInstance = GetSiazServiceInstance( Settings );
            if ( serviceInstance is null )
            {
                Logger.Fatal( "Failed to create service instance - exiting" );
                LogManager.Shutdown( );
                return (int)Errno.ENOATTR;
            }

            WebApplicationBuilder serviceBuilder = WebApplication.CreateBuilder( );

            // Disposal happens after service shutdown, so this inspection can be ignored here
            // ReSharper disable once AccessToDisposedClosure
            serviceBuilder.Host
                          .UseSystemd( )
                          .ConfigureServices( ( _, services ) => { services.AddHostedService( _ => serviceInstance ); } );
            WebApplication svc;
            if ( Settings.Monitoring.Enabled )
            {
                serviceBuilder.WebHost
                              .UseKestrel( static ( _, kestrelOptions ) =>
                              {
                                  kestrelOptions.Configure( _configurationRoot
                                                            .GetRequiredSection( "Monitoring" )
                                                            .GetSection( "Kestrel" ) )
                                                .Load( );
                              } );
                svc = serviceBuilder.Build( );

                RouteGroupBuilder statusGroup = svc.MapGroup( "/" );
                statusGroup.MapGet( "/", ServiceObserver.GetApplicationState );
                statusGroup.MapGet( "/state", ServiceObserver.GetApplicationState );
                statusGroup.MapGet( "/fullstate", ServiceObserver.GetFullApplicationState );
                statusGroup.MapGet( "/workingset", ServiceObserver.GetWorkingSet );
                statusGroup.MapGet( "/version", ServiceObserver.GetVersion );
                statusGroup.MapGet( "/servicestarttime", ServiceObserver.GetServiceStartTime );

                RouteGroupBuilder snapshotsGroup = svc.MapGroup( "/snapshots" );
                snapshotsGroup.MapGet( "/", ServiceObserver.GetAllCounts );
                snapshotsGroup.MapGet( "/allcounts", ServiceObserver.GetAllCounts );
                snapshotsGroup.MapGet( "/takensucceededsincestart", ServiceObserver.GetSnapshotsTakenSucceededSinceStart );
                snapshotsGroup.MapGet( "/prunedsucceededsincestart", ServiceObserver.GetSnapshotsPrunedSucceededSinceStart );
                snapshotsGroup.MapGet( "/takenfailedsincestart", ServiceObserver.GetSnapshotsTakenFailedSinceStart );
                snapshotsGroup.MapGet( "/prunedfailedsincestart", ServiceObserver.GetSnapshotsPrunedFailedSinceStart );
                snapshotsGroup.MapGet( "/takensucceededlastrun", ServiceObserver.GetSnapshotsTakenSucceededLastRun );
                snapshotsGroup.MapGet( "/prunedsucceededlastrun", ServiceObserver.GetSnapshotsPrunedSucceededLastRun );
                snapshotsGroup.MapGet( "/takenfailedlastrun", ServiceObserver.GetSnapshotsTakenFailedLastRun );
                snapshotsGroup.MapGet( "/prunedfailedlastrun", ServiceObserver.GetSnapshotsPrunedFailedLastRun );
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
        programSettings.Monitoring.Enabled = ( programSettings.Monitoring.Enabled || args.Monitor ) && args is { NoMonitor: false, ConfigConsole: false };
        if ( args.DaemonTimerInterval > 0 )
        {
            programSettings.DaemonTimerIntervalSeconds = Math.Clamp( args.DaemonTimerInterval, 1u, 60u );
        }
    }

    internal static void SetCommandLineLoggingOverride( CommandLineArguments args )
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

    private static SiazService? GetSiazServiceInstance( SnapsInAZfsSettings settings )
    {
        if ( !TryGetZfsCommandRunner( settings, out IZfsCommandRunner? zfsCommandRunner ) )
        {
            return null;
        }

        SiazService service = new( settings, zfsCommandRunner, ServiceObserver, ServiceObserver );
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

    private static bool LoadConfigurationFromConfigurationFiles( [NotNullWhen( true )] ref SnapsInAZfsSettings? settings, [NotNullWhen( true )] out IConfigurationRoot? rootConfiguration, in CommandLineArguments args )
    {
        // Configuration is built in the following order from various sources.
        // Configurations from all sources are merged, and the final configuration that will be used is the result of the merged configurations.
        // If conflicting items exist in multiple configuration sources, the configuration of the configuration source added latest will
        // override earlier values.
        // Note that nlog-specific configuration is separate, in SnapsInAZfs.nlog.json, and is not affected by the configuration specified below,
        // and is loaded/parsed FIRST, before any configuration specified below.
        // See the SnapsInAZfs.Settings.Logging.LoggingSettings class for nlog configuration details.
        // See snapsinazfs(5) for detailed configuration documentation.
        // Configuration order, if not overridden by command-line options:
        // 1. /usr/local/share/SnapsInAZfs/SnapsInAZfs.json   #(Required - Base configuration - Should not be modified by the user)
        // 2. /etc/SnapsInAZfs/SnapsInAZfs.local.json
        // 6. Command-line arguments passed on invocation of SnapsInAZfs
        Logger.Debug( "Getting base configuration from files" );
        ConfigurationBuilder configBuilder = new( );

        IEnumerable<string> requestedFiles = args.ConfigFiles.Length > 0 ? args.ConfigFiles : new[] { "/usr/local/share/SnapsInAZfs/SnapsInAZfs.json", "/usr/local/share/SnapsInAZfs/SnapsInAZfs.nlog.json", "/etc/SnapsInAZfs/SnapsInAZfs.local.json", "/etc/SnapsInAZfs/SnapsInAZfs.nlog.json", "SnapsInAZfs.json", "SnapsInAZfs.local.json", "SnapsInAZfs.nlog.json" };
        foreach ( string filePath in requestedFiles )
        {
            if ( !File.Exists( filePath ) )
            {
                Logger.Debug( "Configuration file not found at {0}", filePath );
                continue;
            }

            Logger.Trace( "Loading configuration file {0}", filePath );
            configBuilder.AddJsonFile( filePath, false, false );
        }

        if ( configBuilder.Sources.Count == 0 )
        {
            Logger.Fatal( "Configuration files not found at any of these locations: {0}", requestedFiles.ToCommaSeparatedSingleLineString( true ) );
            rootConfiguration = null;
            return false;
        }

        rootConfiguration = configBuilder.Build( );

        Logger.Trace( "Building settings objects from IConfiguration" );
        try
        {
            settings = rootConfiguration.Get<SnapsInAZfsSettings>( ) ?? throw new InvalidOperationException( );
            IConfigurationSection nlogConfigSection = rootConfiguration.GetSection( "NLog" );
            LogManager.Configuration = nlogConfigSection.Exists( ) ? new NLogLoggingConfiguration( nlogConfigSection ) : new LoggingConfiguration( );
        }
        catch ( Exception ex )
        {
            Logger.Fatal( ex, "Unable to parse settings from JSON" );
            return false;
        }

        return true;
    }
}
