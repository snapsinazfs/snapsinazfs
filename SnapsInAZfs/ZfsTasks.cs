// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using SnapsInAZfs.Interop.Concurrency;
using SnapsInAZfs.Interop.Libc.Enums;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs;

internal static class ZfsTasks
{
    private const string SnapshotMutexName = "Global\\SnapsInAZfs_Snapshots";
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <exception cref="InvalidOperationException">If an invalid value is returned when getting the mutex</exception>
    internal static void TakeAllConfiguredSnapshots( IZfsCommandRunner commandRunner, SnapsInAZfsSettings settings, DateTimeOffset timestamp, ConcurrentDictionary<string, Dataset> datasets )
    {
        using MutexAcquisitionResult mutexAcquisitionResult = Mutexes.GetAndWaitMutex( SnapshotMutexName );
        switch ( mutexAcquisitionResult.ErrorCode )
        {
            case MutexAcquisitionErrno.Success:
            {
                Logger.Trace( "Successfully acquired mutex {0}", SnapshotMutexName );
            }
                break;
            // All of the error cases can just fall through, here, because we really don't care WHY it failed,
            // for the purposes of taking snapshots. We'll just let the user know and then not take snapshots.
            case MutexAcquisitionErrno.InProgess:
            case MutexAcquisitionErrno.IoException:
            case MutexAcquisitionErrno.AbandonedMutex:
            case MutexAcquisitionErrno.WaitHandleCannotBeOpened:
            case MutexAcquisitionErrno.PossiblyNullMutex:
            case MutexAcquisitionErrno.AnotherProcessIsBusy:
            case MutexAcquisitionErrno.InvalidMutexNameRequested:
            {
                Logger.Error( mutexAcquisitionResult.Exception, "Failed to acquire mutex {0}. Error code {1}", SnapshotMutexName, mutexAcquisitionResult.ErrorCode );
                return;
            }
            default:
                throw new InvalidOperationException( "An invalid value was returned from GetMutex", mutexAcquisitionResult.Exception );
        }

        Logger.Info( "Begin taking snapshots for all configured datasets" );

        foreach ( ( string _, Dataset ds ) in datasets )
        {
            //OK to disable this warning here. We don't use it if the result is false, and we don't put null in the collection in the first place
#pragma warning disable CS8600
            if ( !settings.Templates.TryGetValue( ds.Template, out TemplateSettings template ) )
            {
                string errorMessage = $"Template {ds.Template} specified for {ds.Name} not found in configuration - skipping";
                Logger.Error( errorMessage );
                continue;
            }
#pragma warning restore CS8600

            // The MaxBy function will fail if the sort key is a value type (it is - DateTimeOffset) and the collection is null
            // ReSharper disable SimplifyLinqExpressionUseMinByAndMaxBy
            List<ZfsProperty> propsToSet = new( );
            if ( ds is not { TakeSnapshots: true } )
            {
                Logger.Debug( "Dataset {0} not configured to take snapshots - skipping", ds.Name );
                continue;
            }

            if ( ds is not { Enabled: true } )
            {
                Logger.Debug( "Dataset {0} is disabled - skipping", ds.Name );
                continue;
            }

            Logger.Debug( "Checking for and taking needed snapshots for dataset {0}", ds.Name );

            if ( ds.IsFrequentSnapshotNeeded( template.SnapshotTiming, timestamp ) )
            {
                Logger.Debug( "Frequent snapshot needed for dataset {0}", ds.Name );
                TakeSnapshotKind( ds, SnapshotPeriod.Frequent, propsToSet );
            }

            if ( ds.IsHourlySnapshotNeeded( timestamp ) )
            {
                Logger.Debug( "Hourly snapshot needed for dataset {0}", ds.Name );
                TakeSnapshotKind( ds, SnapshotPeriod.Hourly, propsToSet );
            }

            if ( ds.IsDailySnapshotNeeded( timestamp ) )
            {
                Logger.Debug( "Daily snapshot needed for dataset {0}", ds.Name );
                TakeSnapshotKind( ds, SnapshotPeriod.Daily, propsToSet );
            }

            if ( ds.IsWeeklySnapshotNeeded( template.SnapshotTiming, timestamp ) )
            {
                Logger.Debug( "Weekly snapshot needed for dataset {0}", ds.Name );
                TakeSnapshotKind( ds, SnapshotPeriod.Weekly, propsToSet );
            }

            if ( ds.IsMonthlySnapshotNeeded( timestamp ) )
            {
                Logger.Debug( "Monthly snapshot needed for dataset {0}", ds.Name );
                TakeSnapshotKind( ds, SnapshotPeriod.Monthly, propsToSet );
            }

            if ( ds.IsYearlySnapshotNeeded( timestamp ) )
            {
                Logger.Debug( "Yearly snapshot needed for dataset {0}", ds.Name );
                TakeSnapshotKind( ds, SnapshotPeriod.Yearly, propsToSet );
            }

            if ( propsToSet.Any( ) )
            {
                Logger.Debug( "Took snapshots of {0}. Need to set properties: {1}", ds.Name, string.Join( ',', propsToSet.Select( p => $"{p.Name}: {p.Value}" ) ) );
                if ( commandRunner.SetZfsProperties( settings.DryRun, ds.Name, propsToSet.ToArray( ) ) && !settings.DryRun )
                {
                    Logger.Debug( "Property set successful" );
                    continue;
                }

                if ( settings.DryRun )
                {
                    Logger.Info( "DRY RUN: No properties were set on actual datasets" );
                    continue;
                }

                Logger.Error( "Error setting properties for dataset {0}", ds.Name );
                continue;
            }

            Logger.Debug( "No snapshots needed for dataset {0}", ds.Name );
        }

        Logger.Debug( "Finished taking snapshots" );

        // snapshotName is a defined string. Thus, this NullReferenceException is not possible.
        // ReSharper disable once ExceptionNotDocumentedOptional
        Mutexes.ReleaseMutex( SnapshotMutexName );

        return;

        void TakeSnapshotKind( Dataset ds, SnapshotPeriod period, List<ZfsProperty> propsToSet )
        {
            ZfsProperty? prop = null;
            string datasetSnapshotTimestampPropertyName = period.Kind switch
            {
                SnapshotPeriodKind.Frequent => ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName,
                SnapshotPeriodKind.Hourly => ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName,
                SnapshotPeriodKind.Daily => ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName,
                SnapshotPeriodKind.Weekly => ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName,
                SnapshotPeriodKind.Monthly => ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName,
                SnapshotPeriodKind.Yearly => ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName,
                _ => throw new ArgumentOutOfRangeException( nameof( period ) )
            };
            bool snapshotTaken = TakeSnapshot( commandRunner, settings, ds, period, timestamp, out Snapshot? snapshot );
            if ( snapshotTaken && ds.Properties.TryGetValue( datasetSnapshotTimestampPropertyName, out prop ) )
            {
                Logger.Trace( "{0} snapshot {1} taken successfully", period, snapshot?.Name ?? $"of {ds.Name}" );
                prop.Value = timestamp.ToString( "O" );
                prop.Source = "local";
                ds[ datasetSnapshotTimestampPropertyName ] = prop;
                propsToSet.Add( prop );
            }
            else if ( !snapshotTaken && settings.DryRun )
            {
                ZfsProperty fakeProp = prop ?? new ZfsProperty( datasetSnapshotTimestampPropertyName, timestamp.ToString( "O" ), "local" );
                ds[ datasetSnapshotTimestampPropertyName ] = fakeProp;
                propsToSet.Add( fakeProp );
            }
        }
    }

    internal static async Task<Errno> PruneAllConfiguredSnapshotsAsync( IZfsCommandRunner commandRunner, SnapsInAZfsSettings settings, ConcurrentDictionary<string, Dataset> datasets )
    {
        using MutexAcquisitionResult mutexAcquisitionResult = Mutexes.GetAndWaitMutex( SnapshotMutexName );
        switch ( mutexAcquisitionResult.ErrorCode )
        {
            case MutexAcquisitionErrno.Success:
            {
                Logger.Trace( "Successfully acquired mutex {0}", SnapshotMutexName );
            }
                break;
            // All of the error cases can just fall through, here, because we really don't care WHY it failed,
            // for the purposes of taking snapshots. We'll just let the user know and then not take snapshots.
            case MutexAcquisitionErrno.InProgess:
            case MutexAcquisitionErrno.IoException:
            case MutexAcquisitionErrno.AbandonedMutex:
            case MutexAcquisitionErrno.WaitHandleCannotBeOpened:
            case MutexAcquisitionErrno.PossiblyNullMutex:
            case MutexAcquisitionErrno.AnotherProcessIsBusy:
            case MutexAcquisitionErrno.InvalidMutexNameRequested:
            {
                Logger.Error( mutexAcquisitionResult.Exception, "Failed to acquire mutex {0}. Error code {1}", SnapshotMutexName, mutexAcquisitionResult.ErrorCode );
                return mutexAcquisitionResult;
            }
            default:
                throw new InvalidOperationException( "An invalid value was returned from GetMutex", mutexAcquisitionResult.Exception );
        }

        Logger.Info( "Begin pruning snapshots for all configured datasets" );
        ParallelOptions options = new( ) { MaxDegreeOfParallelism = 4, TaskScheduler = null };
        await Parallel.ForEachAsync( datasets.Values, options, async ( ds, _ ) => await PruneSnapshotsForDatasetAsync( ds ).ConfigureAwait( true ) ).ConfigureAwait( true );

        // snapshotName is a constant string. Thus, this NullReferenceException is not possible.
        // ReSharper disable once ExceptionNotDocumentedOptional
        Mutexes.ReleaseMutex( SnapshotMutexName );

        return Errno.EOK;

        async Task PruneSnapshotsForDatasetAsync( Dataset ds )
        {
            if ( ds is not { Enabled: true } )
            {
                Logger.Debug( "Dataset {0} is disabled - skipping prune", ds.Name );
                return;
            }

            if ( ds is not { PruneSnapshots: true } )
            {
                Logger.Debug( "Dataset {0} not configured to prune snapshots - skipping", ds.Name );
                return;
            }

            List<Snapshot> snapshotsToPruneForDataset = ds.GetSnapshotsToPrune( );

            Logger.Debug( "Need to prune the following snapshots from {0}: {1}", ds.Name, string.Join( ',', snapshotsToPruneForDataset.Select( s => s.Name ) ) );

            foreach ( Snapshot snapshot in snapshotsToPruneForDataset )
            {
                bool destroySuccessful = await commandRunner.DestroySnapshotAsync( snapshot, settings ).ConfigureAwait( true );
                if ( destroySuccessful || settings.DryRun )
                {
                    if ( settings.DryRun )
                    {
                        Logger.Info( "DRY RUN: Snapshot not destroyed, but pretending it was for simulation" );
                    }
                    else
                    {
                        Logger.Info( "Destroyed snapshot {0}", snapshot.Name );
                    }

                    switch ( snapshot.Period.Kind )
                    {
                        case SnapshotPeriodKind.Frequent:
                            ds.FrequentSnapshots.Remove( snapshot );
                            goto default;
                        case SnapshotPeriodKind.Hourly:
                            ds.HourlySnapshots.Remove( snapshot );
                            goto default;
                        case SnapshotPeriodKind.Daily:
                            ds.DailySnapshots.Remove( snapshot );
                            goto default;
                        case SnapshotPeriodKind.Weekly:
                            ds.WeeklySnapshots.Remove( snapshot );
                            goto default;
                        case SnapshotPeriodKind.Monthly:
                            ds.MonthlySnapshots.Remove( snapshot );
                            goto default;
                        case SnapshotPeriodKind.Yearly:
                            ds.YearlySnapshots.Remove( snapshot );
                            goto default;
                        default:
                            ds.AllSnapshots.TryRemove( snapshot.Name, out _ );
                            break;
                    }

                    continue;
                }

                Logger.Error( "Failed to destroy snapshot {0}", snapshot.Name );
            }
        }
    }

    internal static bool TakeSnapshot( IZfsCommandRunner commandRunner, SnapsInAZfsSettings settings, Dataset ds, SnapshotPeriod snapshotPeriod, DateTimeOffset timestamp, out Snapshot? snapshot )
    {
        Logger.Debug( "TakeSnapshot called for {0} with period {1}", ds.Name, snapshotPeriod );
        snapshot = null;
        if ( !ds.Enabled )
        {
            Logger.Trace( "Dataset {0} is not enabled. Skipping", ds.Name );
            return false;
        }

        if ( !ds.TakeSnapshots )
        {
            Logger.Trace( "Dataset {0} is not configured to take snapshots. Skipping", ds.Name );
            return false;
        }

        if ( ds.Recursion == ZfsPropertyValueConstants.ZfsRecursion && ds[ ZfsPropertyNames.RecursionPropertyName ]?.Source != "local" )
        {
            Logger.Trace( "Ancestor of dataset {0} is configured for zfs native recursion and recursion not set locally. Skipping", ds.Name );
            return false;
        }

        if ( !settings.Templates.TryGetValue( ds.Template, out TemplateSettings? template ) )
        {
            Logger.Error( "Template {0} for dataset {1} not found in configuration. Skipping", ds.Template, ds.Name );
            return false;
        }

        switch ( snapshotPeriod.Kind )
        {
            case SnapshotPeriodKind.Frequent:
                if ( ds.RetentionSettings.Frequent == 0 )
                {
                    Logger.Trace( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", snapshotPeriod, ds.Name );
                    return false;
                }

                break;
            case SnapshotPeriodKind.Hourly:
                if ( ds.RetentionSettings.Hourly == 0 )
                {
                    Logger.Trace( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", snapshotPeriod, ds.Name );
                    return false;
                }

                break;
            case SnapshotPeriodKind.Daily:
                if ( ds.RetentionSettings.Daily == 0 )
                {
                    Logger.Trace( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", snapshotPeriod, ds.Name );
                    return false;
                }

                break;
            case SnapshotPeriodKind.Weekly:
                if ( ds.RetentionSettings.Weekly == 0 )
                {
                    Logger.Trace( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", snapshotPeriod, ds.Name );
                    return false;
                }

                break;
            case SnapshotPeriodKind.Monthly:
                if ( ds.RetentionSettings.Monthly == 0 )
                {
                    Logger.Trace( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", snapshotPeriod, ds.Name );
                    return false;
                }

                break;
            case SnapshotPeriodKind.Yearly:
                if ( ds.RetentionSettings.Yearly == 0 )
                {
                    Logger.Trace( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", snapshotPeriod, ds.Name );
                    return false;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException( nameof( snapshotPeriod ), snapshotPeriod, $"Unexpected value received for snapshotPeriod for dataset {ds.Name}. Snapshot not taken." );
        }

        Logger.Trace( "Dataset {0} will have a snapshot taken with these settings: {1}", ds.Name, JsonSerializer.Serialize( new { ds, template } ) );

        if ( commandRunner.TakeSnapshot( ds, snapshotPeriod, timestamp, settings, settings.Templates[ ds.Template ], out snapshot ) )
        {
            ds.AddSnapshot( snapshot );
            Logger.Info( "Snapshot {0} successfully taken", snapshot.Name );
            return true;
        }

        if ( settings.DryRun )
        {
            Logger.Info( "DRY RUN: Snapshot for dataset {0} not taken", ds.Name );
            return false;
        }

        Logger.Error( "Snapshot for dataset {0} not taken", ds.Name );
        return false;
    }

    internal static bool UpdateZfsDatasetSchema( bool dryRun, ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> poolRootsWithPropertyValidities, IZfsCommandRunner zfsCommandRunner )
    {
        bool errorsEncountered = false;
        Logger.Debug( "Requested update of zfs properties schema" );
        foreach ( ( string poolName, ConcurrentDictionary<string, bool> propertiesToAdd ) in poolRootsWithPropertyValidities )
        {
            Logger.Info( "Updating properties for pool {0}", poolName );

            // It's not a nullable type...
            // ReSharper disable once ExceptionNotDocumentedOptional
            string[] propertyArray = propertiesToAdd.Where( kvp => !kvp.Value ).Select( kvp => kvp.Key ).ToArray( );

            // Attempt to set the missing properties for the pool.
            // Log an error if unsuccessful
            if ( zfsCommandRunner.SetDefaultValuesForMissingZfsPropertiesOnPoolAsync( dryRun, poolName, propertyArray ) )
            {
                Logger.Info( "Finished updating properties for pool {0}", poolName );
            }
            else
            {
                if ( dryRun )
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

        return !errorsEncountered;
    }

    public record CheckZfsPropertiesSchemaResult( ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> PoolRootsWithPropertyValidities, bool MissingPropertiesFound );

    public static async Task<CheckZfsPropertiesSchemaResult> CheckZfsPoolRootPropertiesSchemaAsync( IZfsCommandRunner zfsCommandRunner, CommandLineArguments args )
    {
        Logger.Debug( "Checking zfs properties schema" );

        ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> poolRootsWithPropertyValidities = await zfsCommandRunner.GetPoolRootsAndPropertyValiditiesAsync( ).ConfigureAwait( true );
        bool missingPropertiesFound = false;
        foreach ( ( string poolName, ConcurrentDictionary<string, bool>? propertyValidities ) in poolRootsWithPropertyValidities )
        {
            Logger.Debug( "Checking property validities for pool root {0}", poolName );
            bool missingPropertiesFoundForPool = false;
            foreach ( ( string propName, bool propValue ) in propertyValidities )
            {
                if ( !ZfsProperty.DefaultDatasetProperties.ContainsKey( propName ) )
                {
                    Logger.Trace( "Not interested in property {0} for pool root schema check", propName );
                    continue;
                }

                Logger.Trace( "Checking validity of property {0} in pool root {1}", propName, poolName );
                if ( propValue )
                {
                    Logger.Trace( "Pool root {0} has property {1} with a valid value", poolName, propName );
                    continue;
                }

                Logger.Debug( "Pool root {0} has missing or invalid property {1}", poolName, propName );
                if ( !missingPropertiesFoundForPool )
                {
                    missingPropertiesFound = missingPropertiesFoundForPool = true;
                }
            }

            Logger.Debug( "Finished checking property validities for pool root {0}", poolName );

            switch ( args )
            {
                case { CheckZfsProperties: true } when missingPropertiesFoundForPool:
                    Logger.Warn( "Pool {0} is missing the following properties: {1}", poolName, string.Join( ", ", propertyValidities.Where( kvp => !kvp.Value ).Select( kvp => kvp.Key ) ) );
                    continue;
                case { CheckZfsProperties: true } when !missingPropertiesFoundForPool:
                    Logger.Info( "No missing properties in pool {0}", poolName );
                    continue;
                case { PrepareZfsProperties: true } when missingPropertiesFoundForPool:
                    Logger.Info( "Pool {0} is missing the following properties: {1}", poolName, string.Join( ", ", propertyValidities.Where( kvp => !kvp.Value ).Select( kvp => kvp.Key ) ) );
                    continue;
                case { PrepareZfsProperties: true } when !missingPropertiesFoundForPool:
                    Logger.Info( "No missing properties in pool {0}", poolName );
                    continue;
                case { PrepareZfsProperties: false, CheckZfsProperties: false } when missingPropertiesFoundForPool:
                    Logger.Fatal( "Pool {0} is missing the following properties: {1}", poolName, string.Join( ", ", propertyValidities.Where( kvp => !kvp.Value ).Select( kvp => kvp.Key ) ) );
                    continue;
                case { PrepareZfsProperties: false, CheckZfsProperties: false } when !missingPropertiesFoundForPool:
                    Logger.Debug( "No missing properties in pool {0}", poolName );
                    continue;
            }
        }

        return new( poolRootsWithPropertyValidities, missingPropertiesFound );
    }

    /// <summary>
    ///     Gets the pool roots with all SnapsInAZfs properties and capacity properties
    /// </summary>
    /// <param name="zfsCommandRunner"></param>
    /// <returns></returns>
    /// <remarks>Should only be called once schema validity has been checked, or else results are undefined and unsupported</remarks>
    public static async Task<ConcurrentDictionary<string, Dataset>> GetPoolRootsWithPropertiesAndCapacities( IZfsCommandRunner zfsCommandRunner )
    {
        ConcurrentDictionary<string, Dataset> poolRoots = await zfsCommandRunner.GetPoolRootDatasetsWithAllRequiredSnapsInAZfsPropertiesAsync( ).ConfigureAwait( true );

        if ( poolRoots.Any( ) )
        {
            await zfsCommandRunner.GetPoolCapacitiesAsync( poolRoots ).ConfigureAwait( true );
        }

        return poolRoots;
    }

    [SuppressMessage( "ReSharper", "AsyncConverter.AsyncAwaitMayBeElidedHighlighting", Justification = "Without using this all the way down, the application won't actually work properly" )]
    public static async Task GetDatasetsAndSnapshotsFromZfsAsync( IZfsCommandRunner zfsCommandRunner, ConcurrentDictionary<string, Dataset> datasets, ConcurrentDictionary<string, Snapshot> snapshots )
    {
        await zfsCommandRunner.GetDatasetsAndSnapshotsFromZfsAsync( datasets, snapshots ).ConfigureAwait( true );
    }
}
