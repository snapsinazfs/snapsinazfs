// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Settings.Tests.Settings;

[TestFixture]
[Category( "General" )]
[TestOf( typeof( SnapshotPeriod ) )]
public class SnapshotPeriodTests
{
    private static readonly SnapshotPeriod[] AllSnapshotPeriods = { SnapshotPeriod.NotSet, SnapshotPeriod.Frequent, SnapshotPeriod.Hourly, SnapshotPeriod.Daily, SnapshotPeriod.Weekly, SnapshotPeriod.Monthly, SnapshotPeriod.Yearly };
    private static readonly string[] AllSnapshotPeriodStrings = { SnapshotPeriod.NotSetString, SnapshotPeriod.FrequentString, SnapshotPeriod.HourlyString, SnapshotPeriod.DailyString, SnapshotPeriod.WeeklyString, SnapshotPeriod.MonthlyString, SnapshotPeriod.YearlyString };

    [Test]
    public void CompareTo_Null_IsLessThanNotNull( [ValueSource( nameof( AllSnapshotPeriods ) )] SnapshotPeriod left )
    {
        Assert.That( left.CompareTo( null ), Is.EqualTo( -1 ) );
    }

    [Test]
    public void CompareTo_SnapshotPeriod_DelegatesToSnapshotPeriodKind( [ValueSource( nameof( AllSnapshotPeriods ) )] SnapshotPeriod left, [ValueSource( nameof( AllSnapshotPeriods ) )] SnapshotPeriod right )
    {
        SnapshotPeriodKind leftKind = left.Kind;
        SnapshotPeriodKind rightKind = right.Kind;

        Assert.That( left.CompareTo( right ), Is.EqualTo( leftKind.CompareTo( rightKind ) ) );
    }

    [Test]
    public void CompareTo_SnapshotPeriodKind_DelegatesToSnapshotPeriodKind( [ValueSource( nameof( AllSnapshotPeriods ) )] SnapshotPeriod left, [Values] SnapshotPeriodKind right )
    {
        SnapshotPeriodKind leftKind = left.Kind;

        Assert.That( left.CompareTo( right ), Is.EqualTo( leftKind.CompareTo( right ) ) );
    }

    [Test]
    public void Equals_BoxedReferenceEqualitySameAsReferenceEquals( [ValueSource( nameof( AllSnapshotPeriods ) )] SnapshotPeriod left, [ValueSource( nameof( AllSnapshotPeriods ) )] SnapshotPeriod right )
    {
        //Ignore this suggestion because we need to force which method is being called
        Assert.That( left.Equals( (object?)right ), Is.EqualTo( ReferenceEquals( left, right ) ) );
    }

    [Test]
    public void Equals_OtherObjectNullReturnsFalse( [ValueSource( nameof( AllSnapshotPeriods ) )] SnapshotPeriod left )
    {
#pragma warning disable NUnit2010
        //Ignore this suggestion because we need to force which method is being called
        Assert.That( left.Equals( (object?)null ), Is.False );
#pragma warning restore NUnit2010
    }

    [Test]
    public void Equals_SameTypesDelegateToSnapshotPeriodKind( [ValueSource( nameof( AllSnapshotPeriods ) )] SnapshotPeriod left, [ValueSource( nameof( AllSnapshotPeriods ) )] SnapshotPeriod right )
    {
        Assert.That( left.Equals( right ), Is.EqualTo( left.Kind == right.Kind ) );
    }

    [Test]
    [Sequential]
    public void ExplicitCastFromString_ReturnsExpectedSnapshotPeriod( [ValueSource( nameof( AllSnapshotPeriodStrings ) )] string periodString, [ValueSource( nameof( AllSnapshotPeriods ) )] SnapshotPeriod period )
    {
        Assert.That( (SnapshotPeriod)periodString, Is.EqualTo( period ) );
    }

    [Test]
    public void ExplicitCastFromString_ThrowsFormatExceptionOnInvalidValues( [Values( "", " ", "1", "a", "!" )] string periodString )
    {
        Assert.That( ( ) => (SnapshotPeriod)periodString, Throws.TypeOf<FormatException>( ) );
    }

    [Test]
    [Sequential]
    public void ExplicitCastToSnapshotPeriodKind_ReturnsExpectedSnapshotPeriodKind( [ValueSource( nameof( AllSnapshotPeriods ) )] SnapshotPeriod period, [Values] SnapshotPeriodKind periodKind )
    {
        Assert.That( (SnapshotPeriodKind)period, Is.EqualTo( periodKind ) );
    }

    [Test]
    public void GetHashCode_DelegatesToIntFromKind( [ValueSource( nameof( AllSnapshotPeriods ) )] SnapshotPeriod period )
    {
        Assert.That( period.GetHashCode( ), Is.EqualTo( ( (int)period.Kind ).GetHashCode( ) ) );
    }

    [Test]
    [Sequential]
    public void ImplicitCastFromSnapshotPeriodKind_ReturnsExpectedSnapshotPeriod( [ValueSource( nameof( AllSnapshotPeriods ) )] SnapshotPeriod period, [Values] SnapshotPeriodKind periodKind )
    {
        SnapshotPeriod periodFromImplicitCast = periodKind;
        Assert.That( periodFromImplicitCast, Is.EqualTo( period ) );
    }

    [Test]
    public void ToString_AsExpected( [ValueSource( nameof( AllSnapshotPeriods ) )] SnapshotPeriod period )
    {
        string periodString = period.Kind switch
        {
            SnapshotPeriodKind.Frequent => SnapshotPeriod.FrequentString,
            SnapshotPeriodKind.Hourly => SnapshotPeriod.HourlyString,
            SnapshotPeriodKind.Daily => SnapshotPeriod.DailyString,
            SnapshotPeriodKind.Weekly => SnapshotPeriod.WeeklyString,
            SnapshotPeriodKind.Monthly => SnapshotPeriod.MonthlyString,
            SnapshotPeriodKind.Yearly => SnapshotPeriod.YearlyString,
            SnapshotPeriodKind.NotSet => SnapshotPeriod.NotSetString,
            _ => SnapshotPeriod.NotSetString
        };
        Assert.That( period.ToString( ), Is.EqualTo( periodString ) );
    }
}
