// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsCommandRunner;

[TestFixture]
[Category( "General" )]
public class RawZfsObjectTests
{
    [Test]
    public void Constructor_ShortVersionCreatesNewCollection( )
    {
        RawZfsObject obj = new( "testObject", ZfsPropertyValueConstants.FileSystem );
        Assert.Multiple( ( ) =>
        {
            Assert.That( obj, Is.Not.Null );
            Assert.That( obj.Properties, Is.Not.Null );
            Assert.That( obj.Properties, Is.Empty );
        } );
    }
}
