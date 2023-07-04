// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes;

public class TestCommandRunner : DummyZfsCommandRunner
{
    public override async Task GetDatasetsAndSnapshotsFromZfsAsync( SnapsInAZfsSettings settings, ConcurrentDictionary<string, ZfsRecord> datasets, ConcurrentDictionary<string, Snapshot> snapshots )
    {
        string propertiesString = IZfsProperty.KnownDatasetProperties.Union( IZfsProperty.KnownSnapshotProperties ).ToCommaSeparatedSingleLineString( );
        Logger.Debug( "Pretending to run zfs get type,{0},available,used -H -p -r -t filesystem,volume,snapshot", propertiesString );
        ConfiguredCancelableAsyncEnumerable<string> lineProvider = ZfsExecEnumeratorAsync( "get", "testData-WithSnapshotsToPrune.txt" ).ConfigureAwait( true );
        SortedDictionary<string, RawZfsObject> rawObjects = new( );
        await GetRawZfsObjectsAsync( lineProvider, rawObjects ).ConfigureAwait( true );
        ProcessRawObjects( rawObjects, datasets, snapshots );
        CheckAndUpdateLastSnapshotTimesForDatasets( settings, datasets );
    }
}
