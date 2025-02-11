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
[TestOf( typeof( FormattingSettings ) )]
[Parallelizable( ParallelScope.All )]
public class FormattingSettingsTests
{
    [Test]
    [Combinatorial]
    public void GenerateShortSnapshotName_ReturnsExpectedValue( [Values( SnapshotPeriodKind.Frequent, SnapshotPeriodKind.Hourly, SnapshotPeriodKind.Daily, SnapshotPeriodKind.Weekly, SnapshotPeriodKind.Monthly, SnapshotPeriodKind.Yearly )] SnapshotPeriodKind period, [ValueSource( nameof( GetTimestampsForFormatTests ) )] DateTimeOffset timestamp )
    {
        FormattingSettings testFormattingSettings = FormattingSettings.GetDefault( );
        string shortName = testFormattingSettings.GenerateShortSnapshotName( in period, in timestamp );
        Assert.That( shortName, Is.Not.Null );
        Assert.That( shortName, Is.Not.Empty );
#pragma warning disable CS8509 // We don't care about the NotSet value for this test
        Assert.That( shortName, Is.EqualTo( $"{testFormattingSettings.Prefix}{testFormattingSettings.ComponentSeparator}{timestamp.ToString( testFormattingSettings.TimestampFormatString )}{testFormattingSettings.ComponentSeparator}{period switch
        {
            SnapshotPeriodKind.Frequent => testFormattingSettings.FrequentSuffix,
            SnapshotPeriodKind.Hourly => testFormattingSettings.HourlySuffix,
            SnapshotPeriodKind.Daily => testFormattingSettings.DailySuffix,
            SnapshotPeriodKind.Weekly => testFormattingSettings.WeeklySuffix,
            SnapshotPeriodKind.Monthly => testFormattingSettings.MonthlySuffix,
            SnapshotPeriodKind.Yearly => testFormattingSettings.YearlySuffix
        }}" ) );
#pragma warning restore CS8509 // We don't care about the NotSet value for this test
    }

    [Test]
    public void GenerateShortSnapshotName_ThrowsOnBadTimestampFormatString( )
    {
        FormattingSettings testFormattingSettings = FormattingSettings.GetDefault( ) with { TimestampFormatString = "a" };
        Assert.That( ( ) => testFormattingSettings.GenerateShortSnapshotName( SnapshotPeriodKind.Frequent, DateTimeOffset.UnixEpoch ), Throws.TypeOf<FormatException>( ) );
    }

    [Test]
    [Combinatorial]
    public void GenerateShortSnapshotName_ThrowsOnInvalidPeriod( )
    {
        FormattingSettings testFormattingSettings = FormattingSettings.GetDefault( );
        Assert.That( ( ) => testFormattingSettings.GenerateShortSnapshotName( SnapshotPeriodKind.NotSet, DateTimeOffset.UnixEpoch ), Throws.TypeOf<ArgumentOutOfRangeException>( ) );
    }

    private static IEnumerable<DateTimeOffset> GetTimestampsForFormatTests( )
    {
        yield return DateTimeOffset.UnixEpoch;
        yield return new( 2023, 8, 1, 1, 0, 0, TimeZoneInfo.Local.BaseUtcOffset );
    }
}
