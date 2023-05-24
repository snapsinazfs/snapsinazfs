// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;
using System.Text.Json;
using Sanoid.Common.Configuration;
using Sanoid.Common.Configuration.Datasets;
using Sanoid.Common.Configuration.Snapshots;
using Sanoid.Interop.Concurrency;
using Sanoid.Interop.Libc.Enums;

namespace Sanoid;

internal static class SnapshotTasks
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <exception cref="InvalidOperationException">If an invalid value is returned when getting the mutex</exception>
    internal static Errno TakeAllConfiguredSnapshots( Configuration config, SnapshotPeriod period, DateTimeOffset timestamp )
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
            case MutexAcquisitionErrno.InProgess:
            case MutexAcquisitionErrno.IoException:
            case MutexAcquisitionErrno.AbandonedMutex:
            case MutexAcquisitionErrno.WaitHandleCannotBeOpened:
            case MutexAcquisitionErrno.PossiblyNullMutex:
            case MutexAcquisitionErrno.AnotherProcessIsBusy:
            case MutexAcquisitionErrno.InvalidMutexNameRequested:
            {
                Logger.Error( mutexAcquisitionResult.Exception, "Failed to acquire mutex {0}. Error code {1}.", snapshotMutexName, mutexAcquisitionResult.ErrorCode );
                return mutexAcquisitionResult;
            }
            default:
                throw new InvalidOperationException( "An invalid value was returned from GetMutex", mutexAcquisitionResult.Exception );
        }

        ConcurrentQueue<Dataset> wantedRoots = new( );
        Logger.Debug( "Building Dataset queue for snapshots" );
        foreach ( ( string _, Dataset dataset ) in config.Datasets )
        {
            if ( dataset.Path == "/" )
            {
                continue;
            }

            Logger.Debug( "Checking dataset {0} for inclusion.", dataset.Path );
            switch ( dataset )
            {
                case { Template.AutoSnapshot: true, Enabled: true }:
                {
                    Logger.Debug( "{0} is wanted for snapshots. Checking period.", dataset.Path );
                    if ( dataset.IsWantedForPeriod( period ) )
                    {
                        Logger.Debug( "{0} is wanted for period. Adding to queue.", dataset.Path );
                        wantedRoots.Enqueue( dataset );
                        continue;
                    }

                    Logger.Trace( "{0} is not wanted for period.", dataset.Path );
                }
                    break;
                case { Enabled: false }:
                {
                    Logger.Trace( "{0} is not enabled for snapshots.", dataset.Path );
                }
                    break;
                case { Template: null }:
                {
                    Logger.Trace( "Dataset {0} has no Template. Skipping." );
                }
                    break;
                default:
                {
                    Logger.Error( "Dataset {0} did not match any expected conditions. Exiting.", dataset.Path );
                    wantedRoots.Clear( );
                    throw new InvalidOperationException( $"Dataset {dataset.Path} did not match any expected conditions. Exiting." );
                }
            }
        }

        Logger.Debug( "Finished building Dataset queue for snapshots" );
        Logger.Trace( "SnapshotQueue: {0}", JsonSerializer.Serialize( wantedRoots.Select( wr => wr.VirtualPath ).ToArray( ) ) );

        while ( wantedRoots.TryDequeue( out Dataset? ds ) )
        {
            TakeSnapshot( config, ds, period, timestamp );
        }

        // snapshotName is a defined string. Thus, this NullReferenceException is not possible.
        // ReSharper disable once ExceptionNotDocumentedOptional
        Mutexes.ReleaseMutex( snapshotMutexName );

        return Errno.EOK;
    }

    internal static void TakeSnapshot( Configuration config, Dataset ds, SnapshotPeriod snapshotPeriod, DateTimeOffset timestamp )
    {
        config.ZfsCommandRunner.ZfsSnapshot( ds, config.SnapshotNaming.GetSnapshotName( snapshotPeriod, timestamp ) );
    }
}
