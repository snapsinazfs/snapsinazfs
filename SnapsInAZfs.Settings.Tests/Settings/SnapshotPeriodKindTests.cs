// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Settings.Tests.Settings;

[TestFixture]
[Category( "General" )]
[Category( "TypeStructure" )]
[Description("These tests are a layer of protection against potentially breaking changes to the definition of the SnapshotPeriodKind enum itself")]
[TestOf( typeof( SnapshotPeriodKind ) )]
public class SnapshotPeriodKindTests
{
    [Test]
    [TestCase( SnapshotPeriodKind.NotSet, 0 )]
    [TestCase( SnapshotPeriodKind.Frequent, 1 )]
    [TestCase( SnapshotPeriodKind.Hourly, 2 )]
    [TestCase( SnapshotPeriodKind.Daily, 3 )]
    [TestCase( SnapshotPeriodKind.Weekly, 4 )]
    [TestCase( SnapshotPeriodKind.Monthly, 5 )]
    [TestCase( SnapshotPeriodKind.Yearly, 6 )]
    [Description( "Guarding against unintentional changes to the int values of the enum" )]
    public void EnumValues_AsExpected( SnapshotPeriodKind enumValue, int intValue )
    {
        Assert.That( (int)enumValue, Is.EqualTo( intValue ) );
    }

    [Test]
    [Description( "Guarding against additions to the enum" )]
    public void EnumEntries_AsExpected( )
    {
        string[] names = Enum.GetNames<SnapshotPeriodKind>( );
        Assert.That( names, Has.Length.EqualTo( 7 ) );
    }
}
