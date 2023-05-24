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
    [Test( Description = "Tests all possible values of ZfsListObjectTypes and their expected string representations" )]
    [Category( "General" )]
    [TestCase( (ZfsListObjectTypes)1, ExpectedResult = "filesystem" )]
    [TestCase( (ZfsListObjectTypes)2, ExpectedResult = "snapshot" )]
    [TestCase( (ZfsListObjectTypes)3, ExpectedResult = "filesystem,snapshot" )]
    [TestCase( (ZfsListObjectTypes)4, ExpectedResult = "volume" )]
    [TestCase( (ZfsListObjectTypes)5, ExpectedResult = "filesystem,volume" )]
    [TestCase( (ZfsListObjectTypes)6, ExpectedResult = "snapshot,volume" )]
    [TestCase( (ZfsListObjectTypes)7, ExpectedResult = "filesystem,snapshot,volume" )]
    public string ListTypeEnumStringsAsExpected( ZfsListObjectTypes types )
    {
        return types.ToStringForCommandLine( );
    }
}
