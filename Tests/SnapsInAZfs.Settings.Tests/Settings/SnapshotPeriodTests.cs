#region MIT LICENSE

// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/

#endregion

using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Settings.Tests.Settings;

[TestFixture]
[Category( "General" )]
[TestOf( typeof( SnapshotPeriod ) )]
public class SnapshotPeriodTests
{
    private static readonly SnapshotPeriod[] AllSnapshotPeriods = [SnapshotPeriod.NotSet, SnapshotPeriod.Frequent, SnapshotPeriod.Hourly, SnapshotPeriod.Daily, SnapshotPeriod.Weekly, SnapshotPeriod.Monthly, SnapshotPeriod.Yearly];
    private static readonly string[] AllSnapshotPeriodStrings = [SnapshotPeriod.NotSetString, SnapshotPeriod.FrequentString, SnapshotPeriod.HourlyString, SnapshotPeriod.DailyString, SnapshotPeriod.WeeklyString, SnapshotPeriod.MonthlyString, SnapshotPeriod.YearlyString];

    [Test]
    public void Compare_ItemsEqual_IfBothNull( )
    {
        Assert.That( SnapshotPeriod.Compare( null, null ), Is.Zero );
    }

    [Test]
    public void Compare_ProperlyOrdersValues( [ValueSource( nameof( AllSnapshotPeriods ) )] SnapshotPeriod item1, [ValueSource( nameof( AllSnapshotPeriods ) )] SnapshotPeriod item2 )
    {
        int testCompareResult = SnapshotPeriod.Compare( item1, item2 );
        int kindCompareResult = item1.Kind.CompareTo( item2.Kind );
        int testSign = int.Sign( testCompareResult );
        int kindSign = int.Sign( kindCompareResult );
        Assert.That( testSign, Is.EqualTo( kindSign ) );
    }

    [Test]
    public void Compare_RightItemGreater_IfOnlyLeftItemIsNull( [ValueSource( nameof( AllSnapshotPeriods ) )] SnapshotPeriod rightPeriod )
    {
        Assert.That( SnapshotPeriod.Compare( null, rightPeriod ), Is.Negative );
    }

    [Test]
    public void CompareTo_Null_IsLessThanNotNull( [ValueSource( nameof( AllSnapshotPeriods ) )] SnapshotPeriod left )
    {
        Assert.That( left.CompareTo( null ), Is.Positive );
    }

    [Test]
    public void CompareTo_SnapshotPeriod_DelegatesToSnapshotPeriodKind( [ValueSource( nameof( AllSnapshotPeriods ) )] SnapshotPeriod left, [ValueSource( nameof( AllSnapshotPeriods ) )] SnapshotPeriod right )
    {
        SnapshotPeriodKind leftKind = left.Kind;
        SnapshotPeriodKind rightKind = right.Kind;

        Assert.That( int.Sign( left.CompareTo( right ) ), Is.EqualTo( int.Sign( leftKind.CompareTo( rightKind ) ) ) );
    }

    [Test]
    public void CompareTo_SnapshotPeriodKind_DelegatesToSnapshotPeriodKind( [ValueSource( nameof( AllSnapshotPeriods ) )] SnapshotPeriod left, [Values] SnapshotPeriodKind right )
    {
        SnapshotPeriodKind leftKind = left.Kind;

        Assert.That( int.Sign( left.CompareTo( right ) ), Is.EqualTo( int.Sign( leftKind.CompareTo( right ) ) ) );
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
    [Sequential]
    public void StringToSnapshotPeriodKind_ReturnsExpectedValues( [ValueSource( nameof( AllSnapshotPeriodStrings ) )] string periodString, [ValueSource( nameof( AllSnapshotPeriods ) )] SnapshotPeriod period )
    {
        Assert.That( ( ) => SnapshotPeriod.StringToSnapshotPeriodKind( periodString ), Is.EqualTo( period.Kind ) );
    }

    [Test]
    public void StringToSnapshotPeriodKind_ThrowsFormatException_OnBadPeriod( )
    {
        Assert.That( static ( ) => SnapshotPeriod.StringToSnapshotPeriodKind( "BogusValue" ), Throws.TypeOf<FormatException>( ) );
    }

    [Test]
    public void ToString_AsExpected( [ValueSource( nameof( AllSnapshotPeriods ) )] SnapshotPeriod period )
    {
#pragma warning disable CS8524
        string periodString = period.Kind switch
        {
            SnapshotPeriodKind.Frequent => SnapshotPeriod.FrequentString,
            SnapshotPeriodKind.Hourly => SnapshotPeriod.HourlyString,
            SnapshotPeriodKind.Daily => SnapshotPeriod.DailyString,
            SnapshotPeriodKind.Weekly => SnapshotPeriod.WeeklyString,
            SnapshotPeriodKind.Monthly => SnapshotPeriod.MonthlyString,
            SnapshotPeriodKind.Yearly => SnapshotPeriod.YearlyString,
            SnapshotPeriodKind.NotSet => SnapshotPeriod.NotSetString
        };
        Assert.That( period.ToString( ), Is.EqualTo( periodString ) );
#pragma warning restore CS8524
    }
}
