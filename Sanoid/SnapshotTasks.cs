// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;
using Sanoid.Common.Configuration;
using Sanoid.Common.Configuration.Datasets;
using Sanoid.Common.Configuration.Snapshots;

namespace Sanoid;

internal static class SnapshotTasks
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    internal static void TakeAllConfiguredSnapshots( Configuration config )
    {
        ConcurrentQueue<Dataset> wantedRoots = new( );
        Logger.Debug( "Building Dataset queue for snapshots" );
        foreach ( ( string _, Dataset pool ) in config.Pools )
        {
            Logger.Debug( "Looking for first wanted dataset in pool {0}", pool.Path );
            if ( pool.GetFirstWanted( ) is { } processRootDs )
            {
                Logger.Debug( "{0} is highest-level wanted dataset in pool {1}. Adding to queue.",processRootDs.Path, pool.Path );
                wantedRoots.Enqueue( processRootDs );
                continue;
            }

            Logger.Debug( "Neither {0} nor any of its children are wanted for snapshots.", pool.Path );
        }
        Logger.Debug( "Finished building Dataset queue for snapshots" );
    }

    internal static async Task<TakeSnapshotTaskResult> TakeSnapshot(Configuration config, Dataset ds )
    {
        throw new NotImplementedException( );
    }
}

internal class TakeSnapshotTaskResult
{
}

internal class TakeAllConfiguredSnapshotsTaskResult
{
    internal ConcurrentDictionary<string, Snapshot> Snapshots { get; } = new( );
}
