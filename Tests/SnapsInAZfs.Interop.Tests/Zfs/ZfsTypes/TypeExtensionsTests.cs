// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes;

[TestFixture]
[TestOf( typeof( TypeExtensions ) )]
[Category( "General" )]
public class TypeExtensionsTests
{
    [Test]
    [TestCaseSource( nameof( AsTrueFalseRadioButtonIndexTestCaseValues ) )]
    public int AsTrueFalseRadioButtonIndex_ReturnsCorrectValue( ZfsProperty<bool> testProperty )
    {
        return testProperty.AsTrueFalseRadioIndex( );
    }

    [Test]
    public void GetMostRecentSnapshotZfsPropertyName_ReturnsCorrectValue( [ValueSource( nameof( GetMostRecentSnapshotZfsPropertyNameValues ) )] SnapshotPeriod kind )
    {
        switch ( kind )
        {
            case SnapshotPeriod.NotSetString:
                Assert.That( kind.GetMostRecentSnapshotZfsPropertyName, Throws.InstanceOf<ArgumentOutOfRangeException>( ) );
                return;
            case SnapshotPeriod.FrequentString:
                Assert.That( kind.GetMostRecentSnapshotZfsPropertyName( ), Is.EqualTo( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName ) );
                return;
            case SnapshotPeriod.HourlyString:
                Assert.That( kind.GetMostRecentSnapshotZfsPropertyName( ), Is.EqualTo( ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName ) );
                return;
            case SnapshotPeriod.DailyString:
                Assert.That( kind.GetMostRecentSnapshotZfsPropertyName( ), Is.EqualTo( ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName ) );
                return;
            case SnapshotPeriod.WeeklyString:
                Assert.That( kind.GetMostRecentSnapshotZfsPropertyName( ), Is.EqualTo( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName ) );
                return;
            case SnapshotPeriod.MonthlyString:
                Assert.That( kind.GetMostRecentSnapshotZfsPropertyName( ), Is.EqualTo( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName ) );
                return;
            case SnapshotPeriod.YearlyString:
                Assert.That( kind.GetMostRecentSnapshotZfsPropertyName( ), Is.EqualTo( ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName ) );
                return;
            default:
                Assert.That( kind.GetMostRecentSnapshotZfsPropertyName, Throws.InstanceOf<ArgumentOutOfRangeException>( ) );
                return;
        }
    }

    [Test]
    [Sequential]
    public void GetZfsPathParent_ReturnsProperPath( [Values( "gen1a", "gen1b/gen2b", "gen1c/gen2c/gen3c" )] string original, [Values( "gen1a", "gen1b", "gen1c/gen2c" )] string parent )
    {
        Assert.That( original.GetZfsPathParent( ), Is.EqualTo( parent ) );
    }

    [Test]
    [TestCaseSource( nameof( IntPropertyIsWantedTestCaseValues ) )]
    public bool IsNotWanted_ReturnsCorrectValue( ZfsProperty<int> testProperty )
    {
        return !testProperty.IsNotWanted( );
    }

    [Test]
    [TestCaseSource( nameof( IntPropertyIsWantedTestCaseValues ) )]
    public bool IsWanted_ReturnsCorrectValue( ZfsProperty<int> testProperty )
    {
        return testProperty.IsWanted( );
    }

    private static TestCaseData[] AsTrueFalseRadioButtonIndexTestCaseValues( )
    {
        return
        [
            new TestCaseData( ZfsProperty<bool>.CreateWithoutParent( "trueProperty", true ) ) { ExpectedResult = 0, HasExpectedResult = true },
            new TestCaseData( ZfsProperty<bool>.CreateWithoutParent( "falseProperty", false ) ) { ExpectedResult = 1, HasExpectedResult = true }
        ];
    }

    private static SnapshotPeriod[] GetMostRecentSnapshotZfsPropertyNameValues( )
    {
        return [SnapshotPeriod.NotSet, SnapshotPeriod.Frequent, SnapshotPeriod.Hourly, SnapshotPeriod.Daily, SnapshotPeriod.Weekly, SnapshotPeriod.Monthly, SnapshotPeriod.Yearly];
    }

    private static TestCaseData[] IntPropertyIsWantedTestCaseValues( )
    {
        return
        [
            new TestCaseData( ZfsProperty<int>.CreateWithoutParent( "0Property", 0 ) ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( ZfsProperty<int>.CreateWithoutParent( "1Property", 1 ) ) { ExpectedResult = true, HasExpectedResult = true }
        ];
    }
}
