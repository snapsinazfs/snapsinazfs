// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using SnapsInAZfs.Interop.Zfs.ZfsTypes;

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes;

public class IsSnapshotNeededTestCase
{
    public IsSnapshotNeededTestCase( string testName, ZfsRecord dataset, DateTimeOffset timestamp, bool isSnapshotNeededExpected )
    {
        TestName = testName;
        Dataset = dataset;
        Timestamp = timestamp;
        IsSnapshotNeededExpected = isSnapshotNeededExpected;
    }

    public string TestName { get; set; }
    public ZfsRecord Dataset { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public bool IsSnapshotNeededExpected { get; set; }
}
