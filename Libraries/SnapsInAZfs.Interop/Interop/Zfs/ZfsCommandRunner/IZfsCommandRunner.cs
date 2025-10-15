#region MIT LICENSE
// Copyright 2025 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/
#endregion

namespace SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;

using System.Collections.Concurrent;
using ZfsTypes;

public interface IZfsCommandRunner
{
  string ZfsPath { get; init; }

  /// <summary>
  ///   Gets the path to the zpool utility, set at initialization.
  /// </summary>
  string ZpoolPath { get; init; }

  /// <summary>
  ///   Destroys a zfs snapshot.
  /// </summary>
  /// <returns>
  ///   A boolean value indicating whether the operation succeeded (i.e., no exceptions were thrown).
  /// </returns>
  Task<ZfsCommandRunnerOperationStatus> DestroySnapshotAsync ( Snapshot snapshot, SnapsInAZfsSettings settings );

  /// <summary>
  ///   Gets everything SnapsInAZfs cares about from ZFS.
  /// </summary>
  /// <param name="settings"></param>
  /// <param name="datasets">A collection of datasets for this method to finish populating.</param>
  /// <param name="snapshots">A collection of snapshots for this method to populate</param>
  Task GetDatasetsAndSnapshotsFromZfsAsync (
    SnapsInAZfsSettings                     settings,
    ConcurrentDictionary<string, ZfsRecord> datasets,
    ConcurrentDictionary<string, Snapshot>  snapshots
  );

  /// <summary>
  ///   Gets a collection of datasets and their property validity
  /// </summary>
  /// <returns>
  ///   A <see cref="ConcurrentDictionary{TKey,TValue}" /> of TKey=<see langword="string" />s, as pool root names, to
  ///   TValue=<see cref="ConcurrentDictionary{TKey,TValue}" /> of TKey=<see langword="string" />s, as property names, to
  ///   TValue=<see langword="bool" />s indicating whether that property is defined and has a valid value for its type.
  /// </returns>
  Task<ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>> GetPoolRootsAndPropertyValiditiesAsync ( );

  /// <summary>
  ///   Inherits the provided <see cref="IZfsProperty" /> for <paramref name="zfsPath" />
  /// </summary>
  /// <param name="dryRun">
  ///   If true, instructs the method not to actually call the ZFS utility, but instead just report what
  ///   it <em>would</em> have done.
  /// </param>
  /// <param name="zfsPath">The fully-qualified path to operate on</param>
  /// <param name="propertyToInherit">
  ///   An <see cref="IZfsProperty" /> objects to inherit from the parent of <paramref name="zfsPath" />
  /// </param>
  /// <returns>
  ///   If <paramref name="dryRun" /> is <see langword="true" />: Always returns <see langword="false" /><br />
  ///   Otherwise, a <see langword="bool" /> indicating success or failure of the operation.
  /// </returns>
  Task<ZfsCommandRunnerOperationStatus> InheritZfsPropertyAsync ( bool dryRun, string zfsPath, IZfsProperty propertyToInherit );

  bool SetDefaultValuesForMissingZfsPropertiesOnPoolAsync ( SnapsInAZfsSettings settings, string poolName, string[] propertyArray );

  /// <summary>
  ///   Sets the provided <see cref="IZfsProperty" /> values for <paramref name="zfsPath" />
  /// </summary>
  /// <param name="dryRun">
  ///   If true, instructs the method not to actually call the ZFS utility, but instead just report what
  ///   it <em>would</em> have done.
  /// </param>
  /// <param name="zfsPath">The fully-qualified path to operate on</param>
  /// <param name="taskSemaphore">A semaphore to signal completion of all outstanding async operations.</param>
  /// <param name="properties">A parameterized array of <see cref="IZfsProperty" /> objects to set</param>
  /// <returns>
  ///   A <see langword="bool" /> indicating success or failure of the operation.
  /// </returns>
  Task<ZfsCommandRunnerOperationStatus> SetZfsPropertiesAsync ( bool dryRun, string zfsPath, SemaphoreSlim taskSemaphore, params IZfsProperty[] properties );

  /// <summary>
  ///   Sets the provided <see cref="IZfsProperty" /> values for <paramref name="zfsPath" />
  /// </summary>
  /// <param name="dryRun">
  ///   If true, instructs the method not to actually call the ZFS utility, but instead just report what
  ///   it <em>would</em> have done.
  /// </param>
  /// <param name="zfsPath">The fully-qualified path to operate on</param>
  /// <param name="properties">
  ///   A <see cref="List{T}" /> of <see cref="IZfsProperty" /> objects to set on
  ///   <paramref name="zfsPath" />
  /// </param>
  /// <returns>
  ///   If <paramref name="dryRun" /> is <see langword="true" />: Always returns <see langword="false" /><br />
  ///   Otherwise, a <see langword="bool" /> indicating success or failure of the operation.
  /// </returns>
  Task<ZfsCommandRunnerOperationStatus> SetZfsPropertiesAsync ( bool dryRun, string zfsPath, List<IZfsProperty> properties );

  /// <summary>
  ///   Creates a zfs snapshot
  /// </summary>
  /// <returns>
  ///   A boolean value indicating whether the operation succeeded (i.e., no exceptions were thrown).
  /// </returns>
  ZfsCommandRunnerOperationStatus TakeSnapshot (
    ZfsRecord           ds,
    SnapshotPeriod      period,
    in DateTimeOffset   timestamp,
    SnapsInAZfsSettings snapsInAZfsSettings,
    FormattingSettings  datasetFormattingSettings,
    out Snapshot?       snapshot
  );

  IAsyncEnumerable<string> ZfsExecEnumeratorAsync ( string verb, string args );

  IAsyncEnumerable<string> ZpoolExecEnumerator ( string verb, string args );
}

/// <summary>
///   Base class for classes that call native ZFS utilities from the system.
/// </summary>
/// <typeparam name="TRunner">Self-reference of the type, for support of static abstract properties.</typeparam>
/// <remarks>
///   Default implementations of command functions return mocked values.
/// </remarks>
public interface IZfsCommandRunner<out TRunner> : IZfsCommandRunner
  where TRunner : IZfsCommandRunner<TRunner>
{
  /// <summary>
  ///   Creates a new instance of <typeparamref name="TRunner" />.
  /// </summary>
  /// <param name="zfsPath">The path to the zfs utility.</param>
  /// <param name="zpoolPath">The path to the zpool utility.</param>
  /// <returns>
  ///   A new instance of <typeparamref name="TRunner" />, with <see cref="IZfsCommandRunner.ZfsPath" /> and <see cref="IZfsCommandRunner.ZpoolPath" /> initialized to the provided values.
  /// </returns>
  public static abstract TRunner Create ( string zfsPath, string zpoolPath );
}
