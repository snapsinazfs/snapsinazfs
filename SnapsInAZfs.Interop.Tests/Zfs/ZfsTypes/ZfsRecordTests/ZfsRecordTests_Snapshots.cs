// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Collections.Concurrent;
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
    [TestCase( SnapshotPeriodKind.Frequent, "2023-01-01T00:00:00.0000000-07:00" )]
    [TestCase( SnapshotPeriodKind.Hourly, "2023-01-01T00:01:00.0000000-07:00" )]
    [TestCase( SnapshotPeriodKind.Daily, "2023-01-02T00:00:00.0000000-07:00" )]
    [TestCase( SnapshotPeriodKind.Weekly, "2023-01-08T00:00:00.0000000-07:00" )]
    [TestCase( SnapshotPeriodKind.Monthly, "2023-02-01T00:00:00.0000000-07:00" )]
    [TestCase( SnapshotPeriodKind.Yearly, "2024-01-01T00:00:00.0000000-07:00" )]
    public void AddSnapshot_LastObservedTimestampsUpdated( SnapshotPeriodKind periodKind, DateTimeOffset timestamp )
    {
        Assume.That( timestamp, Is.GreaterThan( DateTimeOffset.UnixEpoch ) );
        Assume.That( periodKind, Is.Not.EqualTo( SnapshotPeriodKind.Manual ) );
        Assume.That( periodKind, Is.Not.EqualTo( SnapshotPeriodKind.Temporary ) );
        ZfsRecord dataset = ZfsRecordTestHelpers.GetNewTestRootFileSystemFs1( );
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
        Assume.That( dataset.LastObservedFrequentSnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
        Assume.That( dataset.LastObservedHourlySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
        Assume.That( dataset.LastObservedDailySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
        Assume.That( dataset.LastObservedWeeklySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
        Assume.That( dataset.LastObservedMonthlySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
        Assume.That( dataset.LastObservedYearlySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );

        Snapshot snapshot = new( periodKind.ToString( "G" ), true, periodKind, timestamp, dataset );
        dataset.AddSnapshot( snapshot );

        // Disable this because the filter it with an assumption in the test itself, above
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        DateTimeOffset lastObservedSnapshotTimestampOfPeriodAfterAddSnapshot = DateTimeOffset.MinValue;
        DateTimeOffset[] otherLastObservedSnapshotTimestampsAfterAddSnapshot = { DateTimeOffset.MinValue };
        switch ( periodKind )
        {
            case SnapshotPeriodKind.Frequent:
                lastObservedSnapshotTimestampOfPeriodAfterAddSnapshot = dataset.LastObservedFrequentSnapshotTimestamp;
                otherLastObservedSnapshotTimestampsAfterAddSnapshot = new[]
                {
                    dataset.LastObservedHourlySnapshotTimestamp,
                    dataset.LastObservedDailySnapshotTimestamp,
                    dataset.LastObservedWeeklySnapshotTimestamp,
                    dataset.LastObservedMonthlySnapshotTimestamp,
                    dataset.LastObservedYearlySnapshotTimestamp
                };
                break;
            case SnapshotPeriodKind.Hourly:
                lastObservedSnapshotTimestampOfPeriodAfterAddSnapshot = dataset.LastObservedHourlySnapshotTimestamp;
                otherLastObservedSnapshotTimestampsAfterAddSnapshot = new[]
                {
                    dataset.LastObservedFrequentSnapshotTimestamp,
                    dataset.LastObservedDailySnapshotTimestamp,
                    dataset.LastObservedWeeklySnapshotTimestamp,
                    dataset.LastObservedMonthlySnapshotTimestamp,
                    dataset.LastObservedYearlySnapshotTimestamp
                };
                break;
            case SnapshotPeriodKind.Daily:
                lastObservedSnapshotTimestampOfPeriodAfterAddSnapshot = dataset.LastObservedDailySnapshotTimestamp;
                otherLastObservedSnapshotTimestampsAfterAddSnapshot = new[]
                {
                    dataset.LastObservedFrequentSnapshotTimestamp,
                    dataset.LastObservedHourlySnapshotTimestamp,
                    dataset.LastObservedWeeklySnapshotTimestamp,
                    dataset.LastObservedMonthlySnapshotTimestamp,
                    dataset.LastObservedYearlySnapshotTimestamp
                };
                break;
            case SnapshotPeriodKind.Weekly:
                lastObservedSnapshotTimestampOfPeriodAfterAddSnapshot = dataset.LastObservedWeeklySnapshotTimestamp;
                otherLastObservedSnapshotTimestampsAfterAddSnapshot = new[]
                {
                    dataset.LastObservedFrequentSnapshotTimestamp,
                    dataset.LastObservedHourlySnapshotTimestamp,
                    dataset.LastObservedDailySnapshotTimestamp,
                    dataset.LastObservedMonthlySnapshotTimestamp,
                    dataset.LastObservedYearlySnapshotTimestamp
                };
                break;
            case SnapshotPeriodKind.Monthly:
                lastObservedSnapshotTimestampOfPeriodAfterAddSnapshot = dataset.LastObservedMonthlySnapshotTimestamp;
                otherLastObservedSnapshotTimestampsAfterAddSnapshot = new[]
                {
                    dataset.LastObservedFrequentSnapshotTimestamp,
                    dataset.LastObservedHourlySnapshotTimestamp,
                    dataset.LastObservedDailySnapshotTimestamp,
                    dataset.LastObservedWeeklySnapshotTimestamp,
                    dataset.LastObservedYearlySnapshotTimestamp
                };
                break;
            case SnapshotPeriodKind.Yearly:
                lastObservedSnapshotTimestampOfPeriodAfterAddSnapshot = dataset.LastObservedYearlySnapshotTimestamp;
                otherLastObservedSnapshotTimestampsAfterAddSnapshot = new[]
                {
                    dataset.LastObservedFrequentSnapshotTimestamp,
                    dataset.LastObservedHourlySnapshotTimestamp,
                    dataset.LastObservedDailySnapshotTimestamp,
                    dataset.LastObservedWeeklySnapshotTimestamp,
                    dataset.LastObservedMonthlySnapshotTimestamp
                };
                break;
        }

        Assert.That( lastObservedSnapshotTimestampOfPeriodAfterAddSnapshot, Is.EqualTo( timestamp ) );
        Assert.That( otherLastObservedSnapshotTimestampsAfterAddSnapshot, Has.All.EqualTo( DateTimeOffset.UnixEpoch ) );
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
        ZfsRecord dataset = ZfsRecordTestHelpers.GetNewTestRootFileSystemFs1( );
        Snapshot snapshot = new( periodKind.ToString( "G" ), true, periodKind, DateTimeOffset.UnixEpoch, dataset );
        dataset.AddSnapshot( snapshot );
        Assert.Multiple( ( ) =>
        {
            foreach ( (SnapshotPeriodKind period, ConcurrentDictionary<string, Snapshot>? snapsInPeriod ) in dataset.Snapshots )
            {
                if ( period != periodKind )
                {
                    Assert.That( dataset.Snapshots[ period ], Does.Not.ContainKey( snapshot.Name ) );
                    Assert.That( dataset.Snapshots[ period ], Does.Not.ContainValue( snapshot ) );
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
    [Parallelizable]
    public void GetSnapshotsToPrune_NoSnapshotsInDataset( )
    {
        ZfsRecord dataset = ZfsRecordTestHelpers.GetNewTestRootFileSystemFs1( );
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
    [Parallelizable]
    [TestCase( "2020-01-01T00:00:00.0000000-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-02T23:59:59.9999999-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-03T00:00:00.0000000-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-03T23:59:59.9999999-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-04T00:00:00.0000000-07:00", ExpectedResult = true )]
    [TestCase( "2025-07-05T00:00:00.0000000-07:00", ExpectedResult = true )]
    public bool IsDailySnapshotNeeded( DateTimeOffset timestamp )
    {
        ZfsRecord newTestRootFileSystem = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        return newTestRootFileSystem.IsDailySnapshotNeeded( timestamp );
    }

    [Test]
    [Parallelizable]
    [TestCase( "2020-01-01T00:00:00.0000000-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-03T01:14:59.9999999-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-03T01:15:00.0000000-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-03T01:29:59.9999999-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-03T01:30:00.0000000-07:00", ExpectedResult = true )]
    [TestCase( "2023-07-03T01:31:00.0000000-07:00", ExpectedResult = true )]
    public bool IsFrequentSnapshotNeeded( DateTimeOffset timestamp )
    {
        return ZfsRecordTestHelpers.GetNewTestRootFileSystem( ).IsFrequentSnapshotNeeded( SnapshotTimingSettings.GetDefault( ), timestamp );
    }

    [Test]
    [Parallelizable]
    [TestCase( "2020-01-01T00:00:00.0000000-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-03T00:59:59.9999999-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-03T01:00:00.0000000-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-03T01:59:59.9999999-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-03T02:00:00.0000000-07:00", ExpectedResult = true )]
    [TestCase( "2024-07-03T02:00:00.0000000-07:00", ExpectedResult = true )]
    public bool IsHourlySnapshotNeeded( DateTimeOffset timestamp )
    {
        return ZfsRecordTestHelpers.GetNewTestRootFileSystem( ).IsHourlySnapshotNeeded( timestamp );
    }

    [Test]
    [Parallelizable]
    [TestCase( "2020-01-01T00:00:00.0000000-07:00", ExpectedResult = false )]
    [TestCase( "2023-06-30T23:59:59.9999999-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-01T00:00:00.0000000-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-31T23:59:59.9999999-07:00", ExpectedResult = false )]
    [TestCase( "2024-08-01T00:00:00.0000000-07:00", ExpectedResult = true )]
    [TestCase( "2025-01-01T00:00:00.0000000-07:00", ExpectedResult = true )]
    public bool IsMonthlySnapshotNeeded( DateTimeOffset timestamp )
    {
        return ZfsRecordTestHelpers.GetNewTestRootFileSystem( ).IsMonthlySnapshotNeeded( timestamp );
    }

    [Test]
    [Parallelizable]
    [TestCase( "2020-01-01T00:00:00.0000000-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-02T23:59:59.9999999-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-03T00:00:00.0000000-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-09T23:59:59.9999999-07:00", ExpectedResult = false )]
    [TestCase( "2023-07-10T00:00:00.0000000-07:00", ExpectedResult = true )]
    [TestCase( "2023-07-11T00:00:00.0000000-07:00", ExpectedResult = true )]
    public bool IsWeeklySnapshotNeeded( DateTimeOffset timestamp )
    {
        return ZfsRecordTestHelpers.GetNewTestRootFileSystem( ).IsWeeklySnapshotNeeded( SnapshotTimingSettings.GetDefault( ), timestamp );
    }

    [Test]
    [Parallelizable]
    [TestCase( "2020-01-01T00:00:00.0000000-07:00", ExpectedResult = false )]
    [TestCase( "2022-12-31T23:59:59.9999999-07:00", ExpectedResult = false )]
    [TestCase( "2023-01-01T00:00:00.0000000-07:00", ExpectedResult = false )]
    [TestCase( "2023-12-31T23:59:59.9999999-07:00", ExpectedResult = false )]
    [TestCase( "2024-01-01T00:00:00.0000000-07:00", ExpectedResult = true )]
    [TestCase( "2025-01-01T00:00:00.0000000-07:00", ExpectedResult = true )]
    public bool IsYearlySnapshotNeeded( DateTimeOffset timestamp )
    {
        return ZfsRecordTestHelpers.GetNewTestRootFileSystem( ).IsYearlySnapshotNeeded( timestamp );
    }

    [Test]
    [Parallelizable]
    public void Snapshots_CollectionInitializedProperly( )
    {
        ZfsRecord dataset = ZfsRecordTestHelpers.GetNewTestRootFileSystemFs1( );
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
        ZfsRecord dataset = ZfsRecordTestHelpers.GetNewTestRootFileSystemFs1( );

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
