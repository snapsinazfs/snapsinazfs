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

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.Json;
using PowerArgs;
using SnapsInAZfs.Interop.Libc.Enums;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Monitoring;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs;

/// <summary>
///     The service class for running everything but the configuration console
/// </summary>
public sealed class SiazService : BackgroundService, IApplicationStateObservable, ISnapshotOperationsObservable
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );
    private static readonly AutoResetEvent SnapshotAutoResetEvent = new( true );
    private readonly CommandLineArguments _commandLineArguments;
    private readonly TimeSpan _daemonTimerInterval;

    private readonly SnapsInAZfsSettings _settings;
    private readonly IZfsCommandRunner _zfsCommandRunner;

    /// <summary>
    ///     Creates a new instance of the service class, without monitoring
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="zfsCommandRunner"></param>
    public SiazService( SnapsInAZfsSettings settings, IZfsCommandRunner zfsCommandRunner )
    {
        _settings = settings;
        _zfsCommandRunner = zfsCommandRunner;
        _commandLineArguments = Args.GetAmbientArgs<CommandLineArguments>( );
        _settings.DaemonTimerIntervalSeconds = Math.Clamp( _settings.DaemonTimerIntervalSeconds, 1, 60 );

        // The clamp immediately above this makes this exception not possible
        // ReSharper disable once ExceptionNotDocumented
        _daemonTimerInterval = TimeSpan.FromSeconds( _settings.DaemonTimerIntervalSeconds );
    }

    /// <summary>
    ///     Creates a new instance of the service class, with monitoring
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="zfsCommandRunner"></param>
    /// <param name="applicationStateObserver"></param>
    /// <param name="snapshotOperationsObserver"></param>
    public SiazService( SnapsInAZfsSettings settings, IZfsCommandRunner zfsCommandRunner, IApplicationStateObserver applicationStateObserver, ISnapshotOperationsObserver snapshotOperationsObserver )
    {
        _settings = settings;
        _zfsCommandRunner = zfsCommandRunner;
        _commandLineArguments = Args.GetAmbientArgs<CommandLineArguments>( );
        _settings.DaemonTimerIntervalSeconds = Math.Clamp( _settings.DaemonTimerIntervalSeconds, 1, 60 );

        // The clamp immediately above this makes this exception not possible
        // ReSharper disable once ExceptionNotDocumented
        _daemonTimerInterval = TimeSpan.FromSeconds( _settings.DaemonTimerIntervalSeconds );
        _stateObserver = applicationStateObserver;
        _snapshotOperationsObserver = snapshotOperationsObserver;
        _stateObserver.RegisterApplicationStateObservable( this );
        _snapshotOperationsObserver.RegisterSnapshotOperationsObservable( this );
    }

    private DateTimeOffset _lastRunTime = DateTimeOffset.Now;
    private DateTimeOffset _nextRunTime = DateTimeOffset.Now;

    private ApplicationState _state = ApplicationState.Init;

    /// <summary>
    ///     Gets the last exit code that was set by methods called by the service.
    /// </summary>
    public static int ExitStatus { get; } = (int)Errno.EOK;

    internal static SiazExecutionResultCode LastExecutionResultCode = SiazExecutionResultCode.None;

    internal static DateTimeOffset Timestamp;

    /// <exception cref="Exception" accessor="set">A delegate callback throws an exception.</exception>
    public ApplicationState State
    {
        get => _state;
        private set
        {
            if ( _state != value )
            {
                ApplicationStateChanged?.Invoke( this, new( _state, value ) );
            }

            _state = value;
        }
    }

    /// <inheritdoc />
    public DateTimeOffset ServiceStartTime { get; } = DateTimeOffset.Now;

    public event EventHandler<ApplicationStateChangedEventArgs>? ApplicationStateChanged;

    /// <inheritdoc />
    public event EventHandler<long>? NextRunTimeChanged;

    /// <inheritdoc />
    public event EventHandler<DateTimeOffset>? BeginPruningSnapshots;

    /// <inheritdoc />
    public event EventHandler<DateTimeOffset>? BeginTakingSnapshots;

    /// <inheritdoc />
    public event EventHandler<DateTimeOffset>? EndPruningSnapshots;

    /// <inheritdoc />
    public event EventHandler<DateTimeOffset>? EndTakingSnapshots;

    /// <inheritdoc />
    public event EventHandler<SnapshotOperationEventArgs>? PruneSnapshotFailed;

    /// <inheritdoc />
    public event EventHandler<SnapshotOperationEventArgs>? PruneSnapshotSucceeded;

    /// <inheritdoc />
    public event EventHandler<SnapshotOperationEventArgs>? TakeSnapshotFailed;

    /// <inheritdoc />
    public event EventHandler<SnapshotOperationEventArgs>? TakeSnapshotSucceeded;

    private static async Task<CheckZfsPropertiesSchemaResult> CheckZfsPoolRootPropertiesSchemaAsync( IZfsCommandRunner zfsCommandRunner, CommandLineArguments args )
    {
        Logger.Debug( "Checking zfs properties schema" );

        ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> poolRootsWithPropertyValidities = await zfsCommandRunner.GetPoolRootsAndPropertyValiditiesAsync( ).ConfigureAwait( false );
        bool missingPropertiesFound = false;
        foreach ( ( string poolName, ConcurrentDictionary<string, bool>? propertyValidities ) in poolRootsWithPropertyValidities )
        {
            Logger.Debug( "Checking property validities for pool root {0}", poolName );
            bool missingPropertiesFoundForPool = false;
            foreach ( ( string propName, bool propValue ) in propertyValidities )
            {
                if ( !IZfsProperty.DefaultDatasetProperties.ContainsKey( propName ) )
                {
                #if DEBUG
                    Logger.Trace( "Not interested in property {0} for pool root schema check", propName );
                #endif
                    continue;
                }

            #if DEBUG
                Logger.Trace( "Checking validity of property {0} in pool root {1}", propName, poolName );
            #endif
                if ( propValue )
                {
                #if DEBUG
                    Logger.Trace( "Pool root {0} has property {1} with a valid value", poolName, propName );
                #endif
                    continue;
                }

                Logger.Debug( "Pool root {0} has missing or invalid property {1}", poolName, propName );
                if ( !missingPropertiesFoundForPool )
                {
                    missingPropertiesFound = missingPropertiesFoundForPool = true;
                }
            }

            foreach ( ( string propName, _ ) in IZfsProperty.DefaultDatasetProperties )
            {
                if ( !propName.StartsWith( ZfsPropertyNames.SiazNamespace ) || propertyValidities.ContainsKey( propName ) )
                {
                    continue;
                }

                propertyValidities.TryAdd( propName, false );
                missingPropertiesFound = missingPropertiesFoundForPool = true;
            }

            Logger.Debug( "Finished checking property validities for pool root {0}", poolName );

            switch ( args )
            {
                case { CheckZfsProperties: true } when missingPropertiesFoundForPool:
                    Logger.Warn( "Pool {0} is missing the following properties: {1}", poolName, propertyValidities.Where( static kvp => !kvp.Value ).KeysToCommaSeparatedSingleLineString( true ) );
                    continue;
                case { CheckZfsProperties: true } when !missingPropertiesFoundForPool:
                    Logger.Info( "No missing properties in pool {0}", poolName );
                    continue;
                case { PrepareZfsProperties: true } when missingPropertiesFoundForPool:
                    Logger.Info( "Pool {0} is missing the following properties: {1}", poolName, propertyValidities.Where( static kvp => !kvp.Value ).KeysToCommaSeparatedSingleLineString( true ) );
                    continue;
                case { PrepareZfsProperties: true } when !missingPropertiesFoundForPool:
                    Logger.Info( "No missing properties in pool {0}", poolName );
                    continue;
                case { PrepareZfsProperties: false, CheckZfsProperties: false } when missingPropertiesFoundForPool:
                    Logger.Fatal( "Pool {0} is missing the following properties: {1}", poolName, propertyValidities.Where( static kvp => !kvp.Value ).KeysToCommaSeparatedSingleLineString( true ) );
                    continue;
                case { PrepareZfsProperties: false, CheckZfsProperties: false } when !missingPropertiesFoundForPool:
                    Logger.Debug( "No missing properties in pool {0}", poolName );
                    continue;
            }
        }

        return new( poolRootsWithPropertyValidities, missingPropertiesFound );
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync( CancellationToken stoppingToken )
    {
        State = ApplicationState.Idle;
        // As soon as the application has started, execute the main method
        // If we weren't asked to daemonize, return immediately after that
        // Otherwise, set a timer and start ticking
        using CancellationTokenSource tokenSource = CancellationTokenSource.CreateLinkedTokenSource( stoppingToken );
        CancellationToken serviceCancellationToken = tokenSource.Token;

        // Run once, unconditionally
        // Afterward, only continue if we're running as a daemon, stop hasn't been requested, and there wasn't an error.
        LastExecutionResultCode = await ExecuteSiazAsync( _zfsCommandRunner, _commandLineArguments, Timestamp, serviceCancellationToken ).ConfigureAwait( true );
        if ( !_settings.Daemonize || serviceCancellationToken.IsCancellationRequested || LastExecutionResultCode is not SiazExecutionResultCode.Completed )
        {
            State = ApplicationState.Terminating;
            serviceCancellationToken.ThrowIfCancellationRequested( );
            Environment.Exit( ExitStatus );
            return;
        }

        // We're running as a daemon.
        // Set up the timer and prepare to loop infinitely,
        // using that timer, until stop is requested or something bad happens.
        _lastRunTime = DateTimeOffset.Now;
        int greatestCommonFrequentIntervalMinutes = GetGreatestCommonFrequentIntervalFactor( _settings.Templates );

        // The timer aliases its run time to the top of the calculated minute,
        // so the first run time on the timer will almost always be less than the configured interval.
        // This is fine, as no action will be taken unless it actually is time to do so.
        // All subsequent runs will be on the greatest common factor of all configured frequent intervals in all templates.

        SetNextRunTime( in greatestCommonFrequentIntervalMinutes, in Timestamp, in _lastRunTime, out _nextRunTime );

        Timestamp = DateTimeOffset.Now;
        GetNewTimerInterval( in Timestamp, in _daemonTimerInterval, out TimeSpan timerInterval, out DateTimeOffset expectedTickTimestamp );
        const int maxDriftMilliseconds = 300;

        PeriodicTimer daemonRunTimer = new( timerInterval );
        try
        {
            while ( !serviceCancellationToken.IsCancellationRequested && await daemonRunTimer.WaitForNextTickAsync( serviceCancellationToken ).ConfigureAwait( true ) )
            {
                TimeSpan differenceBetweenCurrentAndConfiguredInterval = timerInterval.Subtract( _daemonTimerInterval ).Duration( );
                if ( differenceBetweenCurrentAndConfiguredInterval.TotalMilliseconds > maxDriftMilliseconds )
                {
                #if DEBUG
                    Logger.Debug( "Restarting timer after adjustment - Old interval: {0:G}, New interval: {1:G}", timerInterval, _daemonTimerInterval );
                #endif
                    daemonRunTimer.Dispose( );
                    daemonRunTimer = new( _daemonTimerInterval );
                    timerInterval = _daemonTimerInterval;
                }

                try
                {
                    Timestamp = DateTimeOffset.Now;
                #if DEBUG
                    Logger.Debug( "Timer ticked at {0:O} - Interval: {1:G}", Timestamp, timerInterval );
                #endif
                    TimeSpan drift = ( Timestamp - expectedTickTimestamp ).Duration( );
                    GetNextTickTimestamp( in Timestamp, in _daemonTimerInterval, out expectedTickTimestamp );
                    if ( drift.TotalMilliseconds > maxDriftMilliseconds )
                    {
                        GetNewTimerInterval( in Timestamp, in _daemonTimerInterval, out timerInterval, out expectedTickTimestamp );
                        Logger.Debug( "Clock drifted beyond threshold (drift: {0:G}). Adjusting timer interval to {1:G}", drift, timerInterval );
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
                    greatestCommonFrequentIntervalMinutes = GetGreatestCommonFrequentIntervalFactor( _settings.Templates );

                    if ( Timestamp >= _nextRunTime )
                    {
                        _lastRunTime = DateTimeOffset.Now;
                        Logger.Debug( "Running configured SIAZ operations at {0:O}", _lastRunTime );
                        SetNextRunTime( in greatestCommonFrequentIntervalMinutes, in Timestamp, in _lastRunTime, out _nextRunTime );

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
            State = ApplicationState.Terminating;
            daemonRunTimer.Dispose( );
        }
    }

    internal static int GetGreatestCommonFrequentIntervalFactor( Dictionary<string, TemplateSettings> templates )
    {
        return templates.Values.Select( static t => t.SnapshotTiming.FrequentPeriod ).ToImmutableSortedSet( ).GreatestCommonFactor( );
    }

    internal static void GetNewTimerInterval( in DateTimeOffset timestamp, in TimeSpan configuredTimerInterval, out TimeSpan calculatedTimerInterval, out DateTimeOffset nextTickTimestamp )
    {
        GetNextTickTimestamp( in timestamp, in configuredTimerInterval, out nextTickTimestamp );
        calculatedTimerInterval = nextTickTimestamp - timestamp;
    }

    internal static void GetNextTickTimestamp( in DateTimeOffset timestamp, in TimeSpan configuredTimerInterval, out DateTimeOffset nextTickTimestamp )
    {
        DateTimeOffset currentTimeTruncatedToTopOfCurrentHour = timestamp.Subtract( new TimeSpan( 0, 0, timestamp.Minute, timestamp.Second, timestamp.Millisecond, timestamp.Microsecond ) );
        nextTickTimestamp = currentTimeTruncatedToTopOfCurrentHour + configuredTimerInterval;

        // This is the time equivalent, to millisecond precision, of offsetting a loop stop condition by 1
        // Without this, whole factors of 60 will result in an off-by-one error
        // DateTimeOffset supports even greater precision, but not all systems/clocks can handle that accurately,
        // and that would be overkill for this purpose anyway.
        while ( nextTickTimestamp < timestamp.AddMilliseconds( 1 ) )
        {
            nextTickTimestamp += configuredTimerInterval;
        }
    }

    /// <exception cref="Exception">A delegate callback throws an exception.</exception>
    internal async Task PruneAllConfiguredSnapshotsAsync( IZfsCommandRunner commandRunner, SnapsInAZfsSettings settings, ConcurrentDictionary<string, ZfsRecord> datasets )
    {
        if ( !SnapshotAutoResetEvent.WaitOne( 30000 ) )
        {
            Logger.Error( "Timed out waiting to prune snapshots. Another operation is in progress." );
            return;
        }

        Logger.Info( "Begin pruning snapshots for all configured datasets" );
        BeginPruningSnapshots?.Invoke( this, DateTimeOffset.Now );
        await Parallel.ForEachAsync( datasets.Values, new ParallelOptions { MaxDegreeOfParallelism = 4 }, async ( ds, _ ) => await PruneSnapshotsForDatasetAsync( ds ).ConfigureAwait( false ) ).ConfigureAwait( false );

        EndPruningSnapshots?.Invoke( this, DateTimeOffset.Now );
        Logger.Info( "Finished pruning snapshots" );
        SnapshotAutoResetEvent.Set( );

        return;

        async Task PruneSnapshotsForDatasetAsync( ZfsRecord ds )
        {
            if ( ds is not { Enabled.Value: true } )
            {
                Logger.Debug( "Dataset {0} is disabled - skipping prune", ds.Name );
                return;
            }

            if ( ds is not { PruneSnapshots.Value: true } )
            {
                Logger.Debug( "Dataset {0} not configured to prune snapshots - skipping", ds.Name );
                return;
            }

            List<Snapshot> snapshotsToPruneForDataset = ds.GetSnapshotsToPrune( );

            Logger.Debug( "Need to prune the following snapshots from {0}: {1}", ds.Name, snapshotsToPruneForDataset.ToCommaSeparatedSingleLineString( true ) );

            foreach ( Snapshot snapshot in snapshotsToPruneForDataset )
            {
                ZfsCommandRunnerOperationStatus destroyStatus = await commandRunner.DestroySnapshotAsync( snapshot, settings ).ConfigureAwait( false );
                switch ( destroyStatus )
                {
                    case ZfsCommandRunnerOperationStatus.DryRun:
                        Logger.Info( "DRY RUN: Snapshot not destroyed, but pretending it was for simulation" );
                        goto Remove;
                    case ZfsCommandRunnerOperationStatus.Success:
                        Logger.Info( "Destroyed snapshot {0}", snapshot.Name );
                    Remove:
                        if ( !ds.RemoveSnapshot( snapshot ) )
                        {
                            Logger.Debug( "Unable to remove snapshot {0} from {1} {2} object", snapshot.Name, ds.Kind, ds.Name );
                        }

                        PruneSnapshotSucceeded?.Invoke( this, new( snapshot.Name, snapshot.Timestamp.Value ) );
                        continue;
                    case ZfsCommandRunnerOperationStatus.ZfsProcessFailure:
                    case ZfsCommandRunnerOperationStatus.Failure:
                    case ZfsCommandRunnerOperationStatus.NameValidationFailed:
                    default:
                        PruneSnapshotFailed?.Invoke( this, new( ds.Name, snapshot.Timestamp.Value ) );
                        Logger.Error( "Failed to destroy snapshot {0}", snapshot.Name );
                        continue;
                }
            }
        }
    }

    /// <exception cref="InvalidOperationException">If an invalid value is returned when getting the mutex</exception>
    /// <exception cref="Exception">A delegate callback throws an exception.</exception>
    internal async Task TakeAllConfiguredSnapshotsAsync( DateTimeOffset timestamp, ConcurrentDictionary<string, ZfsRecord> datasets, ConcurrentDictionary<string, Snapshot> snapshots )
    {
        if ( !SnapshotAutoResetEvent.WaitOne( 30000 ) )
        {
            Logger.Error( "Timed out waiting to take snapshots. Another operation is in progress" );
            return;
        }

        Logger.Info( "Begin taking snapshots for all configured datasets" );
        BeginTakingSnapshots?.Invoke( this, DateTimeOffset.Now );
        State = ApplicationState.TakingSnapshots;
        //Need to operate on a sorted collection
        ImmutableSortedDictionary<string, ZfsRecord> sortedDatasets = datasets.ToImmutableSortedDictionary( );
        foreach ( ( string _, ZfsRecord ds ) in sortedDatasets )
        {
            //OK to disable this warning here. We don't use it if the result is false, and we don't put null in the collection in the first place
#pragma warning disable CS8600
            if ( !_settings.Templates.TryGetValue( ds.Template.Value, out TemplateSettings template ) )
            {
                Logger.Error( "Template {0} specified for {1} not found in configuration - skipping snapshots for {1}", ds.Template.Name, ds.Name );
                continue;
            }
#pragma warning restore CS8600

            List<IZfsProperty> propsToSet = new( );
            if ( ds is not { TakeSnapshots.Value: true } )
            {
                Logger.Debug( "Dataset {0} not configured to take snapshots - skipping", ds.Name );
                continue;
            }

            if ( ds is not { Enabled.Value: true } )
            {
                Logger.Debug( "Dataset {0} is disabled - skipping", ds.Name );
                continue;
            }

            Logger.Debug( "Checking for and taking needed snapshots for dataset {0}", ds.Name );

            if ( ds.IsFrequentSnapshotNeeded( template.SnapshotTiming, in timestamp ) )
            {
                Logger.Debug( "Frequent snapshot needed for dataset {0}", ds.Name );
                ( bool success, Snapshot? snapshot ) = TakeSnapshotKind( ds, SnapshotPeriod.Frequent, propsToSet );
                if ( success && snapshot is not null )
                {
                    snapshots[ snapshot.Name ] = snapshot;
                }
            }

            if ( ds.IsHourlySnapshotNeeded( in timestamp ) )
            {
                Logger.Debug( "Hourly snapshot needed for dataset {0}", ds.Name );
                ( bool success, Snapshot? snapshot ) = TakeSnapshotKind( ds, SnapshotPeriod.Hourly, propsToSet );
                if ( success && snapshot is not null )
                {
                    snapshots[ snapshot.Name ] = snapshot;
                }
            }

            if ( ds.IsDailySnapshotNeeded( in timestamp ) )
            {
                Logger.Debug( "Daily snapshot needed for dataset {0}", ds.Name );
                ( bool success, Snapshot? snapshot ) = TakeSnapshotKind( ds, SnapshotPeriod.Daily, propsToSet );
                if ( success && snapshot is not null )
                {
                    snapshots[ snapshot.Name ] = snapshot;
                }
            }

            if ( ds.IsWeeklySnapshotNeeded( template.SnapshotTiming, in timestamp ) )
            {
                Logger.Debug( "Weekly snapshot needed for dataset {0}", ds.Name );
                ( bool success, Snapshot? snapshot ) = TakeSnapshotKind( ds, SnapshotPeriod.Weekly, propsToSet );
                if ( success && snapshot is not null )
                {
                    snapshots[ snapshot.Name ] = snapshot;
                }
            }

            if ( ds.IsMonthlySnapshotNeeded( in timestamp ) )
            {
                Logger.Debug( "Monthly snapshot needed for dataset {0}", ds.Name );
                ( bool success, Snapshot? snapshot ) = TakeSnapshotKind( ds, SnapshotPeriod.Monthly, propsToSet );
                if ( success && snapshot is not null )
                {
                    snapshots[ snapshot.Name ] = snapshot;
                }
            }

            if ( ds.IsYearlySnapshotNeeded( in timestamp ) )
            {
                Logger.Debug( "Yearly snapshot needed for dataset {0}", ds.Name );
                ( bool success, Snapshot? snapshot ) = TakeSnapshotKind( ds, SnapshotPeriod.Yearly, propsToSet );
                if ( success && snapshot is not null )
                {
                    snapshots[ snapshot.Name ] = snapshot;
                }
            }

            if ( propsToSet.Any( ) )
            {
                Logger.Debug( "Took snapshots of {0}. Need to set properties: {1}", ds.Name, propsToSet.Select( static p => $"{p.Name}: {p.ValueString}" ).ToCommaSeparatedSingleLineString( ) );
                ZfsCommandRunnerOperationStatus setPropertiesResult = await _zfsCommandRunner.SetZfsPropertiesAsync( _settings.DryRun, ds.Name, propsToSet.ToArray( ) ).ConfigureAwait( true );
                switch ( setPropertiesResult )
                {
                    case ZfsCommandRunnerOperationStatus.Success:
                        Logger.Debug( "Property set successful" );
                        continue;
                    case ZfsCommandRunnerOperationStatus.DryRun:
                        Logger.Info( "DRY RUN: No properties were set on actual datasets" );
                        continue;
                    case ZfsCommandRunnerOperationStatus.ZeroLengthRequest:
                        Logger.Warn( "Set property request contained 0 elements for {0}", ds.Name );
                        continue;
                    default:
                        Logger.Error( "Error setting properties for dataset {0}", ds.Name );
                        continue;
                }
            }

            Logger.Debug( "No snapshots needed for dataset {0}", ds.Name );
        }

        EndTakingSnapshots?.Invoke( this, DateTimeOffset.Now );
        Logger.Info( "Finished taking snapshots" );

        SnapshotAutoResetEvent.Set( );

        return;

        (bool success, Snapshot? snapshot) TakeSnapshotKind( ZfsRecord ds, SnapshotPeriod period, List<IZfsProperty> propsToSet )
        {
            Logger.Trace( "Requested to take {0} snapshot of {1}", period, ds.Name );
            bool snapshotTaken = TakeSnapshot( ds, period, timestamp, out Snapshot? snapshot );
            switch ( snapshotTaken )
            {
                case true:
                    Logger.Trace( "{0} snapshot {1} taken successfully", period, snapshot?.Name ?? $"of {ds.Name}" );
                    propsToSet.Add( ds.UpdateProperty( period.GetMostRecentSnapshotZfsPropertyName( ), in timestamp ) );
                    return ( true, snapshot );
                case false when _settings.DryRun:
                    propsToSet.Add( ds.UpdateProperty( period.GetMostRecentSnapshotZfsPropertyName( ), in timestamp ) );
                    return ( true, null );
                default:
                    return ( false, null );
            }
        }
    }

    /// <exception cref="Exception">A delegate callback throws an exception.</exception>
    internal bool TakeSnapshot( ZfsRecord ds, SnapshotPeriod period, DateTimeOffset timestamp, out Snapshot? snapshot )
    {
        Logger.Trace( "TakeSnapshot called for {0} with period {1}", ds.Name, period );
        snapshot = null;

        switch ( ds )
        {
            case { Enabled.Value: false }:
                Logger.Trace( "Dataset {0} is not enabled. Skipping", ds.Name );
                return false;
            case { TakeSnapshots.Value: false }:
                Logger.Trace( "Dataset {0} is not configured to take snapshots. Skipping", ds.Name );
                return false;
            case { IsPoolRoot: false, Recursion.Value: ZfsPropertyValueConstants.ZfsRecursion, ParentDataset.Recursion.Value: ZfsPropertyValueConstants.ZfsRecursion }:
                Logger.Debug( "Ancestor {1} of dataset {0} is already configured for zfs native recursion. Skipping", ds.Name, ds.ParentDataset.Name );
                return false;
            case { IsPoolRoot: false, Recursion.Value: ZfsPropertyValueConstants.SnapsInAZfs, ParentDataset.Recursion.Value: ZfsPropertyValueConstants.ZfsRecursion }:
                Logger.Warn( "Ancestor {1} of dataset {0} is configured for zfs native recursion and local recursion is set. No new snapshot will be taken of {0} to avoid name collision. Check ZFS configuration", ds.Name, ds.ParentDataset.Name );
                return false;
        }

        Logger.Trace( "Looking up template {0} for {1} snapshot of {2}", ds.Template.Value, period, ds.Name );

        if ( !_settings.Templates.TryGetValue( ds.Template.Value, out TemplateSettings? template ) )
        {
            Logger.Error( "Template {0} for dataset {1} not found in configuration. Skipping", ds.Template.Value, ds.Name );
            return false;
        }

        switch ( period.Kind )
        {
            case SnapshotPeriodKind.Frequent:
                if ( ds.SnapshotRetentionFrequent.IsNotWanted( ) )
                {
                #if DEBUG
                    Logger.Trace( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", period, ds.Name );
                #endif
                    return false;
                }

                break;
            case SnapshotPeriodKind.Hourly:
                if ( ds.SnapshotRetentionHourly.IsNotWanted( ) )
                {
                #if DEBUG
                    Logger.Trace( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", period, ds.Name );
                #endif
                    return false;
                }

                break;
            case SnapshotPeriodKind.Daily:
                if ( ds.SnapshotRetentionDaily.IsNotWanted( ) )
                {
                #if DEBUG
                    Logger.Trace( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", period, ds.Name );
                #endif
                    return false;
                }

                break;
            case SnapshotPeriodKind.Weekly:
                if ( ds.SnapshotRetentionWeekly.IsNotWanted( ) )
                {
                #if DEBUG
                    Logger.Trace( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", period, ds.Name );
                 #endif
                   return false;
                }

                break;
            case SnapshotPeriodKind.Monthly:
                if ( ds.SnapshotRetentionMonthly.IsNotWanted( ) )
                {
                #if DEBUG
                    Logger.Trace( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", period, ds.Name );
                 #endif
                    return false;
                }

                break;
            case SnapshotPeriodKind.Yearly:
                if ( ds.SnapshotRetentionYearly.IsNotWanted( ) )
                {
                #if DEBUG
                    Logger.Trace( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", period, ds.Name );
                 #endif
                    return false;
                }

                break;
            case SnapshotPeriodKind.NotSet:
            default:
            {
                TakeSnapshotFailed?.Invoke( this, new( ds.Name, in timestamp ) );
                throw new ArgumentOutOfRangeException( nameof( period ), period, $"Unexpected value received for Period for dataset {ds.Name}. Snapshot not taken." );
            }
        }

        Logger.Trace( "{0} {1} will have a {2} snapshot taken with these settings: {3}", ds.Kind, ds.Name, period, JsonSerializer.Serialize( new { ds.Template, ds.Recursion } ) );

        ZfsCommandRunnerOperationStatus zfsCommandRunnerStatus = _zfsCommandRunner.TakeSnapshot( ds, period, in timestamp, _settings, template.Formatting, out snapshot );
        switch ( zfsCommandRunnerStatus )
        {
            case ZfsCommandRunnerOperationStatus.DryRun:
                ds.AddSnapshot( snapshot! );
                Logger.Info( "DRY RUN: Pretending snapshot {0} was successfully taken", snapshot!.Name );
                TakeSnapshotSucceeded?.Invoke( this, new( snapshot.Name, in timestamp ) );
                return false;
            case ZfsCommandRunnerOperationStatus.Success:
                ds.AddSnapshot( snapshot! );
                TakeSnapshotSucceeded?.Invoke( this, new( snapshot!.Name, in timestamp ) );
                Logger.Info( "Snapshot {0} successfully taken", snapshot!.Name );
                return true;
            default:
                TakeSnapshotFailed?.Invoke( this, new( ds.Name, in timestamp ) );
                Logger.Error( "{0} snapshot for {1} {2} not taken", period, ds.Kind, ds.Name );
                return false;
        }
    }

    /// <exception cref="Exception">A delegate callback throws an exception.</exception>
    internal bool UpdateZfsDatasetSchema( SnapsInAZfsSettings settings, ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> poolRootsWithPropertyValidities, IZfsCommandRunner zfsCommandRunner )
    {
        State = ApplicationState.UpdatingZfsPropertySchema;
        bool errorsEncountered = false;
        Logger.Debug( "Requested update of zfs properties schema" );
        foreach ( ( string poolName, ConcurrentDictionary<string, bool> propertiesToAdd ) in poolRootsWithPropertyValidities )
        {
            Logger.Info( "Updating properties for pool {0}", poolName );

            // It's not a nullable type...
            // ReSharper disable once ExceptionNotDocumentedOptional
            string[] propertyArray = propertiesToAdd.Where( static kvp => !kvp.Value ).Select( static kvp => kvp.Key ).ToArray( );

            if ( propertyArray.Length == 0 )
            {
                Logger.Info( "No missing properties to set for {0} - Skipping", poolName );
                continue;
            }

            // Attempt to set the missing properties for the pool.
            // Log an error if unsuccessful
            if ( zfsCommandRunner.SetDefaultValuesForMissingZfsPropertiesOnPoolAsync( settings, poolName, propertyArray ) )
            {
                Logger.Info( "Finished updating properties for pool {0}", poolName );
            }
            else
            {
                if ( settings.DryRun )
                {
                    Logger.Info( "DRY RUN: Properties intentionally not set for {0}: {1}", poolName, JsonSerializer.Serialize( propertyArray ) );
                }
                else
                {
                    errorsEncountered = true;
                    Logger.Error( "Failed updating properties for pool {0}. Unset properties: {1}", poolName, JsonSerializer.Serialize( propertyArray ) );
                }
            }
        }

        Logger.Debug( "Finished updating zfs properties schema for all pool roots" );
        if ( errorsEncountered )
        {
            Logger.Error( "Some operations failed. See previous log output." );
        }

        State = ApplicationState.UpdatingZfsPropertySchemaCompleted;
        return !errorsEncountered;
    }

    /// <exception cref="Exception">A delegate callback throws an exception.</exception>
    private async Task<SiazExecutionResultCode> ExecuteSiazAsync( IZfsCommandRunner zfsCommandRunner, CommandLineArguments args, DateTimeOffset currentTimestamp, CancellationToken cancellationToken )
    {
        try
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

            State = ApplicationState.CheckingZfsPropertySchema;
            CheckZfsPropertiesSchemaResult schemaCheckResult = await CheckZfsPoolRootPropertiesSchemaAsync( zfsCommandRunner, args ).ConfigureAwait( true );
            State = ApplicationState.Executing;

            Logger.Trace( "Result of schema check is: {0}", JsonSerializer.Serialize( schemaCheckResult ) );

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
                    return UpdateZfsDatasetSchema( _settings, schemaCheckResult.PoolRootsWithPropertyValidities, zfsCommandRunner )
                        ? SiazExecutionResultCode.ZfsPropertyUpdate_Succeeded
                        : SiazExecutionResultCode.ZfsPropertyUpdate_Failed;
                }
            }

            if ( cancellationToken.IsCancellationRequested )
            {
                return SiazExecutionResultCode.CancelledByToken;
            }

            // To avoid wasted allocations when there are not exactly 31 elements (the default size of a ConcurrentDictionary),
            // let's set the initial capacity of the datasets collection to the number we found in the schema check,
            // and set initial capacity of the snapshots collection to 4x that.
            int initialDsDictionaryCapacity = schemaCheckResult.PoolRootsWithPropertyValidities.Sum( static pair => pair.Value.Count );
            ConcurrentDictionary<string, ZfsRecord> datasets = new( Environment.ProcessorCount, initialDsDictionaryCapacity );
            ConcurrentDictionary<string, Snapshot> snapshots = new( Environment.ProcessorCount, initialDsDictionaryCapacity * 4 );

            Logger.Debug( "Getting remaining datasets and all snapshots from ZFS" );
            State = ApplicationState.GettingDataFromZfs;

            await zfsCommandRunner.GetDatasetsAndSnapshotsFromZfsAsync( _settings, datasets, snapshots ).ConfigureAwait( true );
            State = ApplicationState.Executing;

            Logger.Debug( "Finished getting datasets and snapshots from ZFS" );

            if ( cancellationToken.IsCancellationRequested )
            {
                return SiazExecutionResultCode.CancelledByToken;
            }

            // Handle taking new snapshots, if requested
            if ( _settings is { TakeSnapshots: true } )
            {
                Logger.Debug( "TakeSnapshots is true. Taking configured snapshots using timestamp {0:O}", currentTimestamp );
                State = ApplicationState.TakingSnapshots;
                await TakeAllConfiguredSnapshotsAsync( currentTimestamp, datasets, snapshots ).ConfigureAwait( true );
                State = ApplicationState.Executing;
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
                State = ApplicationState.PruningSnapshots;
                await PruneAllConfiguredSnapshotsAsync( zfsCommandRunner, _settings, datasets ).ConfigureAwait( true );
                State = ApplicationState.Executing;
            }
            else
            {
                Logger.Info( "Not pruning snapshots" );
            }

            // As a final step, if we are executing as a daemon, unsubscribe all datasets from their parents' events,
            // to reduce how many generations old objects will live before the GC cleans them up
            // ReSharper disable once InvertIf
            if ( _settings is { Daemonize: true } )
            {
                foreach ( ( string _, ZfsRecord ds ) in datasets )
                {
                    if ( ds.IsPoolRoot )
                    {
                        UnsubscribeChildRecordsFromEvents( ds );
                    }
                }

                static void UnsubscribeChildRecordsFromEvents( ZfsRecord node )
                {
                    foreach ( ( string _, ZfsRecord child ) in node.GetSortedChildDatasets( ) )
                    {
                        UnsubscribeChildRecordsFromEvents( child );
                        node.UnsubscribeChildFromPropertyEvents( child );
                    }

                    foreach ( ( SnapshotPeriodKind _, ConcurrentDictionary<string, Snapshot> dict ) in node.Snapshots )
                    {
                        foreach ( ( string _, Snapshot snap ) in dict )
                        {
                            node.UnsubscribeSnapshotFromPropertyEvents( snap );
                        }
                    }
                }
            }

            return SiazExecutionResultCode.Completed;
        }
        finally
        {
            State = ApplicationState.Idle;
        }
    }

    private void SetNextRunTime( in int greatestCommonFrequentIntervalMinutes, in DateTimeOffset timestamp, in DateTimeOffset lastRunTime, out DateTimeOffset nextRunTime )
    {
        DateTimeOffset lastRunTimeSnappedToFrequentPeriod = timestamp.Subtract( new TimeSpan( 0, 0, timestamp.Minute % greatestCommonFrequentIntervalMinutes, timestamp.Second, timestamp.Millisecond, timestamp.Microsecond ) );
        DateTimeOffset lastRunTimeSnappedToNextFrequentPeriod = lastRunTimeSnappedToFrequentPeriod.AddMinutes( greatestCommonFrequentIntervalMinutes );
        DateTimeOffset lastRunTimePlusFrequentInterval = lastRunTime.AddMinutes( greatestCommonFrequentIntervalMinutes );
        nextRunTime = new[] { lastRunTimePlusFrequentInterval, lastRunTimeSnappedToNextFrequentPeriod }.Min( );
        NextRunTimeChanged?.Invoke( this, nextRunTime.ToUnixTimeMilliseconds( ) );
    }

    private sealed record CheckZfsPropertiesSchemaResult( ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> PoolRootsWithPropertyValidities, bool MissingPropertiesFound );

    // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
    private readonly ISnapshotOperationsObserver? _snapshotOperationsObserver;

    private readonly IApplicationStateObserver? _stateObserver;
    // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable
}
