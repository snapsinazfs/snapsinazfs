// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Collections.Concurrent;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes.ZfsRecordTests;

[TestFixture]
[Order( 30 )]
[Category( "General" )]
[FixtureLifeCycle( LifeCycle.SingleInstance )]
[NonParallelizable]
[Parallelizable( ParallelScope.Children )]
[TestOf( typeof( ZfsRecord ) )]
public class ZfsRecordTests_Snapshots
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
        Snapshot snapshot = new( period.ToString( "G" ), true, period, DateTimeOffset.UnixEpoch, dataset );
        dataset.AddSnapshot( snapshot );
        Assert.That( dataset.Snapshots, Contains.Key( period ) );
        Assert.That( dataset.Snapshots[ period ], Contains.Key( snapshot.Name ) );
        Assert.That( dataset.Snapshots[ period ], Contains.Value( snapshot ) );
    }

    [Test]
    public void GetSnapshotsToPrune_NoSnapshotsInDataset( )
    {
        ZfsRecord dataset = ZfsRecordTestHelpers.TestRootFileSystemFs1 with { };
        Assume.That( dataset, Is.Not.Null );
        Assume.That( dataset.Snapshots, Is.Not.Null );
        Assume.That( dataset.Snapshots, Is.Not.Empty );
        Assume.That( dataset.Snapshots[ SnapshotPeriodKind.Frequent ], Is.Not.Null );
        Assume.That( dataset.Snapshots[ SnapshotPeriodKind.Frequent ], Is.Empty );
        Assume.That( dataset.Snapshots[ SnapshotPeriodKind.Hourly ], Is.Not.Null );
        Assume.That( dataset.Snapshots[ SnapshotPeriodKind.Hourly ], Is.Empty );
        Assume.That( dataset.Snapshots[ SnapshotPeriodKind.Daily ], Is.Not.Null );
        Assume.That( dataset.Snapshots[ SnapshotPeriodKind.Daily ], Is.Empty );
        Assume.That( dataset.Snapshots[ SnapshotPeriodKind.Weekly ], Is.Not.Null );
        Assume.That( dataset.Snapshots[ SnapshotPeriodKind.Weekly ], Is.Empty );
        Assume.That( dataset.Snapshots[ SnapshotPeriodKind.Monthly ], Is.Not.Null );
        Assume.That( dataset.Snapshots[ SnapshotPeriodKind.Monthly ], Is.Empty );
        Assume.That( dataset.Snapshots[ SnapshotPeriodKind.Yearly ], Is.Not.Null );
        Assume.That( dataset.Snapshots[ SnapshotPeriodKind.Yearly ], Is.Empty );
        List<Snapshot> snapshotsToPrune = dataset.GetSnapshotsToPrune( );
        Assert.That( snapshotsToPrune, Is.Empty );
    }

    [Test]
    [TestCase( "2020-01-01T00:00:00.0000000-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-02T23:59:59.9999999-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-03T00:00:00.0000000-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-03T23:59:59.9999999-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-04T00:00:00.0000000-07:00", ExpectedResult = true )]
    [TestCase( "2025-07-05T00:00:00.0000000-07:00", ExpectedResult = true )]
    public bool IsDailySnapshotNeeded( DateTimeOffset timestamp )
    {
        return ZfsRecordTestHelpers.TestRootFileSystem.IsDailySnapshotNeeded( timestamp );
    }

    [Test]
    [TestCase( "2020-01-01T00:00:00.0000000-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-03T01:14:59.9999999-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-03T01:15:00.0000000-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-03T01:29:59.9999999-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-03T01:30:00.0000000-07:00", ExpectedResult = true )]
    [TestCase( "2023-07-03T01:31:00.0000000-07:00", ExpectedResult = true )]
    public bool IsFrequentSnapshotNeeded( DateTimeOffset timestamp )
    {
        return ZfsRecordTestHelpers.TestRootFileSystem.IsFrequentSnapshotNeeded( SnapshotTimingSettings.GetDefault( ), timestamp );
    }

    [Test]
    [TestCase( "2020-01-01T00:00:00.0000000-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-03T00:59:59.9999999-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-03T01:00:00.0000000-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-03T01:59:59.9999999-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-03T02:00:00.0000000-07:00", ExpectedResult = true )]
    [TestCase( "2024-07-03T02:00:00.0000000-07:00", ExpectedResult = true )]
    public bool IsHourlySnapshotNeeded( DateTimeOffset timestamp )
    {
        return ZfsRecordTestHelpers.TestRootFileSystem.IsHourlySnapshotNeeded( timestamp );
    }

    [Test]
    [TestCase( "2020-01-01T00:00:00.0000000-07:00", ExpectedResult = false )]
    [TestCase( "2023-06-30T23:59:59.9999999-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-01T00:00:00.0000000-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-31T23:59:59.9999999-07:00", ExpectedResult = false )]
    [TestCase( "2024-08-01T00:00:00.0000000-07:00", ExpectedResult = true )]
    [TestCase( "2025-01-01T00:00:00.0000000-07:00", ExpectedResult = true )]
    public bool IsMonthlySnapshotNeeded( DateTimeOffset timestamp )
    {
        return ZfsRecordTestHelpers.TestRootFileSystem.IsMonthlySnapshotNeeded( timestamp );
    }

    [Test]
    [TestCase( "2020-01-01T00:00:00.0000000-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-02T23:59:59.9999999-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-03T00:00:00.0000000-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-09T23:59:59.9999999-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-10T00:00:00.0000000-07:00", ExpectedResult = true )]
    [TestCase( "2023-07-11T00:00:00.0000000-07:00", ExpectedResult = true )]
    public bool IsWeeklySnapshotNeeded( DateTimeOffset timestamp )
    {
        return ZfsRecordTestHelpers.TestRootFileSystem.IsWeeklySnapshotNeeded( SnapshotTimingSettings.GetDefault( ), timestamp );
    }

    [Test]
    [TestCase( "2020-01-01T00:00:00.0000000-07:00", ExpectedResult = false )]
    [TestCase( "2022-12-31T23:59:59.9999999-07:00", ExpectedResult = false )]
    [TestCase( "2023-01-01T00:00:00.0000000-07:00", ExpectedResult = false )]
    [TestCase( "2023-12-31T23:59:59.9999999-07:00", ExpectedResult = false )]
    [TestCase( "2024-01-01T00:00:00.0000000-07:00", ExpectedResult = true )]
    [TestCase( "2025-01-01T00:00:00.0000000-07:00", ExpectedResult = true )]
    public bool IsYearlySnapshotNeeded( DateTimeOffset timestamp )
    {
        return ZfsRecordTestHelpers.TestRootFileSystem.IsYearlySnapshotNeeded( timestamp );
    }

    [Test]
    public void Snapshots_CollectionInitializedProperly( )
    {
        ZfsRecord dataset = ZfsRecordTestHelpers.TestRootFileSystemFs1 with { };
        Assume.That( dataset, Is.Not.Null );

        Assert.Multiple( ( ) =>
        {
            Assert.That( dataset.Snapshots, Is.Not.Null );
            Assert.That( dataset.Snapshots, Is.Not.Empty );
            Assert.That( dataset.Snapshots, Has.Count.EqualTo( 6 ) );
            Assert.That( dataset.Snapshots, Does.ContainKey( SnapshotPeriodKind.Frequent ) );
            Assert.That( dataset.Snapshots, Does.ContainKey( SnapshotPeriodKind.Hourly ) );
            Assert.That( dataset.Snapshots, Does.ContainKey( SnapshotPeriodKind.Daily ) );
            Assert.That( dataset.Snapshots, Does.ContainKey( SnapshotPeriodKind.Weekly ) );
            Assert.That( dataset.Snapshots, Does.ContainKey( SnapshotPeriodKind.Monthly ) );
            Assert.That( dataset.Snapshots, Does.ContainKey( SnapshotPeriodKind.Yearly ) );
        } );
    }

    [Test]
    [TestCase( SnapshotPeriodKind.Frequent )]
    [TestCase( SnapshotPeriodKind.Hourly )]
    [TestCase( SnapshotPeriodKind.Daily )]
    [TestCase( SnapshotPeriodKind.Weekly )]
    [TestCase( SnapshotPeriodKind.Monthly )]
    [TestCase( SnapshotPeriodKind.Yearly )]
    public void Snapshots_SubCollectionsInitializedCorrectly( SnapshotPeriodKind periodKind )
    {
        ZfsRecord dataset = ZfsRecordTestHelpers.TestRootFileSystemFs1 with { };
        Assume.That( dataset, Is.Not.Null );
        Assume.That( dataset.Snapshots, Is.Not.Null );
        Assume.That( dataset.Snapshots, Is.Not.Empty );
        Assume.That( dataset.Snapshots, Has.Count.EqualTo( 6 ) );
        Assume.That( dataset.Snapshots, Does.ContainKey( periodKind ) );

        Assert.Multiple( ( ) =>
        {
            Assert.That( dataset.Snapshots[ periodKind ], Is.Not.Null );
            Assert.That( dataset.Snapshots[ periodKind ], Is.InstanceOf<ConcurrentDictionary<string, Snapshot>>( ) );
            Assert.That( dataset.Snapshots[ periodKind ], Is.Empty );
        } );
    }
}
