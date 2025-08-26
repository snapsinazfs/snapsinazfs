#region MIT LICENSE
// Copyright 2025 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/
#endregion

namespace SnapsInAZfs.Settings.Tests.Settings;

[TestFixture]
[Category ( "General" )]
[Category ( "TypeStructure" )]
[Description ( "These tests are a layer of protection against potentially breaking changes to the definition of the SnapshotPeriodKind enum itself" )]
[TestOf ( typeof( SnapshotPeriodKind ) )]
public class SnapshotPeriodKindTests
{
    [Test]
    [Description ( "Guarding against additions to the enum" )]
    public void EnumEntries_AsExpected ( )
    {
        string[] names = Enum.GetNames<SnapshotPeriodKind>( );
        Assert.That ( names, Has.Length.EqualTo ( 7 ) );
    }

    [Test]
    [TestCase ( SnapshotPeriodKind.NotSet,   0 )]
    [TestCase ( SnapshotPeriodKind.Frequent, 1 )]
    [TestCase ( SnapshotPeriodKind.Hourly,   2 )]
    [TestCase ( SnapshotPeriodKind.Daily,    3 )]
    [TestCase ( SnapshotPeriodKind.Weekly,   4 )]
    [TestCase ( SnapshotPeriodKind.Monthly,  5 )]
    [TestCase ( SnapshotPeriodKind.Yearly,   6 )]
    [Description ( "Guarding against unintentional changes to the int values of the enum" )]
    public void EnumValues_AsExpected( SnapshotPeriodKind enumValue, int intValue )
    {
        Assert.That ( (int)enumValue, Is.EqualTo ( intValue ) );
    }
}
