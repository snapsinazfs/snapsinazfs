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
using System.Runtime.CompilerServices;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs;

/// <summary>
/// Dummy command runner used for testing when running on windows or wherever zfs isn't installed
/// </summary>
public sealed class DummyZfsCommandRunner : ZfsCommandRunnerBase
{
    // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
    private readonly string _zfsPath;
    private readonly string _zpoolPath;
    // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

    /// <exception cref="ArgumentNullException">Path to zfs or zpool utility cannot be null <paramref name="pathToZfs"/></exception>
    public DummyZfsCommandRunner( string pathToZfs, string pathToZpool )
    {
        if ( string.IsNullOrWhiteSpace( pathToZfs ) )
        {
            throw new ArgumentNullException( nameof( pathToZfs ), "Path to zfs utility cannot be null" );
        }
        if ( string.IsNullOrWhiteSpace( pathToZpool ) )
        {
            throw new ArgumentNullException( nameof( pathToZpool ), "Path to zpool utility cannot be null" );
        }

        _zfsPath = pathToZfs;
        _zpoolPath = pathToZpool;
        Logger.Debug( "DummyZfsCommandRunner created with fake ZFS utilities at {0} and {1}", _zfsPath, _zpoolPath );
    }

    // ReSharper disable RedundantAwait
    // ReSharper disable AsyncConverter.AsyncAwaitMayBeElidedHighlighting
    /// <inheritdoc />
    /// <remarks>
    /// This method always either succeeds or reports DryRun.
    /// </remarks>
    public override async Task<ZfsCommandRunnerOperationStatus> DestroySnapshotAsync( Snapshot snapshot, SnapsInAZfsSettings settings )
    {
        await Task.Delay( 100 ).ConfigureAwait( true );
        return await Task.FromResult( settings.DryRun ? ZfsCommandRunnerOperationStatus.DryRun : ZfsCommandRunnerOperationStatus.Success ).ConfigureAwait( true );
    }
    // ReSharper restore AsyncConverter.AsyncAwaitMayBeElidedHighlighting
    // ReSharper restore RedundantAwait

    /// <inheritdoc />
    public override async Task GetDatasetsAndSnapshotsFromZfsAsync( SnapsInAZfsSettings settings, ConcurrentDictionary<string, ZfsRecord> datasets, ConcurrentDictionary<string, Snapshot> snapshots )
    {
        string propertiesString = IZfsProperty.KnownDatasetProperties.Union( IZfsProperty.KnownSnapshotProperties ).ToCommaSeparatedSingleLineString( );
        Logger.Debug( "Pretending to run zfs get type,{0},available,used -H -p -r -t filesystem,volume,snapshot", propertiesString );
        ConfiguredCancelableAsyncEnumerable<string> lineProvider = ZfsExecEnumeratorAsync( "get", "fullZfsGet.txt" ).ConfigureAwait( true );
        SortedDictionary<string, RawZfsObject> rawObjects = new( );
        await GetRawZfsObjectsAsync( lineProvider, rawObjects ).ConfigureAwait( true );
        ProcessRawObjects( rawObjects, datasets, snapshots );
        CheckAndUpdateLastSnapshotTimesForDatasets( settings, datasets );
    }

    /// <inheritdoc />
    public override Task<ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>> GetPoolRootsAndPropertyValiditiesAsync( )
    {
        return GetPoolRootsAndPropertyValiditiesAsync( "poolroots-withproperties.txt" );
    }

    /// <inheritdoc />
    public override Task<ZfsCommandRunnerOperationStatus> InheritZfsPropertyAsync( bool dryRun, string zfsPath, IZfsProperty propertyToInherit )
    {
        // Just pretend it succeeded or, if dryRun specified, return that for consistency with the real thing
        return Task.FromResult( dryRun ? ZfsCommandRunnerOperationStatus.DryRun : ZfsCommandRunnerOperationStatus.Success );
    }

    /// <inheritdoc />
    public override bool SetDefaultValuesForMissingZfsPropertiesOnPoolAsync( SnapsInAZfsSettings settings, string poolName, string[] propertyArray )
    {
        return !settings.DryRun;
    }

    /// <inheritdoc />
    public override Task<ZfsCommandRunnerOperationStatus> SetZfsPropertiesAsync( bool dryRun, string zfsPath, List<IZfsProperty> properties )
    {
        // Just pretend it succeeded or, if dryRun specified, return that for consistency with the real thing
        return Task.FromResult( dryRun ? ZfsCommandRunnerOperationStatus.DryRun : ZfsCommandRunnerOperationStatus.Success );
    }

    /// <inheritdoc />
    public override Task<ZfsCommandRunnerOperationStatus> SetZfsPropertiesAsync( bool dryRun, string zfsPath, params IZfsProperty[] properties )
    {
        // Just pretend it succeeded or, if dryRun specified, return that for consistency with the real thing
        return Task.FromResult( dryRun ? ZfsCommandRunnerOperationStatus.DryRun : ZfsCommandRunnerOperationStatus.Success );
    }

    /// <inheritdoc />
    public override ZfsCommandRunnerOperationStatus TakeSnapshot( ZfsRecord ds, SnapshotPeriod period, in DateTimeOffset timestamp, SnapsInAZfsSettings snapsInAZfsSettings, FormattingSettings datasetFormattingSettings, out Snapshot? snapshot )
    {
        bool zfsRecursionWanted = ds.Recursion.Value == ZfsPropertyValueConstants.ZfsRecursion;
        Logger.Debug( "{0:G} {2}snapshot requested for dataset {1}", period.Kind, ds.Name, zfsRecursionWanted ? "recursive " : "" );
        string snapName = datasetFormattingSettings.GenerateFullSnapshotName( ds.Name, period.Kind, timestamp );
        ZfsProperty<string> snapshotSourceSystem = ZfsProperty<string>.CreateWithoutParent( ZfsPropertyNames.SourceSystem, snapsInAZfsSettings.LocalSystemName );
        snapshot = new( snapName, in period.Kind, in snapshotSourceSystem, in timestamp, ds );
        if ( snapsInAZfsSettings.DryRun )
        {
            string arguments = $"snapshot {( zfsRecursionWanted ? "-r " : "" )}{snapshot.GetSnapshotOptionsStringForZfsSnapshot( )} {snapshot.Name}";
            Logger.Info( "DRY RUN: Would execute `{0} {1}`", snapsInAZfsSettings.ZfsPath, $"snapshot {arguments}" );
            return ZfsCommandRunnerOperationStatus.DryRun;
        }
        return ZfsCommandRunnerOperationStatus.Success;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="verb" /> is <see langword="null" />.</exception>
    /// <exception cref="IOException">Invalid attempt to read when no data present</exception>
    public override async IAsyncEnumerable<string> ZfsExecEnumeratorAsync( string verb, string args )
    {
        ArgumentException.ThrowIfNullOrEmpty( nameof( verb ), "Verb cannot be null or empty" );

        if ( verb is not ("get" or "list") )
        {
            yield break;
        }

        Logger.Trace( "Preparing to execute `{0} {1} {2}` and yield an enumerator for output", "zfs", verb, args );
        Logger.Debug( "Calling zfs {0} {1}", verb, args );
        using StreamReader zfsProcess = File.OpenText( args );

        while ( !zfsProcess.EndOfStream )
        {
            yield return await zfsProcess.ReadLineAsync( ).ConfigureAwait( true ) ?? throw new IOException( "Invalid attempt to read when no data present" );
        }
    }

    /// <inheritdoc />
    public override IAsyncEnumerable<string> ZpoolExecEnumerator( string verb, string args )
    {
        throw new NotImplementedException( );
    }
}
