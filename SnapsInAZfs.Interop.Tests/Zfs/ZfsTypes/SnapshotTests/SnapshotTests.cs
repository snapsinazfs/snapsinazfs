// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license
using SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes.ZfsRecordTests;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes.SnapshotTests;

[TestFixture]
[Category( "General" )]
[Parallelizable( ParallelScope.Self )]
[TestOf( typeof( Snapshot ) )]
public class SnapshotTests
{
    [Test]
    [Combinatorial]
    public void CompareTo_LeftEarlierThanRight( [Values] SnapshotPeriodKind leftPeriod, [Values] SnapshotPeriodKind rightPeriod )
    {
        Assume.That( leftPeriod, Is.Not.EqualTo( SnapshotPeriodKind.NotSet ), "Skipping NotSet period for left snapshot" );
        Assume.That( rightPeriod, Is.Not.EqualTo( SnapshotPeriodKind.NotSet ), "Skipping NotSet period for right snapshot" );
        DateTimeOffset leftTimestamp = DateTimeOffset.Now;
        DateTimeOffset rightTimestamp = leftTimestamp.AddDays( 1 );
        Snapshot leftSnapshot = SnapshotTestHelpers.GetStandardTestSnapshot( leftPeriod, leftTimestamp );
        Snapshot rightSnapshot = SnapshotTestHelpers.GetStandardTestSnapshot( rightPeriod, rightTimestamp );

        Assert.That( leftSnapshot, Is.LessThan( rightSnapshot ) );
    }

    [Test]
    [Combinatorial]
    public void CompareTo_LeftLaterThanRight( [Values] SnapshotPeriodKind leftPeriod, [Values] SnapshotPeriodKind rightPeriod )
    {
        Assume.That( leftPeriod, Is.Not.EqualTo( SnapshotPeriodKind.NotSet ), "Skipping NotSet period for left snapshot" );
        Assume.That( rightPeriod, Is.Not.EqualTo( SnapshotPeriodKind.NotSet ), "Skipping NotSet period for right snapshot" );
        DateTimeOffset leftTimestamp = DateTimeOffset.Now;
        DateTimeOffset rightTimestamp = leftTimestamp.AddDays( -1 );
        Snapshot leftSnapshot = SnapshotTestHelpers.GetStandardTestSnapshot( leftPeriod, leftTimestamp );
        Snapshot rightSnapshot = SnapshotTestHelpers.GetStandardTestSnapshot( rightPeriod, rightTimestamp );

        Assert.That( leftSnapshot, Is.GreaterThan( rightSnapshot ) );
    }

    [Test]
    [Combinatorial]
    public void CompareTo_SameTimestampAndPeriod( [Values( "parent1", "parent2", "parent3" )] string leftParentName, [Values( "parent2" )] string rightParentName )
    {
        DateTimeOffset leftTimestamp = DateTimeOffset.Now;
        DateTimeOffset rightTimestamp = leftTimestamp;
        SnapshotPeriodKind leftPeriod = SnapshotPeriodKind.Frequent;
        SnapshotPeriodKind rightPeriod = SnapshotPeriodKind.Frequent;
        ZfsRecord leftParent = ZfsRecordTestHelpers.GetNewTestRootFileSystem( leftParentName );
        ZfsRecord rightParent = ZfsRecordTestHelpers.GetNewTestRootFileSystem( rightParentName );
        Snapshot leftSnapshot = SnapshotTestHelpers.GetStandardTestSnapshotForParent( leftPeriod, leftTimestamp, leftParent );
        Snapshot rightSnapshot = SnapshotTestHelpers.GetStandardTestSnapshotForParent( rightPeriod, rightTimestamp, rightParent );

        int nameComparisonResult = string.CompareOrdinal( leftSnapshot.SnapshotName.Value, rightSnapshot.SnapshotName.Value );
        int snapshotComparisonResult = leftSnapshot.CompareTo( rightSnapshot );
        Assert.That( snapshotComparisonResult, Is.EqualTo( nameComparisonResult ) );
    }

    [Test]
    [Combinatorial]
    public void CompareTo_SameTimestamps( [Values] SnapshotPeriodKind leftPeriod, [Values] SnapshotPeriodKind rightPeriod )
    {
        Assume.That( leftPeriod, Is.Not.EqualTo( SnapshotPeriodKind.NotSet ), "Skipping NotSet period for left snapshot" );
        Assume.That( rightPeriod, Is.Not.EqualTo( SnapshotPeriodKind.NotSet ), "Skipping NotSet period for right snapshot" );
        DateTimeOffset leftTimestamp = DateTimeOffset.Now;
        DateTimeOffset rightTimestamp = leftTimestamp;
        ZfsRecord leftParent = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        ZfsRecord rightParent = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        Snapshot leftSnapshot = SnapshotTestHelpers.GetStandardTestSnapshotForParent( leftPeriod, leftTimestamp, leftParent );
        Snapshot rightSnapshot = SnapshotTestHelpers.GetStandardTestSnapshotForParent( rightPeriod, rightTimestamp, rightParent );

        if ( leftPeriod < rightPeriod )
        {
            Assert.That( leftSnapshot, Is.LessThan( rightSnapshot ) );
        }
        else if ( leftPeriod == rightPeriod )
        {
            Assert.That( leftSnapshot, Is.EqualTo( rightSnapshot ) );
        }
        else if ( leftPeriod > rightPeriod )
        {
            Assert.That( leftSnapshot, Is.GreaterThan( rightSnapshot ) );
        }
    }

    [Test]
    [Combinatorial]
    public void GetSnapshotOptionsStringForZfsSnapshot_IsCorrect( [Values] SnapshotPeriodKind period, [Values( "2023-01-01T00:00:00.0000000", "2024-01-01T00:00:00.0000000" )] DateTimeOffset timestamp, [Values( ZfsPropertyValueConstants.SnapsInAZfs, ZfsPropertyValueConstants.ZfsRecursion )] string recursion )
    {
        Assume.That( period, Is.Not.EqualTo( SnapshotPeriodKind.NotSet ), "Skippig NotSet period" );
        ZfsRecord parent = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        parent.UpdateProperty( ZfsPropertyNames.RecursionPropertyName, recursion );
        Snapshot snapshot = SnapshotTestHelpers.GetStandardTestSnapshotForParent( period, timestamp, parent );
        string periodString = (SnapshotPeriod)period;
        string testOptionsString = $"-o {ZfsPropertyNames.SnapshotNamePropertyName}=testRoot@autosnap_{timestamp:s}_{periodString} -o {ZfsPropertyNames.SnapshotPeriodPropertyName}={periodString} -o {ZfsPropertyNames.SnapshotTimestampPropertyName}={timestamp:O} -o {ZfsPropertyNames.RecursionPropertyName}={recursion}";
        string snapshotOptionsString = snapshot.GetSnapshotOptionsStringForZfsSnapshot( );
        Assert.That( snapshotOptionsString, Is.EqualTo( testOptionsString ) );
    }

    [Test]
    public void ToString_ReturnsSnapshotName( )
    {
        Snapshot snapshot = SnapshotTestHelpers.GetStandardTestSnapshot( SnapshotPeriod.Frequent, DateTimeOffset.Now );
        Assert.That( snapshot.ToString( ), Is.EqualTo( snapshot.SnapshotName.Value ) );
    }

    [Test]
    public void UpdateProperty_SnapshotName( )
    {
        Snapshot snapshot = SnapshotTestHelpers.GetStandardTestSnapshot( SnapshotPeriod.Frequent, DateTimeOffset.Now );
        ZfsProperty<string> original = snapshot.SnapshotName with { };

        Assume.That( original.Source, Is.EqualTo( ZfsPropertySourceConstants.Local ) );
        Assume.That( snapshot.SnapshotName, Is.EqualTo( original ) );

        Assert.Multiple( ( ) =>
        {
            Assert.That( ( ) => snapshot.UpdateProperty( ZfsPropertyNames.SnapshotNamePropertyName, "newName" ), Throws.TypeOf<ArgumentOutOfRangeException>( ) );
            Assert.That( snapshot.SnapshotName, Is.EqualTo( original ) );
        } );
    }

    [Test]
    [Combinatorial]
    public void UpdateProperty_SnapshotPeriod([Values(SnapshotPeriodKind.Frequent,SnapshotPeriodKind.Hourly, SnapshotPeriodKind.Daily,SnapshotPeriodKind.Weekly, SnapshotPeriodKind.Monthly, SnapshotPeriodKind.Yearly)]SnapshotPeriodKind originalPeriod, [Values(SnapshotPeriodKind.Frequent,SnapshotPeriodKind.Hourly, SnapshotPeriodKind.Daily,SnapshotPeriodKind.Weekly, SnapshotPeriodKind.Monthly, SnapshotPeriodKind.Yearly)]SnapshotPeriodKind newPeriod )
    {
        Snapshot snapshot = SnapshotTestHelpers.GetStandardTestSnapshot( originalPeriod, DateTimeOffset.Now );
        ZfsProperty<string> original = snapshot.Period with { };

        Assume.That( snapshot.Period, Is.EqualTo( original ) );

        Assert.Multiple( ( ) =>
        {
            Assert.That( ( ) => snapshot.UpdateProperty( ZfsPropertyNames.SnapshotPeriodPropertyName, (SnapshotPeriod)newPeriod ), Throws.TypeOf<ArgumentOutOfRangeException>( ) );
            Assert.That( snapshot.Period, Is.EqualTo( original ) );
        } );
    }

    [Test]
    public void UpdateProperty_SnapshotTimestamp( )
    {
        DateTimeOffset originalTimestamp = DateTimeOffset.Now;
        DateTimeOffset newTimestamp = originalTimestamp.AddDays(1);
        Snapshot snapshot = SnapshotTestHelpers.GetStandardTestSnapshot( SnapshotPeriod.Frequent, originalTimestamp );
        ZfsProperty<DateTimeOffset> original = snapshot.Timestamp with { };

        Assume.That( snapshot.Timestamp, Is.EqualTo( original ) );

        Assert.Multiple( ( ) =>
        {
            Assert.That( ( ) => snapshot.UpdateProperty( ZfsPropertyNames.SnapshotTimestampPropertyName, in newTimestamp ), Throws.TypeOf<ArgumentOutOfRangeException>( ) );
            Assert.That( snapshot.Timestamp, Is.EqualTo( original ) );
        } );
    }
}
