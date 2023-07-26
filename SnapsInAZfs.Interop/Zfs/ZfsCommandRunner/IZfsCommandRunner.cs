// LICENSE:
// 
// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.Json;
using NLog;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;

/// <summary>
///     Base class for classes that call native ZFS utilities from the system.
/// </summary>
/// <remarks>
///     Default implementations of command functions return mocked values.
/// </remarks>
public interface IZfsCommandRunner
{
    /// <summary>
    ///     Destroys a zfs snapshot
    /// </summary>
    /// <returns>
    ///     A boolean value indicating whether the operation succeeded (ie no exceptions were thrown).
    /// </returns>
    public Task<ZfsCommandRunnerOperationStatus> DestroySnapshotAsync( Snapshot snapshot, SnapsInAZfsSettings settings );

    /// <summary>
    ///     Gets everything SnapsInAZfs cares about from ZFS
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="datasets">A collection of datasets for this method to finish populating.</param>
    /// <param name="snapshots">A collection of snapshots for this method to populate</param>
    public Task GetDatasetsAndSnapshotsFromZfsAsync( SnapsInAZfsSettings settings, ConcurrentDictionary<string, ZfsRecord> datasets, ConcurrentDictionary<string, Snapshot> snapshots );

    /// <summary>
    ///     Gets a collection of datasets and their property validity
    /// </summary>
    /// <returns>
    ///     A <see cref="ConcurrentDictionary{TKey,TValue}" /> of TKey=<see langword="string" />s, as pool root names, to
    ///     TValue=<see cref="ConcurrentDictionary{TKey,TValue}" /> of TKey=<see langword="string" />s, as property names, to
    ///     TValue=<see langword="bool" />s indicating whether that property is defined and has a valid value for its type
    /// </returns>
    Task<ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>> GetPoolRootsAndPropertyValiditiesAsync( );

    /// <summary>
    ///     Inherits the provided <see cref="IZfsProperty" /> for <paramref name="zfsPath" />
    /// </summary>
    /// <param name="dryRun">
    ///     If true, instructs the method not to actually call the ZFS utility, but instead just report what
    ///     it <em>would</em> have done.
    /// </param>
    /// <param name="zfsPath">The fully-qualified path to operate on</param>
    /// <param name="propertyToInherit">
    ///     An <see cref="IZfsProperty" /> objects to inherit from the parent of <paramref name="zfsPath" />
    /// </param>
    /// <returns>
    ///     If <paramref name="dryRun" /> is <see langword="true" />: Always returns <see langword="false" /><br />
    ///     Otherwise, a <see langword="bool" /> indicating success or failure of the operation
    /// </returns>
    public Task<ZfsCommandRunnerOperationStatus> InheritZfsPropertyAsync( bool dryRun, string zfsPath, IZfsProperty propertyToInherit );

    bool SetDefaultValuesForMissingZfsPropertiesOnPoolAsync( bool dryRun, string poolName, string[] propertyArray );

    /// <summary>
    ///     Sets the provided <see cref="IZfsProperty" /> values for <paramref name="zfsPath" />
    /// </summary>
    /// <param name="dryRun">
    ///     If true, instructs the method not to actually call the ZFS utility, but instead just report what
    ///     it <em>would</em> have done.
    /// </param>
    /// <param name="zfsPath">The fully-qualified path to operate on</param>
    /// <param name="properties">A parameterized array of <see cref="IZfsProperty" /> objects to set</param>
    /// <returns>
    ///     A <see langword="bool" /> indicating success or failure of the operation.
    /// </returns>
    public Task<ZfsCommandRunnerOperationStatus> SetZfsPropertiesAsync( bool dryRun, string zfsPath, params IZfsProperty[] properties );

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
    public Task<ZfsCommandRunnerOperationStatus> SetZfsPropertiesAsync( bool dryRun, string zfsPath, List<IZfsProperty> properties );

    /// <summary>
    ///     Creates a zfs snapshot
    /// </summary>
    /// <returns>
    ///     A boolean value indicating whether the operation succeeded (ie no exceptions were thrown).
    /// </returns>
    public ZfsCommandRunnerOperationStatus TakeSnapshot( ZfsRecord ds, SnapshotPeriod period, in DateTimeOffset timestamp, SnapsInAZfsSettings snapsInAZfsSettings, TemplateSettings datasetTemplate, out Snapshot? snapshot );

    public IAsyncEnumerable<string> ZfsExecEnumeratorAsync( string verb, string args );

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

    public IAsyncEnumerable<string> ZpoolExecEnumerator( string verb, string args );
}
