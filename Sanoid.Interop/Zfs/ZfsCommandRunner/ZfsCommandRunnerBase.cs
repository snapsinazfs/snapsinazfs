// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;
using NLog;
using Sanoid.Interop.Zfs.ZfsTypes;
using Sanoid.Settings.Settings;
using Terminal.Gui.Trees;

namespace Sanoid.Interop.Zfs.ZfsCommandRunner;

public abstract class ZfsCommandRunnerBase : IZfsCommandRunner
{
    protected static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <inheritdoc />
    public abstract bool TakeSnapshot( Dataset ds, SnapshotPeriod period, DateTimeOffset timestamp, SanoidSettings settings, out Snapshot snapshot );

    /// <inheritdoc />
    public abstract Task<bool> DestroySnapshotAsync( Snapshot snapshot, SanoidSettings settings );

    /// <inheritdoc />
    public abstract Task<bool> GetPoolCapacitiesAsync( ConcurrentDictionary<string, Dataset> datasets );

    /// <inheritdoc />
    public abstract bool SetZfsProperties( bool dryRun, string zfsPath, params ZfsProperty[] properties );

    /// <inheritdoc />
    public abstract bool SetZfsProperties( bool dryRun, string zfsPath, List<IZfsProperty> properties );

    /// <inheritdoc />
    public abstract Dictionary<string, Dataset> GetZfsDatasetConfiguration( string args = " -r" );

    /// <inheritdoc />
    public abstract Task<ConcurrentDictionary<string, Dataset>> GetPoolRootDatasetsWithAllRequiredSanoidPropertiesAsync( );

    /// <inheritdoc />
    public abstract Task GetDatasetsAndSnapshotsFromZfsAsync( ConcurrentDictionary<string, Dataset> datasets, ConcurrentDictionary<string, Snapshot> snapshots );

    public abstract IAsyncEnumerable<string> ZpoolExecEnumerator( string verb, string args );
    public abstract IEnumerable<string> ZfsExecEnumerator( string verb, string args );
    public abstract List<ITreeNode> GetZfsObjectsForConfigConsoleTree( ConcurrentDictionary<string, SanoidZfsDataset> datasets );
}
