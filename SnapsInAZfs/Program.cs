// LICENSE:
// 
// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PowerArgs;
using SnapsInAZfs.Interop.Libc.Enums;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Settings.Logging;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs;

[UsedImplicitly]
internal class Program
{
    // Note that logging will be at whatever level is defined in SnapsInAZfs.nlog.json until configuration is initialized, regardless of command-line parameters.
    // Desired logging parameters should be set in SnapsInAZfs.nlog.json
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );
    internal static SnapsInAZfsSettings? Settings;

    /// <summary>
    ///     Overrides configuration values specified in configuration files or environment variables with arguments supplied on
    ///     the CLI
    /// </summary>
    /// <param name="args"></param>
    private static void ApplyCommandLineArgumentOverrides( in CommandLineArguments args )
    {
        Logger.Debug( "Overriding settings using arguments from command line." );

        Settings!.DryRun |= args.DryRun;
        Settings.TakeSnapshots = ( Settings.TakeSnapshots || args.TakeSnapshots ) && !args.NoTakeSnapshots;
        Settings.PruneSnapshots = ( Settings.PruneSnapshots || args.PruneSnapshots ) && !args.NoPruneSnapshots;
        Settings.Daemonize = ( Settings.Daemonize || args.Daemonize ) && args is { NoDaemonize: false, ConfigConsole: false };
        if ( args.DaemonTimerInterval > 0 )
        {
            Settings.DaemonTimerIntervalSeconds = Math.Clamp( (uint)args.DaemonTimerInterval, 1u, 60u );
        }
    }

    public static async Task<int> Main( string[] argv )
    {
        LoggingSettings.ConfigureLogger( );

        DateTimeOffset currentTimestamp = DateTimeOffset.Now;

        Logger.Trace( "Parsing command-line arguments" );
        CommandLineArguments? args = await Args.ParseAsync<CommandLineArguments>( argv ).ConfigureAwait( true );

        // The nullability context in PowerArgs is wrong, so this absolutely can be null
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if ( args is null )
        {
            Logger.Trace( "Help argument provided. Exiting." );
            return (int)Errno.ECANCELED;
        }

        if ( args.Help )
        {
            Logger.Trace( "Help argument provided. Exiting." );
            return (int)Errno.ECANCELED;
        }

        if ( args.Version )
        {
            string versionString = $"SnapsInAZfs Version: { Assembly.GetExecutingAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}";
            Console.WriteLine( versionString );
            Logger.Debug( versionString );
            Logger.Trace( "Version argument provided. Exiting." );
            return (int)Errno.ECANCELED;
        }

        SetCommandLineLoggingOverride( args );

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
                                           #if WINDOWS
                                               .AddJsonFile( "SnapsInAZfs.json", true, false )
                                               .AddJsonFile( "SnapsInAZfs.local.json", true, true )
                                           #else
                                               .AddJsonFile( "/usr/local/share/SnapsInAZfs/SnapsInAZfs.json", true, false )
                                               .AddJsonFile( "/etc/SnapsInAZfs/SnapsInAZfs.local.json", true, true )
                                           #endif
                                           #if ALLOW_ADJACENT_CONFIG_FILE
                                               .AddJsonFile( "SnapsInAZfs.local.json", true, false )
                                           #endif
                                               .Build( );

        Logger.Trace( "Building settings objects from IConfiguration" );
        try
        {
            Settings = rootConfiguration.Get<SnapsInAZfsSettings>( ) ?? throw new InvalidOperationException( );
        }
        catch ( Exception ex )
        {
            Logger.Fatal( ex, "Unable to parse settings from JSON" );
            return (int)Errno.EFTYPE;
        }

        ApplyCommandLineArgumentOverrides( in args );

        using IHost serviceHost = Host.CreateDefaultBuilder( )
                                .UseSystemd( )
                                .ConfigureServices( ( _, services ) => { services.AddHostedService( ServiceInstanceProvider ); } )
                                .Build( );

        SiazService.Timestamp = currentTimestamp;
        using CancellationTokenSource tokenSource = new( );
        CancellationToken masterToken = tokenSource.Token;

        await serviceHost.RunAsync( masterToken ).ConfigureAwait( true );

        return SiazService.ExitStatus;
    }

    private static SiazService ServiceInstanceProvider( IServiceProvider arg )
    {
        Logger.Trace( "Getting ZFS command runner for the current environment" );
        #if DEBUG_WINDOWS
        IZfsCommandRunner zfsCommandRunner = Environment.OSVersion.Platform switch
        {
            PlatformID.Unix => new ZfsCommandRunner( Settings!.ZfsPath, Settings.ZpoolPath ),
            _ => new DummyZfsCommandRunner( )
        };
        #else
        IZfsCommandRunner zfsCommandRunner = new ZfsCommandRunner( Settings!.ZfsPath, Settings.ZpoolPath );
        #endif

        SiazService service = new( Settings!, zfsCommandRunner );
        return service;
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
    }
}
