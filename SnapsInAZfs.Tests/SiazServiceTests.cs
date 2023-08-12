using System.Diagnostics;
using NUnit.Framework.Internal;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Tests;

[TestFixture]
[TestOf(typeof(SiazService))]
[Parallelizable]
public class SiazServiceTests
{
    [Test]
    [TestCaseSource(nameof(GetNewTimerInterval_NewValuesWithinTolerance_TestCases))]
    public void GetNewTimerInterval_NewValuesWithinTolerance( DateTimeOffset timestamp, TimeSpan configuredTimerInterval, DateTimeOffset expectedNextTickTimestamp, TimeSpan expectedTimerInterval )
    {
        SiazService.GetNewTimerInterval( in timestamp, in configuredTimerInterval, out TimeSpan calculatedTimerInterval, out DateTimeOffset calculatedNextTickTimestamp );
        Assert.Multiple( ( ) =>
        {
            Assert.That( calculatedTimerInterval, Is.EqualTo( expectedTimerInterval ).Within( 250 ).Milliseconds );
            Assert.That( calculatedNextTickTimestamp, Is.EqualTo( expectedNextTickTimestamp ).Within( 250 ).Milliseconds );
        } );
    }

    private static IEnumerable<TestCaseData> GetNewTimerInterval_NewValuesWithinTolerance_TestCases( )
    {
        yield return new ( new DateTimeOffset( 2023, 1, 1, 0, 0, 0, 0, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 10 ), new DateTimeOffset( 2023, 1, 1, 0, 0, 10, 0, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 10 ) );
        yield return new ( new DateTimeOffset( 2023, 1, 1, 0, 0, 0, 25, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 10 ), new DateTimeOffset( 2023, 1, 1, 0, 0, 10, 0, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 9.975d ) );
        yield return new ( new DateTimeOffset( 2023, 1, 1, 0, 0, 0, 250, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 10 ), new DateTimeOffset( 2023, 1, 1, 0, 0, 10, 0, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 9.75d ) );
        yield return new ( new DateTimeOffset( 2023, 1, 1, 0, 0, 0, 500, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 10 ), new DateTimeOffset( 2023, 1, 1, 0, 0, 10, 0, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 9.5d ) );
        yield return new ( new DateTimeOffset( 2023, 1, 1, 0, 0, 0, 500, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 10 ), new DateTimeOffset( 2023, 1, 1, 0, 0, 10, 0, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 9.25d ) );
        yield return new ( new DateTimeOffset( 2023, 1, 1, 0, 0, 1, 0, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 10 ), new DateTimeOffset( 2023, 1, 1, 0, 0, 10, 0, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 9 ) );
        yield return new ( new DateTimeOffset( 2023, 1, 1, 0, 0, 1, 250, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 10 ), new DateTimeOffset( 2023, 1, 1, 0, 0, 10, 0, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 8.75d ) );
        yield return new ( new DateTimeOffset( 2023, 1, 1, 0, 0, 10, 0, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 10 ), new DateTimeOffset( 2023, 1, 1, 0, 0, 20, 0, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 10 ) );
        yield return new ( new DateTimeOffset( 2023, 1, 1, 0, 0, 15, 750, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 10 ), new DateTimeOffset( 2023, 1, 1, 0, 0, 20, 0, 0, TimeSpan.Zero ), TimeSpan.FromSeconds( 4.25d ) );
    }
}
