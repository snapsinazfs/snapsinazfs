// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Sanoid.Common.Zfs;

namespace Sanoid.Common.Tests.Zfs;

[TestFixture]
public class ZfsCommandRunnerTests
{
    [Test]
    [Category( "General" )]
    [Category( "TypeChecks" )]
    public void ZfsListObjectTypesHasFlagsAttribute( )
    {
        Assert.That( typeof( ZfsListObjectTypes ), Has.Attribute<FlagsAttribute>( ) );
    }

    [Test]
    [Category( "General" )]
    [Category( "TypeChecks" )]
    [Sequential]
    public void ZfsListObjectTypesOnlyExpectedValuesDefined( )
    {
        int[] expectedValues = new[] { 1, 2, 4 };
        Assert.That( Enum.GetValuesAsUnderlyingType<ZfsListObjectTypes>( ), Is.EquivalentTo( expectedValues ) );
    }

    [Test( Description = "Tests all possible values of ZfsListObjectTypes and their expected string representations" )]
    [Category( "General" )]
    [TestCase( (ZfsListObjectTypes)1, "filesystem" )]
    [TestCase( (ZfsListObjectTypes)2, "snapshot" )]
    [TestCase( (ZfsListObjectTypes)3, "filesystem,snapshot" )]
    [TestCase( (ZfsListObjectTypes)4, "volume" )]
    [TestCase( (ZfsListObjectTypes)5, "filesystem,volume" )]
    [TestCase( (ZfsListObjectTypes)6, "snapshot,volume" )]
    [TestCase( (ZfsListObjectTypes)7, "filesystem,snapshot,volume" )]
    public void ListTypeEnumStringsAsExpected( ZfsListObjectTypes types, string expectedString )
    {
        Console.Write( "Checking ToStringForCommandLine({0}) returns {1}: ", (int)types, expectedString );
        string actualString = types.ToStringForCommandLine( );
        Console.Write( actualString == expectedString ? "yes" : "no" );
        Assert.That( actualString, Is.EqualTo( expectedString ) );
    }
}
