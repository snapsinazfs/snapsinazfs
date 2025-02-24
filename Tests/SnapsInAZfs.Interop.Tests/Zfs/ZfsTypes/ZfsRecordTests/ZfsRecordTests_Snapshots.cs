// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Collections.Concurrent;
using SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes.SnapshotTests;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes.ZfsRecordTests;

[TestFixture]
[Category( "General" )]
[Parallelizable( ParallelScope.Self )]
[TestOf( typeof( ZfsRecord ) )]
public class ZfsRecordTests_Snapshots
{
    [Test]
    [TestCase( SnapshotPeriodKind.Frequent, "2023-01-01T00:00:00.0000000" )]
    [TestCase( SnapshotPeriodKind.Hourly, "2023-01-01T00:01:00.0000000" )]
    [TestCase( SnapshotPeriodKind.Daily, "2023-01-02T00:00:00.0000000" )]
    [TestCase( SnapshotPeriodKind.Weekly, "2023-01-08T00:00:00.0000000" )]
    [TestCase( SnapshotPeriodKind.Monthly, "2023-02-01T00:00:00.0000000" )]
    [TestCase( SnapshotPeriodKind.Yearly, "2024-01-01T00:00:00.0000000" )]
    public void AddSnapshot_LastObservedTimestampsUpdated( SnapshotPeriodKind periodKind, DateTimeOffset timestamp )
    {
        Assume.That( timestamp, Is.GreaterThan( DateTimeOffset.UnixEpoch ) );
        Assume.That( periodKind, Is.Not.EqualTo( SnapshotPeriodKind.NotSet ) );
        ZfsRecord parentDs = ZfsRecordTestHelpers.GetNewTestRootFileSystem("testRoot" );
        ZfsRecord childDs = parentDs.CreateChildDataset( "testRoot/fs1", ZfsPropertyValueConstants.FileSystem, "testSystem" );
        Assume.That( childDs, Is.Not.Null );
        Assume.That( childDs.Snapshots, Is.Not.Null );
        Assume.That( childDs.Snapshots, Is.Not.Empty );
        Assume.That( childDs.Snapshots[ SnapshotPeriodKind.Frequent ], Is.Not.Null );
        Assume.That( childDs.Snapshots[ SnapshotPeriodKind.Frequent ], Is.Empty );
        Assume.That( childDs.Snapshots[ SnapshotPeriodKind.Hourly ], Is.Not.Null );
        Assume.That( childDs.Snapshots[ SnapshotPeriodKind.Hourly ], Is.Empty );
        Assume.That( childDs.Snapshots[ SnapshotPeriodKind.Daily ], Is.Not.Null );
        Assume.That( childDs.Snapshots[ SnapshotPeriodKind.Daily ], Is.Empty );
        Assume.That( childDs.Snapshots[ SnapshotPeriodKind.Weekly ], Is.Not.Null );
        Assume.That( childDs.Snapshots[ SnapshotPeriodKind.Weekly ], Is.Empty );
        Assume.That( childDs.Snapshots[ SnapshotPeriodKind.Monthly ], Is.Not.Null );
        Assume.That( childDs.Snapshots[ SnapshotPeriodKind.Monthly ], Is.Empty );
        Assume.That( childDs.Snapshots[ SnapshotPeriodKind.Yearly ], Is.Not.Null );
        Assume.That( childDs.Snapshots[ SnapshotPeriodKind.Yearly ], Is.Empty );
        Assume.That( childDs.LastObservedFrequentSnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
        Assume.That( childDs.LastObservedHourlySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
        Assume.That( childDs.LastObservedDailySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
        Assume.That( childDs.LastObservedWeeklySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
        Assume.That( childDs.LastObservedMonthlySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
        Assume.That( childDs.LastObservedYearlySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );

        Snapshot snapshot = SnapshotTestHelpers.GetStandardTestSnapshotForParent( periodKind, timestamp, childDs );
        childDs.AddSnapshot( snapshot );

        // Disable this because the filter it with an assumption in the test itself, above
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        DateTimeOffset lastObservedSnapshotTimestampOfPeriodAfterAddSnapshot = DateTimeOffset.MinValue;
        DateTimeOffset[] otherLastObservedSnapshotTimestampsAfterAddSnapshot = [DateTimeOffset.MinValue];
        switch ( periodKind )
        {
            case SnapshotPeriodKind.Frequent:
                lastObservedSnapshotTimestampOfPeriodAfterAddSnapshot = childDs.LastObservedFrequentSnapshotTimestamp;
                otherLastObservedSnapshotTimestampsAfterAddSnapshot =
                [
                    childDs.LastObservedHourlySnapshotTimestamp,
                    childDs.LastObservedDailySnapshotTimestamp,
                    childDs.LastObservedWeeklySnapshotTimestamp,
                    childDs.LastObservedMonthlySnapshotTimestamp,
                    childDs.LastObservedYearlySnapshotTimestamp
                ];
                break;
            case SnapshotPeriodKind.Hourly:
                lastObservedSnapshotTimestampOfPeriodAfterAddSnapshot = childDs.LastObservedHourlySnapshotTimestamp;
                otherLastObservedSnapshotTimestampsAfterAddSnapshot =
                [
                    childDs.LastObservedFrequentSnapshotTimestamp,
                    childDs.LastObservedDailySnapshotTimestamp,
                    childDs.LastObservedWeeklySnapshotTimestamp,
                    childDs.LastObservedMonthlySnapshotTimestamp,
                    childDs.LastObservedYearlySnapshotTimestamp
                ];
                break;
            case SnapshotPeriodKind.Daily:
                lastObservedSnapshotTimestampOfPeriodAfterAddSnapshot = childDs.LastObservedDailySnapshotTimestamp;
                otherLastObservedSnapshotTimestampsAfterAddSnapshot =
                [
                    childDs.LastObservedFrequentSnapshotTimestamp,
                    childDs.LastObservedHourlySnapshotTimestamp,
                    childDs.LastObservedWeeklySnapshotTimestamp,
                    childDs.LastObservedMonthlySnapshotTimestamp,
                    childDs.LastObservedYearlySnapshotTimestamp
                ];
                break;
            case SnapshotPeriodKind.Weekly:
                lastObservedSnapshotTimestampOfPeriodAfterAddSnapshot = childDs.LastObservedWeeklySnapshotTimestamp;
                otherLastObservedSnapshotTimestampsAfterAddSnapshot =
                [
                    childDs.LastObservedFrequentSnapshotTimestamp,
                    childDs.LastObservedHourlySnapshotTimestamp,
                    childDs.LastObservedDailySnapshotTimestamp,
                    childDs.LastObservedMonthlySnapshotTimestamp,
                    childDs.LastObservedYearlySnapshotTimestamp
                ];
                break;
            case SnapshotPeriodKind.Monthly:
                lastObservedSnapshotTimestampOfPeriodAfterAddSnapshot = childDs.LastObservedMonthlySnapshotTimestamp;
                otherLastObservedSnapshotTimestampsAfterAddSnapshot =
                [
                    childDs.LastObservedFrequentSnapshotTimestamp,
                    childDs.LastObservedHourlySnapshotTimestamp,
                    childDs.LastObservedDailySnapshotTimestamp,
                    childDs.LastObservedWeeklySnapshotTimestamp,
                    childDs.LastObservedYearlySnapshotTimestamp
                ];
                break;
            case SnapshotPeriodKind.Yearly:
                lastObservedSnapshotTimestampOfPeriodAfterAddSnapshot = childDs.LastObservedYearlySnapshotTimestamp;
                otherLastObservedSnapshotTimestampsAfterAddSnapshot =
                [
                    childDs.LastObservedFrequentSnapshotTimestamp,
                    childDs.LastObservedHourlySnapshotTimestamp,
                    childDs.LastObservedDailySnapshotTimestamp,
                    childDs.LastObservedWeeklySnapshotTimestamp,
                    childDs.LastObservedMonthlySnapshotTimestamp
                ];
                break;
        }

        Assert.Multiple( ( ) =>
        {
            Assert.That( lastObservedSnapshotTimestampOfPeriodAfterAddSnapshot, Is.EqualTo( timestamp ) );
            Assert.That( otherLastObservedSnapshotTimestampsAfterAddSnapshot, Has.All.EqualTo( DateTimeOffset.UnixEpoch ) );
        } );
    }

    [Test]
    [Parallelizable]
    [TestCase( SnapshotPeriodKind.Frequent )]
    [TestCase( SnapshotPeriodKind.Hourly )]
    [TestCase( SnapshotPeriodKind.Daily )]
    [TestCase( SnapshotPeriodKind.Weekly )]
    [TestCase( SnapshotPeriodKind.Monthly )]
    [TestCase( SnapshotPeriodKind.Yearly )]
    public void AddSnapshot_SnapshotsInCorrectCollection( SnapshotPeriodKind periodKind )
    {
        ZfsRecord parentDs = ZfsRecordTestHelpers.GetNewTestRootFileSystem("testRoot" );
        ZfsRecord childDs = parentDs.CreateChildDataset( "testRoot/fs1", ZfsPropertyValueConstants.FileSystem, "testSystem" );
        Snapshot snapshot = SnapshotTestHelpers.GetStandardTestSnapshotForParent( periodKind, DateTimeOffset.UnixEpoch, childDs );
        childDs.AddSnapshot( snapshot );
        Assert.Multiple( ( ) =>
        {
            foreach ( (SnapshotPeriodKind period, ConcurrentDictionary<string, Snapshot>? snapsInPeriod ) in childDs.Snapshots )
            {
                if ( period != periodKind )
                {
                    Assert.That( childDs.Snapshots[ period ], Does.Not.ContainKey( snapshot.Name ) );
                    Assert.That( childDs.Snapshots[ period ], Does.Not.ContainValue( snapshot ) );
                }
                else
                {
                    Assert.That( snapsInPeriod, Does.ContainKey( snapshot.Name ) );
                    Assert.That( snapsInPeriod, Does.ContainValue( snapshot ) );
                }
            }
        });
    }

    [Test]
    public void DeepCopyClone_ThrowsArgumentNullExceptionOnNullParent ( )
    {
        ZfsRecord parentDs = ZfsRecordTestHelpers.GetNewTestRootFileSystem ( );
        Snapshot  snapshot = SnapshotTestHelpers.GetStandardTestSnapshotForParent ( SnapshotPeriodKind.Daily, DateTimeOffset.UnixEpoch, parentDs );
        parentDs.AddSnapshot ( snapshot );

        Assume.That ( parentDs,                                                          Is.Not.Null.And.TypeOf<ZfsRecord> ( ) );
        Assume.That ( snapshot,                                                          Is.Not.Null.And.TypeOf<Snapshot> ( ) );
        Assume.That ( snapshot.ParentDataset,                                            Is.Not.Null.And.SameAs ( parentDs ) );
        Assume.That ( parentDs.Snapshots,                                                Is.Not.Null.And.Not.Empty );
        Assume.That ( parentDs.Snapshots,                                                Does.ContainKey ( SnapshotPeriodKind.Daily ) );
        Assume.That ( parentDs.Snapshots [ SnapshotPeriodKind.Daily ],                   Is.Not.Null.And.Not.Empty );
        Assume.That ( parentDs.Snapshots [ SnapshotPeriodKind.Daily ],                   Does.ContainKey ( snapshot.Name ) );
        Assume.That ( parentDs.Snapshots [ SnapshotPeriodKind.Daily ] [ snapshot.Name ], Is.Not.Null );
        Assume.That ( parentDs.Snapshots [ SnapshotPeriodKind.Daily ] [ snapshot.Name ], Is.SameAs ( snapshot ) );

        Assert.That ( ( ) => snapshot.DeepCopyClone ( ), Throws.ArgumentNullException );
    }

    [Test]
    public void DeepCopyClone_ThrowsArgumentExceptionOnSnapshotParent ( )
    {
        ZfsRecord parentDs = ZfsRecordTestHelpers.GetNewTestRootFileSystem ( );
        Snapshot  snapshot = SnapshotTestHelpers.GetStandardTestSnapshotForParent ( SnapshotPeriodKind.Daily, DateTimeOffset.UnixEpoch, parentDs );
        parentDs.AddSnapshot ( snapshot );

        Assume.That ( parentDs,                                                          Is.Not.Null.And.TypeOf<ZfsRecord> ( ) );
        Assume.That ( snapshot,                                                          Is.Not.Null.And.TypeOf<Snapshot> ( ) );
        Assume.That ( snapshot.ParentDataset,                                            Is.Not.Null.And.SameAs ( parentDs ) );
        Assume.That ( parentDs.Snapshots,                                                Is.Not.Null.And.Not.Empty );
        Assume.That ( parentDs.Snapshots,                                                Does.ContainKey ( SnapshotPeriodKind.Daily ) );
        Assume.That ( parentDs.Snapshots [ SnapshotPeriodKind.Daily ],                   Is.Not.Null.And.Not.Empty );
        Assume.That ( parentDs.Snapshots [ SnapshotPeriodKind.Daily ],                   Does.ContainKey ( snapshot.Name ) );
        Assume.That ( parentDs.Snapshots [ SnapshotPeriodKind.Daily ] [ snapshot.Name ], Is.Not.Null );
        Assume.That ( parentDs.Snapshots [ SnapshotPeriodKind.Daily ] [ snapshot.Name ], Is.SameAs ( snapshot ) );

        Assert.That ( ( ) => snapshot.DeepCopyClone ( snapshot ), Throws.ArgumentException.With.InnerException.InstanceOf<NotSupportedException> ( ) );
    }

    [Test]
    [Parallelizable]
    public void GetSnapshotsToPrune_NoSnapshotsInDataset( )
    {
        ZfsRecord parentDs = ZfsRecordTestHelpers.GetNewTestRootFileSystem("testRoot" );
        ZfsRecord childDs = parentDs.CreateChildDataset( "testRoot/fs1", ZfsPropertyValueConstants.FileSystem, "testSystem" );
        Assume.That( childDs, Is.Not.Null );
        Assume.That( childDs.Snapshots, Is.Not.Null );
        Assume.That( childDs.Snapshots, Is.Not.Empty );
        Assume.That( childDs.Snapshots[ SnapshotPeriodKind.Frequent ], Is.Not.Null );
        Assume.That( childDs.Snapshots[ SnapshotPeriodKind.Frequent ], Is.Empty );
        Assume.That( childDs.Snapshots[ SnapshotPeriodKind.Hourly ], Is.Not.Null );
        Assume.That( childDs.Snapshots[ SnapshotPeriodKind.Hourly ], Is.Empty );
        Assume.That( childDs.Snapshots[ SnapshotPeriodKind.Daily ], Is.Not.Null );
        Assume.That( childDs.Snapshots[ SnapshotPeriodKind.Daily ], Is.Empty );
        Assume.That( childDs.Snapshots[ SnapshotPeriodKind.Weekly ], Is.Not.Null );
        Assume.That( childDs.Snapshots[ SnapshotPeriodKind.Weekly ], Is.Empty );
        Assume.That( childDs.Snapshots[ SnapshotPeriodKind.Monthly ], Is.Not.Null );
        Assume.That( childDs.Snapshots[ SnapshotPeriodKind.Monthly ], Is.Empty );
        Assume.That( childDs.Snapshots[ SnapshotPeriodKind.Yearly ], Is.Not.Null );
        Assume.That( childDs.Snapshots[ SnapshotPeriodKind.Yearly ], Is.Empty );
        List<Snapshot> snapshotsToPrune = childDs.GetSnapshotsToPrune( );
        Assert.That( snapshotsToPrune, Is.Empty );
    }

    [Test]
    [NonParallelizable]
    [TestCase( "2020-01-01T00:00:00.0000000", ExpectedResult = false )]
    [TestCase( "2023-07-02T23:59:59.9999999", ExpectedResult = false )]
    [TestCase( "2023-07-03T00:00:00.0000000", ExpectedResult = false )]
    [TestCase( "2023-07-03T23:59:59.9999999", ExpectedResult = false )]
    [TestCase( "2023-07-04T00:00:00.0000000", ExpectedResult = true )]
    [TestCase( "2025-07-05T00:00:00.0000000", ExpectedResult = true )]
    public bool IsDailySnapshotNeeded( DateTimeOffset timestamp )
    {
        ZfsRecord newTestRootFileSystem = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        return newTestRootFileSystem.IsDailySnapshotNeeded( timestamp );
    }

    [Test]
    [Parallelizable]
    [TestCase( "2020-01-01T00:00:00.0000000", ExpectedResult = false )]
    [TestCase( "2023-07-03T01:14:59.9999999", ExpectedResult = false )]
    [TestCase( "2023-07-03T01:15:00.0000000", ExpectedResult = false )]
    [TestCase( "2023-07-03T01:29:59.9999999", ExpectedResult = false )]
    [TestCase( "2023-07-03T01:30:00.0000000", ExpectedResult = true )]
    [TestCase( "2023-07-03T01:31:00.0000000", ExpectedResult = true )]
    public bool IsFrequentSnapshotNeeded( DateTimeOffset timestamp )
    {
        ZfsRecord newTestRootFileSystem = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        return newTestRootFileSystem.IsFrequentSnapshotNeeded(SnapshotTimingSettings.GetDefault(),  timestamp );
    }

    [Test]
    [Parallelizable]
    [TestCase( "2020-01-01T00:00:00.0000000", ExpectedResult = false )]
    [TestCase( "2023-07-03T00:59:59.9999999", ExpectedResult = false )]
    [TestCase( "2023-07-03T01:00:00.0000000", ExpectedResult = false )]
    [TestCase( "2023-07-03T01:59:59.9999999", ExpectedResult = false )]
    [TestCase( "2023-07-03T02:00:00.0000000", ExpectedResult = true )]
    [TestCase( "2024-07-03T02:00:00.0000000", ExpectedResult = true )]
    public bool IsHourlySnapshotNeeded( DateTimeOffset timestamp )
    {
        ZfsRecord newTestRootFileSystem = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        return newTestRootFileSystem.IsHourlySnapshotNeeded( timestamp );
    }

    [Test]
    [Parallelizable]
    [TestCase( "2020-01-01T00:00:00.0000000", ExpectedResult = false )]
    [TestCase( "2023-06-30T23:59:59.9999999", ExpectedResult = false )]
    [TestCase( "2023-07-01T00:00:00.0000000", ExpectedResult = false )]
    [TestCase( "2023-07-31T23:59:59.9999999", ExpectedResult = false )]
    [TestCase( "2024-08-01T00:00:00.0000000", ExpectedResult = true )]
    [TestCase( "2025-01-01T00:00:00.0000000", ExpectedResult = true )]
    public bool IsMonthlySnapshotNeeded( DateTimeOffset timestamp )
    {
        ZfsRecord newTestRootFileSystem = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        return newTestRootFileSystem.IsMonthlySnapshotNeeded( timestamp );
    }

    [Test]
    [Parallelizable]
    [TestCase( "2020-01-01T00:00:00.0000000", ExpectedResult = false )]
    [TestCase( "2023-07-02T23:59:59.9999999", ExpectedResult = false )]
    [TestCase( "2023-07-03T00:00:00.0000000", ExpectedResult = false )]
    [TestCase( "2023-07-09T23:59:59.9999999", ExpectedResult = false )]
    [TestCase( "2023-07-10T00:00:00.0000000", ExpectedResult = true )]
    [TestCase( "2023-07-11T00:00:00.0000000", ExpectedResult = true )]
    public bool IsWeeklySnapshotNeeded( DateTimeOffset timestamp )
    {
        ZfsRecord newTestRootFileSystem = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        return newTestRootFileSystem.IsWeeklySnapshotNeeded( SnapshotTimingSettings.GetDefault( ), timestamp );
    }

    [Test]
    [Parallelizable]
    [TestCase( "2020-01-01T00:00:00.0000000", ExpectedResult = false )]
    [TestCase( "2022-12-31T23:59:59.9999999", ExpectedResult = false )]
    [TestCase( "2023-01-01T00:00:00.0000000", ExpectedResult = false )]
    [TestCase( "2023-12-31T23:59:59.9999999", ExpectedResult = false )]
    [TestCase( "2024-01-01T00:00:00.0000000", ExpectedResult = true )]
    [TestCase( "2025-01-01T00:00:00.0000000", ExpectedResult = true )]
    public bool IsYearlySnapshotNeeded( DateTimeOffset timestamp )
    {
        ZfsRecord newTestRootFileSystem = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        return newTestRootFileSystem.IsYearlySnapshotNeeded( timestamp );
    }

    [Test]
    [Parallelizable]
    public void Snapshots_CollectionInitializedProperly( )
    {
        ZfsRecord dataset = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
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
    [Parallelizable]
    [TestCase( SnapshotPeriodKind.Frequent )]
    [TestCase( SnapshotPeriodKind.Hourly )]
    [TestCase( SnapshotPeriodKind.Daily )]
    [TestCase( SnapshotPeriodKind.Weekly )]
    [TestCase( SnapshotPeriodKind.Monthly )]
    [TestCase( SnapshotPeriodKind.Yearly )]
    public void Snapshots_SubCollectionsInitializedCorrectly( SnapshotPeriodKind periodKind )
    {
        ZfsRecord dataset = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );

        Assume.That( dataset, Is.Not.Null );
        Assume.That( dataset.Snapshots, Is.Not.Null );
        Assume.That( dataset.Snapshots, Is.Not.Empty );
        Assume.That( dataset.Snapshots, Has.Count.EqualTo( 6 ) );
        Assume.That( dataset.Snapshots, Does.ContainKey( periodKind ) );

        Assert.Multiple( ( ) =>
        {
            Assert.That( dataset.Snapshots[ periodKind ], Is.Not.Null );
            Assert.That( dataset.Snapshots[ periodKind ], Is.InstanceOf<ConcurrentDictionary<string, Snapshot>>( ) );
        } );
        Assert.That( dataset.Snapshots[ periodKind ], Is.Empty );
    }
}
