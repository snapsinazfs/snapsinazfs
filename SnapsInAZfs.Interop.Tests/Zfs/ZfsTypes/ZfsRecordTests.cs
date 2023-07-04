// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes;

[TestFixture]
[Category( "General" )]
public class ZfsRecordTests
{
    [Test]
    public void AddSnapshot_LastObservedTimestampsUpdated( )
    {
        Assert.Ignore( "Test not implemented" );
    }

    [Test]
    [TestCase( SnapshotPeriodKind.Frequent )]
    [TestCase( SnapshotPeriodKind.Hourly )]
    [TestCase( SnapshotPeriodKind.Daily )]
    [TestCase( SnapshotPeriodKind.Weekly )]
    [TestCase( SnapshotPeriodKind.Monthly )]
    [TestCase( SnapshotPeriodKind.Yearly )]
    public void AddSnapshot_SnapshotsInCorrectCollection( SnapshotPeriodKind period )
    {
        ZfsRecord dataset = new( "dsName", ZfsPropertyValueConstants.FileSystem );
        Snapshot snapshot = new( "frequent", true, period, DateTimeOffset.UnixEpoch, dataset );
        dataset.AddSnapshot( snapshot );
        Assert.That( dataset.Snapshots, Contains.Key( period ) );
        Assert.That( dataset.Snapshots[ period ], Contains.Key( snapshot.Name ) );
        Assert.That( dataset.Snapshots[ period ], Contains.Value( snapshot ) );
    }

    [Test]
    [TestCase( "sameName", ZfsPropertyValueConstants.FileSystem, "sameName", ZfsPropertyValueConstants.FileSystem, ExpectedResult = true )]
    [TestCase( "differentNameA", ZfsPropertyValueConstants.FileSystem, "differentNameB", ZfsPropertyValueConstants.FileSystem, ExpectedResult = false )]
    [TestCase( "sameName", ZfsPropertyValueConstants.FileSystem, "sameName", ZfsPropertyValueConstants.Volume, ExpectedResult = false )]
    [TestCase( "differentNameA", ZfsPropertyValueConstants.FileSystem, "differentNameB", ZfsPropertyValueConstants.Volume, ExpectedResult = false )]
    public bool EqualityIsByRecordValue( string datasetAName, string datasetAKind, string datasetBName, string datasetBKind )
    {
        // This test should be enhanced to check more cases of inequality
        // Providing a test case provider method would help with that
        ZfsRecord datasetA = new( datasetAName, datasetAKind );
        ZfsRecord datasetB = new( datasetBName, datasetBKind );
        return datasetA == datasetB;
    }

    [Test]
    public void GetSnapshotsToPrune( )
    {
        Assert.Ignore( "Test not implemented" );
    }

    [Test]
    public void IsDailySnapshotNeeded( )
    {
        Assert.Ignore( "Test not implemented" );
    }

    [Test]
    public void IsFrequentSnapshotNeeded( )
    {
        Assert.Ignore( "Test not implemented" );
    }

    [Test]
    public void IsHourlySnapshotNeeded( )
    {
        Assert.Ignore( "Test not implemented" );
    }

    [Test]
    public void IsMonthlySnapshotNeeded( )
    {
        Assert.Ignore( "Test not implemented" );
    }

    [Test]
    public void IsWeeklySnapshotNeeded( )
    {
        Assert.Ignore( "Test not implemented" );
    }

    [Test]
    public void IsYearlySnapshotNeeded( )
    {
        Assert.Ignore( "Test not implemented" );
    }

    [Test]
    public void UpdateProperty( )
    {
        Assert.Ignore( "Test not implemented" );
    }

    [Test]
    public void ValidateName( )
    {
        Assert.Ignore( "Test not implemented" );
    }
}
