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

internal static class SnapshotTasks
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <exception cref="InvalidOperationException">If an invalid value is returned when getting the mutex</exception>
    internal static Errno TakeAllConfiguredSnapshots( IZfsCommandRunner commandRunner, SanoidSettings settings, SnapshotPeriod period, DateTimeOffset timestamp, ref Dictionary<string, Dataset> datasets )
    {
        const string snapshotMutexName = "Global\\Sanoid.net_Snapshots";
        using MutexAcquisitionResult mutexAcquisitionResult = Mutexes.GetAndWaitMutex( snapshotMutexName );
        switch ( mutexAcquisitionResult.ErrorCode )
        {
            case MutexAcquisitionErrno.Success:
            {
                Logger.Debug( "Successfully acquired mutex {0}", snapshotMutexName );
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

        Logger.Debug( "Begin taking {0} snapshots", period );

        foreach ( ( string _, Dataset ds ) in datasets )
        {
            if ( !settings.Templates.TryGetValue( ds.Template, out TemplateSettings? template ) )
            {
                string errorMessage = $"Template {ds.Template} specified for {ds.Name} not found in configuration - skipping";
                Logger.Error( errorMessage );
                continue;
            }

            Snapshot? latestFrequentSnapshot = ds.Snapshots.Values.Where( s => s.Period == SnapshotPeriod.Frequent ).MaxBy( s => s.Timestamp );
            Snapshot? latestHourlySnapshot = ds.Snapshots.Values.Where( s => s.Period == SnapshotPeriod.Hourly ).MaxBy( s => s.Timestamp );
            Snapshot? latestDailySnapshot = ds.Snapshots.Values.Where( s => s.Period == SnapshotPeriod.Daily ).MaxBy( s => s.Timestamp );
            Snapshot? latestWeeklySnapshot = ds.Snapshots.Values.Where( s => s.Period == SnapshotPeriod.Weekly ).MaxBy( s => s.Timestamp );
            Snapshot? latestMonthlySnapshot = ds.Snapshots.Values.Where( s => s.Period == SnapshotPeriod.Monthly ).MaxBy( s => s.Timestamp );
            Snapshot? latestYearlySnapshot = ds.Snapshots.Values.Where( s => s.Period == SnapshotPeriod.Yearly ).MaxBy( s => s.Timestamp );

            if ( template.SnapshotRetention.Frequent > 0 && (latestFrequentSnapshot is null) || timestamp.Subtract( latestFrequentSnapshot.Timestamp ).TotalMinutes >= template.SnapshotTiming.FrequentPeriod )
            {
                TakeSnapshot( commandRunner, settings, ds, SnapshotPeriod.Frequent, timestamp );
            }
            if ( template.SnapshotRetention.Hourly > 0 && ((latestHourlySnapshot is null) || (timestamp.Subtract( latestHourlySnapshot.Timestamp ).TotalHours >= 1d )))
            {
                TakeSnapshot( commandRunner, settings, ds, SnapshotPeriod.Hourly, timestamp );
            }
            if ( template.SnapshotRetention.Daily > 0 && ((latestDailySnapshot is null) || (timestamp.Subtract( latestDailySnapshot.Timestamp ).TotalDays >= 1d) ))
            {
                TakeSnapshot( commandRunner, settings, ds, SnapshotPeriod.Hourly, timestamp );
            }
        }

        Logger.Debug( "Finished taking {0} snapshots", period );

        // snapshotName is a defined string. Thus, this NullReferenceException is not possible.
        // ReSharper disable once ExceptionNotDocumentedOptional
        Mutexes.ReleaseMutex( snapshotMutexName );

        return Errno.EOK;
    }

    internal static void TakeSnapshot( IZfsCommandRunner commandRunner, SanoidSettings settings, Dataset ds, SnapshotPeriod snapshotPeriod, DateTimeOffset timestamp )
    {
        Logger.Debug( "TakeSnapshot called for {0} with period {1}", ds.Name, snapshotPeriod );

        Logger.Debug( "Checking dataset {0} settings: {1}", ds.Name, ds );
        if ( !ds.Enabled )
        {
            Logger.Debug( "Dataset {0} is not enabled. Skipping", ds.Name );
            return;
        }

        if ( !ds.TakeSnapshots )
        {
            Logger.Debug( "Dataset {0} is not configured to take snapshots. Skipping", ds.Name );
            return;
        }

        if ( ds.Recursion == SnapshotRecursionMode.Zfs && ds[ "sanoid.net:recursion" ]?.PropertySource != ZfsPropertySource.Local )
        {
            Logger.Debug( "Ancestor of dataset {0} is configured for zfs native recursion and recursion not set locally. Skipping", ds.Name );
            return;
        }

        if ( !settings.Templates.TryGetValue( ds.Template, out TemplateSettings? template ) )
        {
            Logger.Error( "Template {0} for dataset {1} not found in configuration. Skipping", ds.Template, ds.Name );
            return;
        }

        switch ( snapshotPeriod.Kind )
        {
            case SnapshotPeriodKind.Frequent:
                if ( template.SnapshotRetention.Frequent == 0 )
                {
                    Logger.Debug( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", snapshotPeriod, ds.Name );
                    return;
                }

                break;
            case SnapshotPeriodKind.Hourly:
                if ( template.SnapshotRetention.Hourly == 0 )
                {
                    Logger.Debug( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", snapshotPeriod, ds.Name );
                    return;
                }

                break;
            case SnapshotPeriodKind.Daily:
                if ( template.SnapshotRetention.Daily == 0 )
                {
                    Logger.Debug( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", snapshotPeriod, ds.Name );
                    return;
                }

                break;
            case SnapshotPeriodKind.Weekly:
                if ( template.SnapshotRetention.Weekly == 0 )
                {
                    Logger.Debug( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", snapshotPeriod, ds.Name );
                    return;
                }

                break;
            case SnapshotPeriodKind.Monthly:
                if ( template.SnapshotRetention.Monthly == 0 )
                {
                    Logger.Debug( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", snapshotPeriod, ds.Name );
                    return;
                }

                break;
            case SnapshotPeriodKind.Yearly:
                if ( template.SnapshotRetention.Yearly == 0 )
                {
                    Logger.Debug( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", snapshotPeriod, ds.Name );
                    return;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException( nameof( snapshotPeriod ), snapshotPeriod, $"Unexpected value received for snapshotPeriod for dataset {ds.Name}. Snapshot not taken." );
        }

        Logger.Debug( "Dataset {0} will have a snapshot taken with these settings: {1}", ds.Name, JsonSerializer.Serialize( new { ds, template } ) );

        if ( commandRunner.TakeSnapshot( ds, snapshotPeriod, timestamp, settings, out Snapshot snapshot ) )
        {
            ds.Snapshots[snapshot.Name] = snapshot;
            Logger.Info( "Snapshot {0} successfully taken", snapshot.Name );
        }
        else
        {
            Logger.Error( "Snapshot for dataset {0} not taken", ds.Name );
        }
    }
}
