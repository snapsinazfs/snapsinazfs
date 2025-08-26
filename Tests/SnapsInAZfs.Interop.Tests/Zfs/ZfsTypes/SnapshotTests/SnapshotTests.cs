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

using SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes.ZfsRecordTests;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes.SnapshotTests;

[TestFixture]
[Category ( "General" )]
[Parallelizable ( ParallelScope.Self )]
[TestOf ( typeof( Snapshot ) )]
public class SnapshotTests
{
    [Test]
    [Combinatorial]
    public void CompareTo_LeftEarlierThanRight( [ValueSource ( nameof (GetRelevantSnapshotPeriodKinds) )] SnapshotPeriodKind leftPeriod, [ValueSource ( nameof (GetRelevantSnapshotPeriodKinds) )] SnapshotPeriodKind rightPeriod )
    {
        DateTimeOffset leftTimestamp  = DateTimeOffset.Now;
        DateTimeOffset rightTimestamp = leftTimestamp.AddDays ( 1 );
        Snapshot       leftSnapshot   = SnapshotTestHelpers.GetStandardTestSnapshot ( leftPeriod,  leftTimestamp );
        Snapshot       rightSnapshot  = SnapshotTestHelpers.GetStandardTestSnapshot ( rightPeriod, rightTimestamp );

        Assert.That ( leftSnapshot, Is.LessThan ( rightSnapshot ) );
    }

    [Test]
    [Combinatorial]
    public void CompareTo_LeftLaterThanRight( [ValueSource ( nameof (GetRelevantSnapshotPeriodKinds) )] SnapshotPeriodKind leftPeriod, [ValueSource ( nameof (GetRelevantSnapshotPeriodKinds) )] SnapshotPeriodKind rightPeriod )
    {
        DateTimeOffset leftTimestamp  = DateTimeOffset.Now;
        DateTimeOffset rightTimestamp = leftTimestamp.AddDays ( -1 );
        Snapshot       leftSnapshot   = SnapshotTestHelpers.GetStandardTestSnapshot ( leftPeriod,  leftTimestamp );
        Snapshot       rightSnapshot  = SnapshotTestHelpers.GetStandardTestSnapshot ( rightPeriod, rightTimestamp );

        Assert.That ( leftSnapshot, Is.GreaterThan ( rightSnapshot ) );
    }

    [Test]
    [Combinatorial]
    public void CompareTo_SameTimestampAndPeriod( [Values ( "parent1", "parent2", "parent3" )] string leftParentName, [Values ( "parent2" )] string rightParentName )
    {
        DateTimeOffset     leftTimestamp  = DateTimeOffset.Now;
        DateTimeOffset     rightTimestamp = leftTimestamp;
        SnapshotPeriodKind leftPeriod     = SnapshotPeriodKind.Frequent;
        SnapshotPeriodKind rightPeriod    = SnapshotPeriodKind.Frequent;
        ZfsRecord          leftParent     = ZfsRecordTestHelpers.GetNewTestRootFileSystem ( leftParentName );
        ZfsRecord          rightParent    = ZfsRecordTestHelpers.GetNewTestRootFileSystem ( rightParentName );
        Snapshot           leftSnapshot   = SnapshotTestHelpers.GetStandardTestSnapshotForParent ( leftPeriod,  leftTimestamp,  leftParent );
        Snapshot           rightSnapshot  = SnapshotTestHelpers.GetStandardTestSnapshotForParent ( rightPeriod, rightTimestamp, rightParent );

        int nameComparisonResult     = string.CompareOrdinal ( leftSnapshot.Name, rightSnapshot.Name );
        int snapshotComparisonResult = leftSnapshot.CompareTo ( rightSnapshot );
        Assert.That ( snapshotComparisonResult, Is.EqualTo ( nameComparisonResult ) );
    }

    [Test]
    [Combinatorial]
    public void CompareTo_SameTimestamps( [ValueSource ( nameof (GetRelevantSnapshotPeriodKinds) )] SnapshotPeriodKind leftPeriod, [ValueSource ( nameof (GetRelevantSnapshotPeriodKinds) )] SnapshotPeriodKind rightPeriod )
    {
        DateTimeOffset leftTimestamp  = DateTimeOffset.Now;
        DateTimeOffset rightTimestamp = leftTimestamp;
        ZfsRecord      leftParent     = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        ZfsRecord      rightParent    = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        Snapshot       leftSnapshot   = SnapshotTestHelpers.GetStandardTestSnapshotForParent ( leftPeriod,  leftTimestamp,  leftParent );
        Snapshot       rightSnapshot  = SnapshotTestHelpers.GetStandardTestSnapshotForParent ( rightPeriod, rightTimestamp, rightParent );

        if ( leftPeriod < rightPeriod )
        {
            Assert.That ( leftSnapshot, Is.LessThan ( rightSnapshot ) );
        }
        else if ( leftPeriod == rightPeriod )
        {
            Assert.That ( leftSnapshot, Is.EqualTo ( rightSnapshot ) );
        }
        else if ( leftPeriod > rightPeriod )
        {
            Assert.That ( leftSnapshot, Is.GreaterThan ( rightSnapshot ) );
        }
    }

    [Test]
    [Combinatorial]
    public void GetSnapshotOptionsStringForZfsSnapshot_IsCorrect( [ValueSource ( nameof (GetRelevantSnapshotPeriodKinds) )] SnapshotPeriodKind period, [Values ( "2023-01-01T00:00:00.0000000", "2024-01-01T00:00:00.0000000" )] DateTimeOffset timestamp, [Values ( ZfsPropertyValueConstants.SnapsInAZfs, ZfsPropertyValueConstants.ZfsRecursion )] string recursion )
    {
        ZfsRecord parent = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        parent.UpdateProperty ( ZfsPropertyNames.RecursionPropertyName, recursion );
        Snapshot snapshot              = SnapshotTestHelpers.GetStandardTestSnapshotForParent ( period, timestamp, parent );
        string   periodString          = (SnapshotPeriod)period;
        string   sourceSystem          = ZfsPropertyValueConstants.StandaloneSiazSystem;
        string   testOptionsString     = $"-o {ZfsPropertyNames.SnapshotPeriodPropertyName}={periodString} -o {ZfsPropertyNames.SnapshotTimestampPropertyName}={timestamp:O} -o {ZfsPropertyNames.RecursionPropertyName}={recursion} -o {ZfsPropertyNames.SourceSystem}={sourceSystem}";
        string   snapshotOptionsString = snapshot.GetSnapshotOptionsStringForZfsSnapshot( );
        Assert.That ( snapshotOptionsString, Is.EqualTo ( testOptionsString ) );
    }

    [Test]
    public void ToString_ReturnsName ( )
    {
        Snapshot snapshot = SnapshotTestHelpers.GetStandardTestSnapshot ( SnapshotPeriod.Frequent, DateTimeOffset.Now );
        Assert.That ( snapshot.ToString( ), Is.EqualTo ( snapshot.Name ) );
    }

    [Test]
    [Combinatorial]
    public void UpdateProperty_SnapshotPeriod( [ValueSource ( nameof (GetRelevantSnapshotPeriodKinds) )] SnapshotPeriodKind originalPeriod, [ValueSource ( nameof (GetRelevantSnapshotPeriodKinds) )] SnapshotPeriodKind newPeriod )
    {
        Snapshot            snapshot = SnapshotTestHelpers.GetStandardTestSnapshot ( originalPeriod, DateTimeOffset.Now );
        ZfsProperty<string> original = snapshot.Period with { };

        Assume.That ( snapshot.Period, Is.EqualTo ( original ) );

        Assert.Multiple ( ( ) =>
                          {
                              Assert.That ( ( ) => snapshot.UpdateProperty ( ZfsPropertyNames.SnapshotPeriodPropertyName, (SnapshotPeriod)newPeriod ), Throws.TypeOf<ArgumentOutOfRangeException>( ) );
                              Assert.That ( snapshot.Period,                                                                                           Is.EqualTo ( original ) );
                          }
                        );
    }

    [Test]
    public void UpdateProperty_SnapshotTimestamp ( )
    {
        DateTimeOffset              originalTimestamp = DateTimeOffset.Now;
        DateTimeOffset              newTimestamp      = originalTimestamp.AddDays ( 1 );
        Snapshot                    snapshot          = SnapshotTestHelpers.GetStandardTestSnapshot ( SnapshotPeriod.Frequent, originalTimestamp );
        ZfsProperty<DateTimeOffset> original          = snapshot.Timestamp with { };

        Assume.That ( snapshot.Timestamp, Is.EqualTo ( original ) );

        Assert.Multiple ( ( ) =>
                          {
                              Assert.That ( ( ) => snapshot.UpdateProperty ( ZfsPropertyNames.SnapshotTimestampPropertyName, in newTimestamp ), Throws.TypeOf<ArgumentOutOfRangeException>( ) );
                              Assert.That ( snapshot.Timestamp,                                                                                 Is.EqualTo ( original ) );
                          }
                        );
    }

    private static SnapshotPeriodKind[] GetRelevantSnapshotPeriodKinds ( )
    {
        return [ SnapshotPeriodKind.Frequent, SnapshotPeriodKind.Hourly, SnapshotPeriodKind.Daily, SnapshotPeriodKind.Weekly, SnapshotPeriodKind.Monthly, SnapshotPeriodKind.Yearly ];
    }
}
