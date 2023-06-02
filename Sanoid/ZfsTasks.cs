// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Text.Json;
using Sanoid.Interop.Concurrency;
using Sanoid.Interop.Libc.Enums;
using Sanoid.Interop.Zfs.ZfsCommandRunner;
using Sanoid.Interop.Zfs.ZfsTypes;
using Sanoid.Settings.Settings;

namespace Sanoid;

internal static class ZfsTasks
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <exception cref="InvalidOperationException">If an invalid value is returned when getting the mutex</exception>
    internal static Errno TakeAllConfiguredSnapshots( IZfsCommandRunner commandRunner, SanoidSettings settings, DateTimeOffset timestamp, ref Dictionary<string, Dataset> datasets )
    {
        const string snapshotMutexName = "Global\\Sanoid.net_Snapshots";
        using MutexAcquisitionResult mutexAcquisitionResult = Mutexes.GetAndWaitMutex( snapshotMutexName );
        switch ( mutexAcquisitionResult.ErrorCode )
        {
            case MutexAcquisitionErrno.Success:
            {
                Logger.Trace( "Successfully acquired mutex {0}", snapshotMutexName );
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
                Logger.Error( mutexAcquisitionResult.Exception, "Failed to acquire mutex {0}. Error code {1}", snapshotMutexName, mutexAcquisitionResult.ErrorCode );
                return mutexAcquisitionResult;
            }
            default:
                throw new InvalidOperationException( "An invalid value was returned from GetMutex", mutexAcquisitionResult.Exception );
        }

        Logger.Info( "Begin taking snapshots for all configured datasets" );

        foreach ( ( string _, Dataset ds ) in datasets )
        {
            if ( !settings.Templates.TryGetValue( ds.Template, out TemplateSettings? template ) )
            {
                string errorMessage = $"Template {ds.Template} specified for {ds.Name} not found in configuration - skipping";
                Logger.Error( errorMessage );
                continue;
            }

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

            if ( ds.IsFrequentSnapshotNeeded( template, timestamp ) )
            {
                TakeSnapshotKind( ds, SnapshotPeriod.Frequent, propsToSet );
            }

            if ( ds.IsHourlySnapshotNeeded( template.SnapshotRetention, timestamp ) )
            {
                TakeSnapshotKind( ds, SnapshotPeriod.Hourly, propsToSet );
            }

            if ( ds.IsDailySnapshotNeeded( template.SnapshotRetention, timestamp ) )
            {
                TakeSnapshotKind( ds, SnapshotPeriod.Daily, propsToSet );
            }

            if ( ds.IsWeeklySnapshotNeeded( template, timestamp ) )
            {
                TakeSnapshotKind( ds, SnapshotPeriod.Weekly, propsToSet );
            }

            if ( ds.IsMonthlySnapshotNeeded( template, timestamp ) )
            {
                TakeSnapshotKind( ds, SnapshotPeriod.Monthly, propsToSet );
            }

            if ( ds.IsYearlySnapshotNeeded( template.SnapshotRetention, timestamp ) )
            {
                TakeSnapshotKind( ds, SnapshotPeriod.Yearly, propsToSet );
            }

            commandRunner.SetZfsProperties( settings.DryRun, ds.Name, propsToSet.ToArray( ) );
        }

        Logger.Debug( "Finished taking snapshots" );

        // snapshotName is a defined string. Thus, this NullReferenceException is not possible.
        // ReSharper disable once ExceptionNotDocumentedOptional
        Mutexes.ReleaseMutex( snapshotMutexName );

        return Errno.EOK;

        void TakeSnapshotKind( Dataset ds, SnapshotPeriod period, List<ZfsProperty> propsToSet )
        {
            ZfsProperty? prop = null;
            string datasetSnapshotTimestampPropertyName = period.Kind switch
            {
                SnapshotPeriodKind.Frequent => ZfsProperty.DatasetLastFrequentSnapshotTimestampPropertyName,
                SnapshotPeriodKind.Hourly => ZfsProperty.DatasetLastHourlySnapshotTimestampPropertyName,
                SnapshotPeriodKind.Daily => ZfsProperty.DatasetLastDailySnapshotTimestampPropertyName,
                SnapshotPeriodKind.Weekly => ZfsProperty.DatasetLastWeeklySnapshotTimestampPropertyName,
                SnapshotPeriodKind.Monthly => ZfsProperty.DatasetLastMonthlySnapshotTimestampPropertyName,
                SnapshotPeriodKind.Yearly => ZfsProperty.DatasetLastYearlySnapshotTimestampPropertyName,
                _ => throw new ArgumentOutOfRangeException( )
            };
            bool snapshotTaken = TakeSnapshot( commandRunner, settings, ds, period, timestamp, out Snapshot? snapshot );
            if ( snapshotTaken && ds.Properties.TryGetValue( datasetSnapshotTimestampPropertyName, out prop ) )
            {
                Logger.Trace( "{0} snapshot {1} taken successfully", period, snapshot?.Name ?? $"of {ds.Name}" );
                prop.Value = timestamp.ToString( "O" );
                prop.PropertySource = ZfsPropertySource.Local;
                ds[ datasetSnapshotTimestampPropertyName ] = prop;
                propsToSet.Add( prop );
            }
            else if ( !snapshotTaken && settings.DryRun )
            {
                ZfsProperty fakeProp = prop ?? new ZfsProperty( datasetSnapshotTimestampPropertyName, timestamp.ToString( "O" ), ZfsPropertySource.Local );
                ds[ datasetSnapshotTimestampPropertyName ] = fakeProp;
                propsToSet.Add( fakeProp );
            }
        }
    }

    internal static Errno PruneAllConfiguredSnapshots( IZfsCommandRunner commandRunner, SanoidSettings settings, DateTimeOffset timestamp, ref Dictionary<string, Dataset> datasets )
    {
        const string snapshotMutexName = "Global\\Sanoid.net_Snapshots";
        using MutexAcquisitionResult mutexAcquisitionResult = Mutexes.GetAndWaitMutex( snapshotMutexName );
        switch ( mutexAcquisitionResult.ErrorCode )
        {
            case MutexAcquisitionErrno.Success:
            {
                Logger.Trace( "Successfully acquired mutex {0}", snapshotMutexName );
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
                Logger.Error( mutexAcquisitionResult.Exception, "Failed to acquire mutex {0}. Error code {1}", snapshotMutexName, mutexAcquisitionResult.ErrorCode );
                return mutexAcquisitionResult;
            }
            default:
                throw new InvalidOperationException( "An invalid value was returned from GetMutex", mutexAcquisitionResult.Exception );
        }

        Logger.Info( "Begin pruning snapshots for all configured datasets" );
        List<Snapshot> allSnapshotsToPrune = new( );
        foreach ( ( string _, Dataset ds ) in datasets )
        {
            if ( !settings.Templates.TryGetValue( ds.Template, out TemplateSettings? template ) )
            {
                string errorMessage = $"Template {ds.Template} specified for {ds.Name} not found in configuration - skipping";
                Logger.Error( errorMessage );
                continue;
            }

            if ( ds is not { PruneSnapshots: true } )
            {
                Logger.Debug( "Dataset {0} not configured to prune snapshots - skipping", ds.Name );
                continue;
            }

            if ( ds is not { Enabled: true } )
            {
                Logger.Debug( "Dataset {0} is disabled - skipping prune", ds.Name );
                continue;
            }

            List<Snapshot> snapshotsToPruneForDataset = ds.GetSnapshotsToPrune( template, timestamp );
            Logger.Debug( "Need to prune the following snapshots from {0}: {1}", ds.Name, string.Join( ',', snapshotsToPruneForDataset.Select( s => s.Name ) ) );
            allSnapshotsToPrune.AddRange( snapshotsToPruneForDataset );
        }
        Logger.Debug( "Need to prune the following snapshots globally:{0}", string.Join( ',', allSnapshotsToPrune.Select( s => s.Name ) ) );

        // snapshotName is a defined string. Thus, this NullReferenceException is not possible.
        // ReSharper disable once ExceptionNotDocumentedOptional
        Mutexes.ReleaseMutex( snapshotMutexName );

        return Errno.EOK;
    }

    internal static bool TakeSnapshot( IZfsCommandRunner commandRunner, SanoidSettings settings, Dataset ds, SnapshotPeriod snapshotPeriod, DateTimeOffset timestamp, out Snapshot? snapshot )
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

        if ( ds.Recursion == SnapshotRecursionMode.Zfs && ds[ ZfsProperty.RecursionPropertyName ]?.PropertySource != ZfsPropertySource.Local )
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
                if ( template.SnapshotRetention.Frequent == 0 )
                {
                    Logger.Trace( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", snapshotPeriod, ds.Name );
                    return false;
                }

                break;
            case SnapshotPeriodKind.Hourly:
                if ( template.SnapshotRetention.Hourly == 0 )
                {
                    Logger.Trace( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", snapshotPeriod, ds.Name );
                    return false;
                }

                break;
            case SnapshotPeriodKind.Daily:
                if ( template.SnapshotRetention.Daily == 0 )
                {
                    Logger.Trace( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", snapshotPeriod, ds.Name );
                    return false;
                }

                break;
            case SnapshotPeriodKind.Weekly:
                if ( template.SnapshotRetention.Weekly == 0 )
                {
                    Logger.Trace( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", snapshotPeriod, ds.Name );
                    return false;
                }

                break;
            case SnapshotPeriodKind.Monthly:
                if ( template.SnapshotRetention.Monthly == 0 )
                {
                    Logger.Trace( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", snapshotPeriod, ds.Name );
                    return false;
                }

                break;
            case SnapshotPeriodKind.Yearly:
                if ( template.SnapshotRetention.Yearly == 0 )
                {
                    Logger.Trace( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", snapshotPeriod, ds.Name );
                    return false;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException( nameof( snapshotPeriod ), snapshotPeriod, $"Unexpected value received for snapshotPeriod for dataset {ds.Name}. Snapshot not taken." );
        }

        Logger.Trace( "Dataset {0} will have a snapshot taken with these settings: {1}", ds.Name, JsonSerializer.Serialize( new { ds, template } ) );

        if ( commandRunner.TakeSnapshot( ds, snapshotPeriod, timestamp, settings, out snapshot ) )
        {
            ds.AllSnapshots[ snapshot.Name ] = snapshot;
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

    internal static bool UpdateZfsDatasetSchema( bool dryRun, ref Dictionary<string, Dictionary<string, ZfsProperty>> missingPropertiesByPool, IZfsCommandRunner zfsCommandRunner )
    {
        bool errorsEncountered = false;
        Logger.Debug( "Requested update of zfs properties schema" );
        foreach ( ( string poolName, Dictionary<string, ZfsProperty> propertiesToAdd ) in missingPropertiesByPool )
        {
            Logger.Info( "Updating properties for pool {0}", poolName );

            // It's not a nullable type...
            // ReSharper disable once ExceptionNotDocumentedOptional
            ZfsProperty[] propertyArray = propertiesToAdd.Values.ToArray( );

            // Attempt to set the missing properties for the pool.
            // Log an error if unsuccessful
            if ( zfsCommandRunner.SetZfsProperties( dryRun, poolName, propertyArray ) )
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

    public static (Dictionary<string, Dictionary<string, ZfsProperty>> missingPoolPropertyCollections, bool missingPropertiesFound) CheckZfsPropertiesSchema( IZfsCommandRunner zfsCommandRunner, CommandLineArguments args )
    {
        Logger.Debug( "Checking zfs properties schema" );

        Dictionary<string, Dataset> poolRoots = zfsCommandRunner.GetZfsPoolRoots( );

        Dictionary<string, Dictionary<string, ZfsProperty>> missingPoolPropertyCollections = new( );
        bool missingPropertiesFound = false;

        foreach ( ( string poolName, Dataset? pool ) in poolRoots )
        {
            Logger.Debug( "Checking properties for pool {0}", poolName );
            Logger.Trace( "Pool {0} current properties collection: {1}", poolName, JsonSerializer.Serialize( pool.Properties ) );
            Dictionary<string, ZfsProperty> missingProperties = new( );

            foreach ( ( string? propertyName, ZfsProperty? property ) in ZfsProperty.DefaultDatasetProperties )
            {
                Logger.Trace( "Checking pool {0} for property {1}", poolName, propertyName );
                if ( pool.HasProperty( propertyName ) )
                {
                    Logger.Trace( "Pool {0} already has property {1}", poolName, propertyName );
                    continue;
                }

                Logger.Debug( "Pool {0} does not have property {1}", poolName, property );
                pool.AddProperty( ZfsProperty.DefaultDatasetProperties[ propertyName ] );
                missingProperties.Add( propertyName, ZfsProperty.DefaultDatasetProperties[ propertyName ] );
            }

            if ( missingProperties.Count > 0 )
            {
                missingPoolPropertyCollections.Add( poolName, missingProperties );
            }

            Logger.Debug( "Finished checking properties for pool {0}", poolName );

            // Can't be null because we literally constructed it above...
            // ReSharper disable ExceptionNotDocumentedOptional
            missingPropertiesFound = missingPoolPropertyCollections.Any( );
            bool missingPropertiesFoundForPool = missingProperties.Any( );
            // ReSharper restore ExceptionNotDocumentedOptional

            // Now let's act on what we found for this pool, based on command-line arguments
            switch ( args )
            {
                case { CheckZfsProperties: true } when missingPropertiesFoundForPool:
                    Logger.Warn( "Pool {0} is missing the following properties: {1}", poolName, string.Join( ", ", missingProperties.Keys ) );
                    break;
                case { CheckZfsProperties: true } when !missingPropertiesFoundForPool:
                    Logger.Info( "No missing properties in pool {0}", poolName );
                    break;
                case { PrepareZfsProperties: true } when missingPropertiesFoundForPool:
                    Logger.Info( "Pool {0} is missing the following properties: {1}", poolName, string.Join( ", ", missingProperties.Keys ) );
                    break;
                case { PrepareZfsProperties: true } when !missingPropertiesFoundForPool:
                    Logger.Info( "No missing properties in pool {0}", poolName );
                    break;
                case { PrepareZfsProperties: false, CheckZfsProperties: false } when missingPropertiesFoundForPool:
                    Logger.Fatal( "Pool {0} is missing the following properties: {1}", poolName, string.Join( ", ", missingProperties.Keys ) );
                    break;
                case { PrepareZfsProperties: false, CheckZfsProperties: false } when !missingPropertiesFoundForPool:
                    Logger.Debug( "No missing properties in pool {0}", poolName );
                    break;
            }
        }

        return ( missingPoolPropertyCollections, missingPropertiesFound );
    }
}
