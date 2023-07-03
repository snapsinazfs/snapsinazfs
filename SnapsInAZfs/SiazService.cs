// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Collections.Immutable;
using Microsoft.Extensions.Hosting;
using PowerArgs;
using SnapsInAZfs.Interop.Libc.Enums;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
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

    internal static DateTimeOffset timestamp;

    /// <inheritdoc />
    protected override async Task ExecuteAsync( CancellationToken stoppingToken )
    {
        // As soon as the application has started, execute the main method
        // If we weren't asked to daemonize, return immediately after that
        // Otherwise, set a timer and start ticking
        ExitStatus = await Program.ExecuteSiaz( _zfsCommandRunner, _commandLineArguments, timestamp, stoppingToken ).ConfigureAwait( true );
        _lastRunTime = DateTimeOffset.Now;
        int greatestCommonFrequentIntervalMinutes = GetGreatestCommonFrequentIntervalFactor( );
        SetNextRunTime( greatestCommonFrequentIntervalMinutes );
        if ( !_settings.Daemonize )
        {
            Environment.Exit( ExitStatus );
            return;
        }

        timestamp = DateTimeOffset.Now;
        TimeSpan timerInterval = GetNewTimerInterval( );
        const int maxDriftMilliseconds = 500;

        PeriodicTimer daemonRunTimer = new( timerInterval );
        try
        {
            while ( !stoppingToken.IsCancellationRequested && await daemonRunTimer.WaitForNextTickAsync( stoppingToken ).ConfigureAwait( true ) )
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
                    timestamp = DateTimeOffset.Now;
                    Logger.Trace( "Timer ticked at {0:O}", timestamp );
                    if ( timestamp.Millisecond > maxDriftMilliseconds )
                    {
                        timerInterval = GetNewTimerInterval( );
                        daemonRunTimer.Dispose( );
                        daemonRunTimer = new( timerInterval );
                        Logger.Debug( "Clock drifted beyond threshold. Adjusting timer interval to {0:G}", timerInterval );
                    }

                    // Get this on every tick, in case config has been updated since last run.
                    // .net has already updated it in memory, if it was changed, so this is quick.
                    greatestCommonFrequentIntervalMinutes = GetGreatestCommonFrequentIntervalFactor( );

                    if ( timestamp >= _nextRunTime )
                    {
                        Logger.Debug( "Running configured SIAZ operations at {0:O}", DateTimeOffset.Now );
                        _lastRunTime = DateTimeOffset.Now;
                        SetNextRunTime( greatestCommonFrequentIntervalMinutes );

                        // Fire this off asynchronously
                        ExitStatus = await Program.ExecuteSiaz( _zfsCommandRunner, _commandLineArguments, timestamp, stoppingToken ).ConfigureAwait( true );
                        if ( ExitStatus != 0 )
                        {
                            await StopAsync( stoppingToken ).ConfigureAwait( true );
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
        DateTimeOffset currentTimeTruncatedToTopOfCurrentHour = timestamp.Subtract( new TimeSpan( 0, 0, timestamp.Minute, timestamp.Second, timestamp.Millisecond, timestamp.Microsecond ) );
        DateTimeOffset truncatedTimePlusInterval = currentTimeTruncatedToTopOfCurrentHour + _daemonTimerInterval;
        while ( truncatedTimePlusInterval < timestamp )
        {
            truncatedTimePlusInterval += _daemonTimerInterval;
        }

        return truncatedTimePlusInterval.Subtract( timestamp );
    }

    private static void SetNextRunTime( int greatestCommonFrequentIntervalMinutes )
    {
        DateTimeOffset lastRunTimeSnappedToFrequentPeriod = timestamp.Subtract( new TimeSpan( 0, 0, timestamp.Minute % greatestCommonFrequentIntervalMinutes, timestamp.Second, timestamp.Millisecond, timestamp.Microsecond ) );
        DateTimeOffset lastRunTimeSnappedToNextFrequentPeriod = lastRunTimeSnappedToFrequentPeriod.AddMinutes( greatestCommonFrequentIntervalMinutes );
        DateTimeOffset lastRunTimePlusFrequentInterval = _lastRunTime.AddMinutes( greatestCommonFrequentIntervalMinutes );
        _nextRunTime = new[] { lastRunTimePlusFrequentInterval, lastRunTimeSnappedToNextFrequentPeriod }.Min( );
    }
}
