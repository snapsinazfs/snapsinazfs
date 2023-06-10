// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.Json;
using NLog;
using Sanoid.Interop.Zfs.ZfsTypes;
using Sanoid.Settings.Settings;
using Terminal.Gui.Trees;

namespace Sanoid.Interop.Zfs.ZfsCommandRunner;

/// <summary>
///     Base class for classes that call native ZFS utilities from the system.
/// </summary>
/// <remarks>
///     Default implementations of command functions return mocked values.
/// </remarks>
public interface IZfsCommandRunner
{
    /// <summary>
    ///     Gets a list of ZFS datasets (filesystems and volumes)
    /// </summary>
    /// <returns>
    ///     An <see cref="ImmutableSortedSet{T}" /> of <see langword="string" />s, each representing the ZFS path of a dataset
    ///     on the system.
    /// </returns>
    ImmutableSortedSet<string> ZfsListAll( )
    {
        ImmutableSortedSet<string> dataSets = ImmutableSortedSet<string>.Empty.Union( new[] { "pool1", "pool1/dataset1", "pool1/dataset1/leaf", "pool1/dataset2", "pool1/dataset3", "pool1/zvol1" } );
        LogManager.GetCurrentClassLogger( ).Warn( "Running on windows. Returning fake datasets: {0}", JsonSerializer.Serialize( dataSets ) );
        return dataSets;
    }

    /// <summary>
    ///     Creates a zfs snapshot
    /// </summary>
    /// <returns>
    ///     A boolean value indicating whether the operation succeeded (ie no exceptions were thrown).
    /// </returns>
    public bool TakeSnapshot( Dataset ds, SnapshotPeriod snapshotPeriod, DateTimeOffset timestamp, SanoidSettings settings, out Snapshot snapshot );

    /// <summary>
    ///     Destroys a zfs snapshot
    /// </summary>
    /// <returns>
    ///     A boolean value indicating whether the operation succeeded (ie no exceptions were thrown).
    /// </returns>
    public Task<bool> DestroySnapshotAsync( Snapshot snapshot, SanoidSettings settings );

    /// <summary>
    ///     Gets the capacity property from zfs for the pool roots specified and sets it on the corresponding Dataset objects
    /// </summary>
    /// <param name="datasets"></param>
    /// <returns>A boolean indicating success or failure of the operation</returns>
    public Task<bool> GetPoolCapacitiesAsync( ConcurrentDictionary<string, Dataset> datasets );

    /// <summary>
    ///     Sets the provided <see cref="ZfsProperty" /> values for <paramref name="zfsPath" />
    /// </summary>
    /// <param name="dryRun">
    ///     If true, instructs the method not to actually call the ZFS utility, but instead just report what
    ///     it <em>would</em> have done.
    /// </param>
    /// <param name="zfsPath">The fully-qualified path to operate on</param>
    /// <param name="properties">A parameterized array of <see cref="ZfsProperty" /> objects to set</param>
    /// <returns>
    ///     A <see langword="bool" /> indicating success or failure of the operation.
    /// </returns>
    public bool SetZfsProperties( bool dryRun, string zfsPath, params ZfsProperty[] properties );

    /// <summary>
    ///     Sets the provided <see cref="IZfsProperty" /> values for <paramref name="zfsPath" />
    /// </summary>
    /// <param name="dryRun">
    ///     If true, instructs the method not to actually call the ZFS utility, but instead just report what
    ///     it <em>would</em> have done.
    /// </param>
    /// <param name="zfsPath">The fully-qualified path to operate on</param>
    /// <param name="properties">
    ///     A <see cref="List{T}" /> of <see cref="IZfsProperty" /> objects to set on
    ///     <paramref name="zfsPath" />
    /// </param>
    /// <returns>
    ///     If <paramref name="dryRun" /> is <see langword="true" />: Always returns <see langword="false" /><br />
    ///     Otherwise, a <see langword="bool" /> indicating success or failure of the operation
    /// </returns>
    public bool SetZfsProperties( bool dryRun, string zfsPath, List<IZfsProperty> properties );

    /// <summary>
    ///     Gets all dataset configuration from zfs
    /// </summary>
    /// <returns>
    ///     A <see cref="Dictionary{TKey,TValue}" /> of <see langword="string" /> to <see cref="Dataset" /> of all datasets in
    ///     zfs, with sanoid.net properties populated
    /// </returns>
    public Dictionary<string, Dataset> GetZfsDatasetConfiguration( string args = " -r" );

    /// <summary>
    ///     Gets configuration defined at all pool roots
    /// </summary>
    /// <returns>
    ///     A <see cref="Dictionary{TKey,TValue}" /> of <see langword="string" /> to <see cref="Dataset" /> of pool root
    ///     datasets in
    ///     zfs, with sanoid.net properties populated
    /// </returns>
    public Task<ConcurrentDictionary<string, Dataset>> GetPoolRootDatasetsWithAllRequiredSanoidPropertiesAsync( );

    /// <summary>
    ///     Gets everything Sanoid.net cares about from ZFS, via separate processes executing in parallel using the thread pool
    /// </summary>
    /// <param name="datasets">A collection of datasets for this method to finish populating.</param>
    /// <param name="snapshots">A collection of snapshots for this method to populate</param>
    /// <remarks>Up to one additional thread per existing item in <paramref name="datasets" /> will be spawned</remarks>
    public Task GetDatasetsAndSnapshotsFromZfsAsync( ConcurrentDictionary<string, Dataset> datasets, ConcurrentDictionary<string, Snapshot> snapshots );

    public IAsyncEnumerable<string> ZpoolExecEnumerator( string verb, string args );
    public IAsyncEnumerable<string> ZfsExecEnumeratorAsync( string verb, string args );
    public Task<List<ITreeNode>> GetZfsObjectsForConfigConsoleTreeAsync( ConcurrentDictionary<string, SanoidZfsDataset> baseDatasets, ConcurrentDictionary<string, SanoidZfsDataset> treeDatasets );
}
