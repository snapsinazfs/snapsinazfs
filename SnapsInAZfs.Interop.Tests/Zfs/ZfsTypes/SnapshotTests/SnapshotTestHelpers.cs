// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes.ZfsRecordTests;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes.SnapshotTests;

internal static class SnapshotTestHelpers
{
    internal static Snapshot GetStandardTestSnapshot( SnapshotPeriod period, DateTimeOffset timestamp, string parentName = "testRoot" )
    {
        ZfsRecord parent = ZfsRecordTestHelpers.GetNewTestRootFileSystem( parentName );
        return GetStandardTestSnapshotForParent( period, timestamp, parent );
    }

    internal static Snapshot GetStandardTestSnapshotForParent( SnapshotPeriod period, DateTimeOffset timestamp, ZfsRecord parent )
    {
        return parent.AddSnapshot( new( $"{parent.Name}@autosnap_{timestamp:s}_{period}", period.Kind, timestamp, parent ) );
    }
}
