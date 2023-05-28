// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Sanoid.Common.Zfs;
using Sanoid.Interop.Zfs.Native.Enums;

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
        int[] expectedValues = { 1, 2, 4 };
        Assert.That( Enum.GetValuesAsUnderlyingType<ZfsListObjectTypes>( ), Is.EquivalentTo( expectedValues ) );
    }

    [Test( Description = "Tests all possible values of ZfsObjectKind and their expected string representations" )]
    [Category( "General" )]
    [Category( "TypeChecks" )]
    [TestCase( (ZfsListObjectTypes)1, "filesystem" )]
    [TestCase( (ZfsListObjectTypes)2, "snapshot" )]
    [TestCase( (ZfsListObjectTypes)3, "filesystem,snapshot" )]
    [TestCase( (ZfsListObjectTypes)4, "volume" )]
    [TestCase( (ZfsListObjectTypes)5, "filesystem,volume" )]
    [TestCase( (ZfsListObjectTypes)6, "snapshot,volume" )]
    [TestCase( (ZfsListObjectTypes)7, "filesystem,snapshot,volume" )]
    public void ListTypeEnumStringsAsExpectedForValue( ZfsListObjectTypes types, string expectedString )
    {
        Console.Write( "Checking ToStringForCommandLine({0}) returns {1}: ", (int)types, expectedString );
        string actualString = types.ToStringForCommandLine( );
        Console.Write( actualString == expectedString ? "yes" : "no" );
        Assert.That( actualString, Is.EqualTo( expectedString ) );
    }

    [Test]
    [Category( "General" )]
    [Category( "TypeChecks" )]
    [TestCase( (ZfsListObjectTypes)1, new[] { "filesystem" } )]
    [TestCase( (ZfsListObjectTypes)2, new[] { "snapshot" } )]
    [TestCase( (ZfsListObjectTypes)3, new[] { "filesystem", "snapshot" } )]
    [TestCase( (ZfsListObjectTypes)4, new[] { "volume" } )]
    [TestCase( (ZfsListObjectTypes)5, new[] { "filesystem", "volume" } )]
    [TestCase( (ZfsListObjectTypes)6, new[] { "snapshot", "volume" } )]
    [TestCase( (ZfsListObjectTypes)7, new[] { "filesystem", "snapshot", "volume" } )]
    public void ListTypeEnumStringArrayAsExpectedForValue( ZfsListObjectTypes types, string[] expectedArray )
    {
        string[] actualArray = types.ToStringArray( );
        Assert.Multiple( ( ) =>
        {
            Assert.That( actualArray, Is.Not.Null );
            Assert.That( actualArray, Is.Not.Empty );
            Assert.That( actualArray, Is.All.Not.Null );
            Assert.That( actualArray, Is.All.InstanceOf<string>( ) );
            Assert.That( actualArray, Is.All.Not.Empty );
            Assert.That( actualArray, Is.Unique );
            Assert.That( actualArray, Is.EquivalentTo( expectedArray ) );
        } );
    }

    [Test]
    [Category( "General" )]
    [Category( "TypeChecks" )]
    [TestCase( ZfsListObjectTypes.FileSystem, zfs_type_t.ZFS_TYPE_FILESYSTEM )]
    [TestCase( ZfsListObjectTypes.Snapshot, zfs_type_t.ZFS_TYPE_SNAPSHOT )]
    [TestCase( ZfsListObjectTypes.FileSystem | ZfsListObjectTypes.Snapshot, zfs_type_t.ZFS_TYPE_FILESYSTEM | zfs_type_t.ZFS_TYPE_SNAPSHOT )]
    [TestCase( ZfsListObjectTypes.Volume, zfs_type_t.ZFS_TYPE_VOLUME )]
    [TestCase( ZfsListObjectTypes.FileSystem | ZfsListObjectTypes.Volume, zfs_type_t.ZFS_TYPE_FILESYSTEM | zfs_type_t.ZFS_TYPE_VOLUME )]
    [TestCase( ZfsListObjectTypes.Snapshot | ZfsListObjectTypes.Volume, zfs_type_t.ZFS_TYPE_SNAPSHOT | zfs_type_t.ZFS_TYPE_VOLUME )]
    [TestCase( ZfsListObjectTypes.FileSystem | ZfsListObjectTypes.Snapshot | ZfsListObjectTypes.Volume, zfs_type_t.ZFS_TYPE_FILESYSTEM | zfs_type_t.ZFS_TYPE_SNAPSHOT | zfs_type_t.ZFS_TYPE_VOLUME )]
    public void ListTypeEnumValuesEquivalentToZfsNativeValues( ZfsListObjectTypes sanoidType, zfs_type_t zfsNativeType )
    {
        Assert.That( (int)sanoidType, Is.EqualTo( (int)zfsNativeType ) );
    }
}
