// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using PowerArgs;
using SnapsInAZfs.Interop.Libc.Enums;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs;

/// <summary>
///     The service class for running everything but the configuration console
/// </summary>
public class SiazService : BackgroundService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );
    private readonly CommandLineArguments _commandLineArguments;
    private readonly TimeSpan _daemonTimerInterval;

    private readonly SnapsInAZfsSettings _settings;
    private readonly IZfsCommandRunner _zfsCommandRunner;
    private static DateTimeOffset _lastRunTime = DateTimeOffset.Now;
    private static DateTimeOffset _nextRunTime = DateTimeOffset.Now;

    /// <summary>
    ///     Creates a new instance of the service class
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="zfsCommandRunner"></param>
    public SiazService( SnapsInAZfsSettings settings, IZfsCommandRunner zfsCommandRunner )
    {
        _settings = settings;
        _zfsCommandRunner = zfsCommandRunner;
        _commandLineArguments = Args.GetAmbientArgs<CommandLineArguments>( );
        _daemonTimerInterval = TimeSpan.FromSeconds( Math.Min( 60, _settings.DaemonTimerIntervalSeconds ) );
    }

    /// <summary>
    ///     Gets the last exit code that was set by methods called by the service.
    /// </summary>
    public static int ExitStatus { get; private set; } = (int)Errno.EOK;

    internal static SiazExecutionResultCode LastExecutionResultCode = SiazExecutionResultCode.None;

    internal static DateTimeOffset Timestamp;

    /// <inheritdoc />
    protected override async Task ExecuteAsync( CancellationToken stoppingToken )
    {
        // As soon as the application has started, execute the main method
        // If we weren't asked to daemonize, return immediately after that
        // Otherwise, set a timer and start ticking
        using CancellationTokenSource tokenSource = CancellationTokenSource.CreateLinkedTokenSource( stoppingToken );
        CancellationToken serviceCancellationToken = tokenSource.Token;
        LastExecutionResultCode = await ExecuteSiazAsync( _zfsCommandRunner, _commandLineArguments, Timestamp, serviceCancellationToken ).ConfigureAwait( true );
        if ( !_settings.Daemonize || serviceCancellationToken.IsCancellationRequested || LastExecutionResultCode is not SiazExecutionResultCode.Completed )
        {
            Environment.Exit( ExitStatus );
            return;
        }

        _lastRunTime = DateTimeOffset.Now;
        int greatestCommonFrequentIntervalMinutes = GetGreatestCommonFrequentIntervalFactor( );
        SetNextRunTime( greatestCommonFrequentIntervalMinutes );

        Timestamp = DateTimeOffset.Now;
        TimeSpan timerInterval = GetNewTimerInterval( );
        const int maxDriftMilliseconds = 500;

        PeriodicTimer daemonRunTimer = new( timerInterval );
        try
        {
            while ( !serviceCancellationToken.IsCancellationRequested && await daemonRunTimer.WaitForNextTickAsync( serviceCancellationToken ).ConfigureAwait( true ) )
            {
                if ( timerInterval < _daemonTimerInterval )
                {
                    daemonRunTimer.Dispose( );
                    daemonRunTimer = new( _daemonTimerInterval );
                    timerInterval = _daemonTimerInterval;
                    Logger.Debug( "Clock corrected. Adjusting timer interval to {0:G}", timerInterval );
                }

                try
                {
                    Timestamp = DateTimeOffset.Now;
                    Logger.Trace( "Timer ticked at {0:O}", Timestamp );
                    if ( Timestamp.Millisecond > maxDriftMilliseconds )
                    {
                        timerInterval = GetNewTimerInterval( );
                        Logger.Debug( "Clock drifted beyond threshold. Adjusting timer interval to {0:G}", timerInterval );
                        daemonRunTimer.Dispose( );
                        try
                        {
                            daemonRunTimer = new( timerInterval );
                        }
                        catch ( ArgumentOutOfRangeException ex )
                        {
                            Logger.Error( ex, "Invalid timer period ({0:G}). Initializing timer with default period", timerInterval );
                            daemonRunTimer = new( _daemonTimerInterval );
                        }
                    }

                    // Get this on every tick, in case config has been updated since last run.
                    // .net has already updated it in memory, if it was changed, so this is quick.
                    greatestCommonFrequentIntervalMinutes = GetGreatestCommonFrequentIntervalFactor( );

                    if ( Timestamp >= _nextRunTime )
                    {
                        _lastRunTime = DateTimeOffset.Now;
                        Logger.Debug( "Running configured SIAZ operations at {0:O}", _lastRunTime );
                        SetNextRunTime( greatestCommonFrequentIntervalMinutes );

                        // Fire this off asynchronously
                        LastExecutionResultCode = await ExecuteSiazAsync( _zfsCommandRunner, _commandLineArguments, Timestamp, serviceCancellationToken ).ConfigureAwait( true );
                        if ( LastExecutionResultCode is SiazExecutionResultCode.CancelledByToken or SiazExecutionResultCode.ZfsPropertyCheck_MissingProperties_Fatal or SiazExecutionResultCode.ZfsPropertyUpdate_Failed )
                        {
                            await StopAsync( serviceCancellationToken ).ConfigureAwait( true );
                        }
                    }
                }
                catch ( TaskCanceledException ex )
                {
                    // This is expected.
                    // This is how .net signals termination of the worker.
                    Logger.Debug( ex, "Service shutting down" );
                }
            }
        }
        finally
        {
            daemonRunTimer.Dispose( );
        }
    }

    private int GetGreatestCommonFrequentIntervalFactor( )
    {
        return _settings.Templates.Values.Select( t => t.SnapshotTiming.FrequentPeriod ).ToImmutableSortedSet( ).GreatestCommonFactor( );
    }

    private TimeSpan GetNewTimerInterval( )
    {
        DateTimeOffset currentTimeTruncatedToTopOfCurrentHour = Timestamp.Subtract( new TimeSpan( 0, 0, Timestamp.Minute, Timestamp.Second, Timestamp.Millisecond, Timestamp.Microsecond ) );
        DateTimeOffset truncatedTimePlusInterval = currentTimeTruncatedToTopOfCurrentHour + _daemonTimerInterval;
        while ( truncatedTimePlusInterval < Timestamp.AddMilliseconds( 1 ) )
        {
            truncatedTimePlusInterval += _daemonTimerInterval;
        }

        return truncatedTimePlusInterval.Subtract( Timestamp );
    }

    private static void SetNextRunTime( int greatestCommonFrequentIntervalMinutes )
    {
        DateTimeOffset lastRunTimeSnappedToFrequentPeriod = Timestamp.Subtract( new TimeSpan( 0, 0, Timestamp.Minute % greatestCommonFrequentIntervalMinutes, Timestamp.Second, Timestamp.Millisecond, Timestamp.Microsecond ) );
        DateTimeOffset lastRunTimeSnappedToNextFrequentPeriod = lastRunTimeSnappedToFrequentPeriod.AddMinutes( greatestCommonFrequentIntervalMinutes );
        DateTimeOffset lastRunTimePlusFrequentInterval = _lastRunTime.AddMinutes( greatestCommonFrequentIntervalMinutes );
        _nextRunTime = new[] { lastRunTimePlusFrequentInterval, lastRunTimeSnappedToNextFrequentPeriod }.Min( );
    }

    internal async Task<SiazExecutionResultCode>ExecuteSiazAsync( IZfsCommandRunner zfsCommandRunner, CommandLineArguments args, DateTimeOffset currentTimestamp, CancellationToken cancellationToken )
    {
        using CancellationTokenSource tokenSource = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken );
        if ( cancellationToken.IsCancellationRequested )
        {
            return SiazExecutionResultCode.CancelledByToken;
        }

        Logger.Debug( "Using Settings: {0}", JsonSerializer.Serialize( _settings ) );

        if ( cancellationToken.IsCancellationRequested )
        {
            return SiazExecutionResultCode.CancelledByToken;
        }

        ZfsTasks.CheckZfsPropertiesSchemaResult schemaCheckResult = await ZfsTasks.CheckZfsPoolRootPropertiesSchemaAsync( zfsCommandRunner, args ).ConfigureAwait( true );

        Logger.Debug( "Result of schema check is: {0}", JsonSerializer.Serialize( schemaCheckResult ) );

        if ( cancellationToken.IsCancellationRequested )
        {
            return SiazExecutionResultCode.CancelledByToken;
        }

        // Check
        switch ( args )
        {
            case { CheckZfsProperties: true } when !schemaCheckResult.MissingPropertiesFound:
            {
                // Requested check and no properties were missing.
                // Return 0
                return SiazExecutionResultCode.ZfsPropertyCheck_AllPropertiesPresent;
            }
            case { CheckZfsProperties: true } when schemaCheckResult.MissingPropertiesFound:
            {
                // Requested check and some properties were missing.
                // Return ENOATTR (1093)
                return SiazExecutionResultCode.ZfsPropertyCheck_MissingProperties;
            }
            case { CheckZfsProperties: false, PrepareZfsProperties: false } when schemaCheckResult.MissingPropertiesFound:
            {
                // Did not request check or update (normal run) but properties were missing.
                // Cannot safely do anything useful
                // Log a fatal error and exit with ENOATTR
                Logger.Fatal( "Missing properties were found in zfs. Cannot continue. Exiting" );
                return SiazExecutionResultCode.ZfsPropertyCheck_MissingProperties_Fatal;
            }
            case { PrepareZfsProperties: true }:
            {
                // Requested schema update
                // Run the update and return EOK or ENOATTR based on success of the updates
                return ZfsTasks.UpdateZfsDatasetSchema( _settings.DryRun, schemaCheckResult.PoolRootsWithPropertyValidities, zfsCommandRunner )
                    ? SiazExecutionResultCode.ZfsPropertyUpdate_Succeeded
                    : SiazExecutionResultCode.ZfsPropertyUpdate_Failed;
            }
        }

        if ( args.ConfigConsole )
        {
            ConfigConsole.ConfigConsole.RunConsoleInterface( zfsCommandRunner );
            Environment.Exit( 0 );
            return SiazExecutionResultCode.ConfigConsole_CleanExit;
        }

        if ( cancellationToken.IsCancellationRequested )
        {
            return SiazExecutionResultCode.CancelledByToken;
        }

        ConcurrentDictionary<string, ZfsRecord> datasets = new( );
        ConcurrentDictionary<string, Snapshot> snapshots = new( );


        Logger.Debug( "Getting remaining datasets and all snapshots from ZFS" );

        await ZfsTasks.GetDatasetsAndSnapshotsFromZfsAsync( _settings, zfsCommandRunner, datasets, snapshots ).ConfigureAwait( true );

        Logger.Debug( "Finished getting datasets and snapshots from ZFS" );

        if ( cancellationToken.IsCancellationRequested )
        {
            return SiazExecutionResultCode.CancelledByToken;
        }

        // Handle taking new snapshots, if requested
        if ( _settings is { TakeSnapshots: true } )
        {
            Logger.Debug( "TakeSnapshots is true. Taking configured snapshots using timestamp {0:O}", currentTimestamp );
            ZfsTasks.TakeAllConfiguredSnapshots( zfsCommandRunner, _settings, currentTimestamp, datasets, snapshots );
        }
        else
        {
            Logger.Info( "Not taking snapshots" );
        }

        if ( cancellationToken.IsCancellationRequested )
        {
            return SiazExecutionResultCode.CancelledByToken;
        }

        // Handle pruning old snapshots, if requested
        if ( _settings is { PruneSnapshots: true } )
        {
            Logger.Debug( "PruneSnapshots is true. Pruning configured snapshots" );
            await ZfsTasks.PruneAllConfiguredSnapshotsAsync( zfsCommandRunner, _settings, datasets ).ConfigureAwait( true );
        }
        else
        {
            Logger.Info( "Not pruning snapshots" );
        }

        return SiazExecutionResultCode.Completed;
    }
}
