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

using SnapsInAZfs.Interop.Zfs.ZfsTypes;

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes;

[TestFixture]
[TestOf ( typeof( TypeExtensions ) )]
[Category ( "General" )]
public class TypeExtensionsTests
{
    [Test]
    [TestCaseSource ( nameof (AsTrueFalseRadioButtonIndexTestCaseValues) )]
    public int AsTrueFalseRadioButtonIndex_ReturnsCorrectValue( ZfsProperty<bool> testProperty )
    {
        return testProperty.AsTrueFalseRadioIndex( );
    }

    [Test]
    public void GetMostRecentSnapshotZfsPropertyName_ReturnsCorrectValue( [ValueSource ( nameof (GetMostRecentSnapshotZfsPropertyNameValues) )] SnapshotPeriod kind )
    {
        switch ( kind )
        {
            case SnapshotPeriod.NotSetString:
                Assert.That ( kind.GetMostRecentSnapshotZfsPropertyName, Throws.InstanceOf<ArgumentOutOfRangeException>( ) );

                return;
            case SnapshotPeriod.FrequentString:
                Assert.That ( kind.GetMostRecentSnapshotZfsPropertyName( ), Is.EqualTo ( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName ) );

                return;
            case SnapshotPeriod.HourlyString:
                Assert.That ( kind.GetMostRecentSnapshotZfsPropertyName( ), Is.EqualTo ( ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName ) );

                return;
            case SnapshotPeriod.DailyString:
                Assert.That ( kind.GetMostRecentSnapshotZfsPropertyName( ), Is.EqualTo ( ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName ) );

                return;
            case SnapshotPeriod.WeeklyString:
                Assert.That ( kind.GetMostRecentSnapshotZfsPropertyName( ), Is.EqualTo ( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName ) );

                return;
            case SnapshotPeriod.MonthlyString:
                Assert.That ( kind.GetMostRecentSnapshotZfsPropertyName( ), Is.EqualTo ( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName ) );

                return;
            case SnapshotPeriod.YearlyString:
                Assert.That ( kind.GetMostRecentSnapshotZfsPropertyName( ), Is.EqualTo ( ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName ) );

                return;
            default:
                Assert.That ( kind.GetMostRecentSnapshotZfsPropertyName, Throws.InstanceOf<ArgumentOutOfRangeException>( ) );

                return;
        }
    }

    [Test]
    [Sequential]
    public void GetZfsPathParent_ReturnsProperPath( [Values ( "gen1a", "gen1b/gen2b", "gen1c/gen2c/gen3c" )] string original, [Values ( "gen1a", "gen1b", "gen1c/gen2c" )] string parent )
    {
        Assert.That ( original.GetZfsPathParent( ), Is.EqualTo ( parent ) );
    }

    [Test]
    [TestCaseSource ( nameof (IntPropertyIsWantedTestCaseValues) )]
    public bool IsNotWanted_ReturnsCorrectValue( ZfsProperty<int> testProperty )
    {
        return !testProperty.IsNotWanted( );
    }

    [Test]
    [TestCaseSource ( nameof (IntPropertyIsWantedTestCaseValues) )]
    public bool IsWanted_ReturnsCorrectValue( ZfsProperty<int> testProperty )
    {
        return testProperty.IsWanted( );
    }

    private static TestCaseData[] AsTrueFalseRadioButtonIndexTestCaseValues ( )
    {
        return
        [
            new TestCaseData ( ZfsProperty<bool>.CreateWithoutParent ( "trueProperty",  true ) ) { ExpectedResult  = 0, HasExpectedResult = true },
            new TestCaseData ( ZfsProperty<bool>.CreateWithoutParent ( "falseProperty", false ) ) { ExpectedResult = 1, HasExpectedResult = true }
        ];
    }

    private static SnapshotPeriod[] GetMostRecentSnapshotZfsPropertyNameValues ( )
    {
        return [ SnapshotPeriod.NotSet, SnapshotPeriod.Frequent, SnapshotPeriod.Hourly, SnapshotPeriod.Daily, SnapshotPeriod.Weekly, SnapshotPeriod.Monthly, SnapshotPeriod.Yearly ];
    }

    private static TestCaseData[] IntPropertyIsWantedTestCaseValues ( )
    {
        return
        [
            new TestCaseData ( ZfsProperty<int>.CreateWithoutParent ( "0Property", 0 ) ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData ( ZfsProperty<int>.CreateWithoutParent ( "1Property", 1 ) ) { ExpectedResult = true, HasExpectedResult  = true }
        ];
    }
}
