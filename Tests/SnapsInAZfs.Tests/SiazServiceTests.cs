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

namespace SnapsInAZfs.Tests;

[TestFixture]
[TestOf( typeof( SiazService ) )]
[Parallelizable]
public class SiazServiceTests
{
    [Test]
    [TestCaseSource( nameof( GetGreatestCommonFrequentIntervalFactor_TestCases ) )]
    public void GetGreatestCommonFrequentIntervalFactor_ReturnsExpectedValue( Dictionary<string, TemplateSettings> templates )
    {
        int result = SiazService.GetGreatestCommonFrequentIntervalFactor( templates );
        int[] allPeriods = templates.Select( t => t.Value.SnapshotTiming.FrequentPeriod ).ToArray( );
        Assume.That( result, Is.EqualTo( allPeriods.GreatestCommonFactor( ) ) );
        Assert.Multiple( ( ) =>
        {
            Assert.That( allPeriods.Select( p => p % result ), Has.All.Zero );
            Assert.That( result, Is.EqualTo( allPeriods.GreatestCommonFactor( ) ) );
        } );
    }

    [Test]
    [TestCaseSource( nameof( GetNewTimerInterval_NewValuesWithinTolerance_TestCases ) )]
    public void GetNewTimerInterval_NewValuesWithinTolerance( DateTimeOffset timestamp, TimeSpan configuredTimerInterval, DateTimeOffset expectedNextTickTimestamp, TimeSpan expectedTimerInterval )
    {
        SiazService.GetNewTimerInterval( in timestamp, in configuredTimerInterval, out TimeSpan calculatedTimerInterval, out DateTimeOffset calculatedNextTickTimestamp );
        Assert.Multiple( ( ) =>
        {
            Assert.That( calculatedTimerInterval, Is.EqualTo( expectedTimerInterval ).Within( 250 ).Milliseconds );
            Assert.That( calculatedNextTickTimestamp, Is.EqualTo( expectedNextTickTimestamp ).Within( 250 ).Milliseconds );
        } );
    }

    private static IEnumerable<TestCaseData> GetGreatestCommonFrequentIntervalFactor_TestCases( )
    {
        yield return new( new Dictionary<string, TemplateSettings>
        {
            {
                "template1", new TemplateSettings
                {
                    SnapshotTiming = SnapshotTimingSettings.GetDefault( )
                }
            }
        } );
        yield return new( new Dictionary<string, TemplateSettings>
        {
            {
                "template1", new TemplateSettings
                {
                    SnapshotTiming = SnapshotTimingSettings.GetDefault( )
                }
            },
            {
                "template2", new TemplateSettings
                {
                    SnapshotTiming = SnapshotTimingSettings.GetDefault( ) with { FrequentPeriod = 7 }
                }
            }
        } );
        yield return new( new Dictionary<string, TemplateSettings>
        {
            {
                "template1", new TemplateSettings
                {
                    SnapshotTiming = SnapshotTimingSettings.GetDefault( )
                }
            },
            {
                "template2", new TemplateSettings
                {
                    SnapshotTiming = SnapshotTimingSettings.GetDefault( ) with { FrequentPeriod = 7 }
                }
            },
            {
                "template3", new TemplateSettings
                {
                    SnapshotTiming = SnapshotTimingSettings.GetDefault( ) with { FrequentPeriod = 3 }
                }
            }
        } );
    }

    private static IEnumerable<TestCaseData> GetNewTimerInterval_NewValuesWithinTolerance_TestCases( )
    {
        yield return new( new DateTimeOffset( 2023, 1, 1, 0, 0, 0, 0, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 10 ), new DateTimeOffset( 2023, 1, 1, 0, 0, 10, 0, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 10 ) );
        yield return new( new DateTimeOffset( 2023, 1, 1, 0, 0, 0, 25, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 10 ), new DateTimeOffset( 2023, 1, 1, 0, 0, 10, 0, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 9.975d ) );
        yield return new( new DateTimeOffset( 2023, 1, 1, 0, 0, 0, 250, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 10 ), new DateTimeOffset( 2023, 1, 1, 0, 0, 10, 0, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 9.75d ) );
        yield return new( new DateTimeOffset( 2023, 1, 1, 0, 0, 0, 500, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 10 ), new DateTimeOffset( 2023, 1, 1, 0, 0, 10, 0, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 9.5d ) );
        yield return new( new DateTimeOffset( 2023, 1, 1, 0, 0, 0, 500, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 10 ), new DateTimeOffset( 2023, 1, 1, 0, 0, 10, 0, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 9.25d ) );
        yield return new( new DateTimeOffset( 2023, 1, 1, 0, 0, 1, 0, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 10 ), new DateTimeOffset( 2023, 1, 1, 0, 0, 10, 0, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 9 ) );
        yield return new( new DateTimeOffset( 2023, 1, 1, 0, 0, 1, 250, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 10 ), new DateTimeOffset( 2023, 1, 1, 0, 0, 10, 0, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 8.75d ) );
        yield return new( new DateTimeOffset( 2023, 1, 1, 0, 0, 10, 0, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 10 ), new DateTimeOffset( 2023, 1, 1, 0, 0, 20, 0, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 10 ) );
        yield return new( new DateTimeOffset( 2023, 1, 1, 0, 0, 15, 750, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 10 ), new DateTimeOffset( 2023, 1, 1, 0, 0, 20, 0, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 4.25d ) );
    }
}
