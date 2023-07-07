// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PowerArgs;
using SnapsInAZfs.Interop.Libc.Enums;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
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
    public static void ApplyCommandLineArgumentOverrides( in CommandLineArguments args )
    {
        Logger.Debug( "Overriding settings using arguments from command line." );

        Settings!.DryRun |= args.DryRun;
        Settings.TakeSnapshots = ( Settings.TakeSnapshots | args.TakeSnapshots ) & !args.NoTakeSnapshots;
        Settings.PruneSnapshots = ( Settings.PruneSnapshots | args.PruneSnapshots ) & !args.NoPruneSnapshots;
        Settings.Daemonize |= args.Daemonize;
    }

    public static async Task<int> ExecuteSiazAsync( IZfsCommandRunner zfsCommandRunner, CommandLineArguments args, DateTimeOffset currentTimestamp, CancellationToken cancellationToken )
    {
        if ( cancellationToken.IsCancellationRequested )
        {
            return (int)Errno.ECANCELED;
        }

        Logger.Debug( "Using Settings: {0}", JsonSerializer.Serialize( Settings ) );

        if ( cancellationToken.IsCancellationRequested )
        {
            return (int)Errno.ECANCELED;
        }

        ZfsTasks.CheckZfsPropertiesSchemaResult schemaCheckResult = await ZfsTasks.CheckZfsPoolRootPropertiesSchemaAsync( zfsCommandRunner, args ).ConfigureAwait( true );

        Logger.Debug( "Result of schema check is: {0}", JsonSerializer.Serialize( schemaCheckResult ) );

        if ( cancellationToken.IsCancellationRequested )
        {
            return (int)Errno.ECANCELED;
        }

        // Check
        switch ( args )
        {
            case { CheckZfsProperties: true } when !schemaCheckResult.MissingPropertiesFound:
            {
                // Requested check and no properties were missing.
                // Return 0
                return (int)Errno.EOK;
            }
            case { CheckZfsProperties: true } when schemaCheckResult.MissingPropertiesFound:
            {
                // Requested check and some properties were missing.
                // Return ENOATTR (1093)
                return (int)Errno.ENOATTR;
            }
            case { CheckZfsProperties: false, PrepareZfsProperties: false } when schemaCheckResult.MissingPropertiesFound:
            {
                // Did not request check or update (normal run) but properties were missing.
                // Cannot safely do anything useful
                // Log a fatal error and exit with ENOATTR
                Logger.Fatal( "Missing properties were found in zfs. Cannot continue. Exiting" );
                return (int)Errno.ENOATTR;
            }
            case { PrepareZfsProperties: true }:
            {
                // Requested schema update
                // Run the update and return EOK or ENOATTR based on success of the updates
                return ZfsTasks.UpdateZfsDatasetSchema( Settings!.DryRun, schemaCheckResult.PoolRootsWithPropertyValidities, zfsCommandRunner )
                    ? (int)Errno.EOK
                    : (int)Errno.ENOATTR;
            }
        }

        if ( args.ConfigConsole )
        {
            ConfigConsole.ConfigConsole.RunConsoleInterface( zfsCommandRunner );
            return (int)Errno.EOK;
        }

        ConcurrentDictionary<string, ZfsRecord> datasets = new( );
        ConcurrentDictionary<string, Snapshot> snapshots = new( );

        if ( cancellationToken.IsCancellationRequested )
        {
            return (int)Errno.ECANCELED;
        }

        Logger.Debug( "Getting remaining datasets and all snapshots from ZFS" );

        await ZfsTasks.GetDatasetsAndSnapshotsFromZfsAsync( Settings!, zfsCommandRunner, datasets, snapshots ).ConfigureAwait( true );

        Logger.Debug( "Finished getting datasets and snapshots from ZFS" );

        if ( cancellationToken.IsCancellationRequested )
        {
            return (int)Errno.ECANCELED;
        }

        // Handle taking new snapshots, if requested
        if ( Settings is { TakeSnapshots: true } )
        {
            Logger.Debug( "TakeSnapshots is true. Taking configured snapshots using timestamp {0:O}", currentTimestamp );
            ZfsTasks.TakeAllConfiguredSnapshots( zfsCommandRunner, Settings, currentTimestamp, datasets, snapshots );
        }
        else
        {
            Logger.Info( "Not taking snapshots" );
        }

        if ( cancellationToken.IsCancellationRequested )
        {
            return (int)Errno.ECANCELED;
        }

        // Handle pruning old snapshots, if requested
        if ( Settings is { PruneSnapshots: true } )
        {
            Logger.Debug( "PruneSnapshots is true. Pruning configured snapshots" );
            await ZfsTasks.PruneAllConfiguredSnapshotsAsync( zfsCommandRunner, Settings, datasets ).ConfigureAwait( true );
        }
        else
        {
            Logger.Info( "Not pruning snapshots" );
        }

        return (int)Errno.EOK;
    }

    public static async Task<int> Main( string[] argv )
    {
        LoggingSettings.ConfigureLogger( );

        DateTimeOffset currentTimestamp = DateTimeOffset.Now;

        Logger.Trace( "Parsing command-line arguments" );
        CommandLineArguments? args = Args.Parse<CommandLineArguments>( argv );

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
            Logger.Info( Assembly.GetExecutingAssembly( ).GetName( ).Version!.ToString );
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

        IHost serviceHost = Host.CreateDefaultBuilder( )
                                .UseSystemd( )
                                .ConfigureServices( ( _, services ) => { services.AddHostedService( ServiceInstanceProvider ); } )
                                .Build( );

        SiazService.timestamp = currentTimestamp;

        await serviceHost.RunAsync( ).ConfigureAwait( true );

        serviceHost.Dispose( );

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
        IZfsCommandRunner zfsCommandRunner =new ZfsCommandRunner( Settings!.ZfsPath, Settings.ZpoolPath );
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
