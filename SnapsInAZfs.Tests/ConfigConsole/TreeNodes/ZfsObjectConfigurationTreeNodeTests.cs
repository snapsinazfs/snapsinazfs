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

using System.Reflection;
using SnapsInAZfs.ConfigConsole.TreeNodes;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;

namespace SnapsInAZfs.Tests.ConfigConsole.TreeNodes;

[TestFixture]
[TestOf( typeof( ZfsObjectConfigurationTreeNode ) )]
public class ZfsObjectConfigurationTreeNodeTests
{
    [Test]
    public void CopyBaseDatasetPropertiesToTreeDataset( )
    {
        ZfsRecord baseRecord = new( "testRoot", ZfsPropertyValueConstants.FileSystem, "host.domain.tld", false );
        ZfsRecord treeRecord = new( "testRoot", ZfsPropertyValueConstants.FileSystem, "host.domain.tld", false );
        Assume.That( baseRecord == treeRecord, Is.True );
        ZfsObjectConfigurationTreeNode testNode = new( "testRoot", baseRecord, treeRecord );
        Assume.That( testNode.IsModified, Is.False );
        Assume.That( testNode.IsLocallyModified, Is.False );
        Assume.That( testNode.TreeDataset, Is.SameAs( treeRecord ) );
        PropertyInfo baseDatasetPropertyInfo = typeof( ZfsObjectConfigurationTreeNode ).GetProperty( "BaseDataset", BindingFlags.NonPublic | BindingFlags.Instance )!;
        Assume.That( baseDatasetPropertyInfo.GetValue( testNode ), Is.SameAs( baseRecord ) );
        Assume.That( baseDatasetPropertyInfo.GetValue( testNode ), Is.Not.SameAs( treeRecord ) );
        Assume.That( treeRecord.Enabled.Value, Is.False );
        testNode.UpdateTreeNodeProperty( ZfsPropertyNames.EnabledPropertyName, true );
        Assume.That( testNode.TreeDataset.Enabled.Value, Is.True );
        Assume.That( testNode.IsModified,Is.True  );
        Assume.That( testNode.IsLocallyModified,Is.True  );
        Assume.That( treeRecord == baseRecord, Is.False );

        testNode.CopyBaseDatasetPropertiesToTreeDataset();
        Assert.Multiple( ( ) =>
        {
            Assert.That( testNode.IsModified, Is.False );
            Assert.That( testNode.IsLocallyModified, Is.False );
            Assert.That( testNode.TreeDataset.Enabled.Value, Is.False );
            Assert.That( treeRecord == baseRecord, Is.True );
            Assert.That( testNode.TreeDataset, Is.Not.SameAs( baseRecord ) );
            Assert.That( testNode.TreeDataset, Is.SameAs( treeRecord ) );
        } );
    }

    [Test]
    public void CopyTreeDatasetPropertiesToBaseDatasetTest( )
    {
        ZfsRecord baseRecord = new( "testRoot", ZfsPropertyValueConstants.FileSystem, "host.domain.tld", false );
        ZfsRecord treeRecord = new( "testRoot", ZfsPropertyValueConstants.FileSystem, "host.domain.tld", false );
        Assume.That( baseRecord == treeRecord, Is.True );
        ZfsObjectConfigurationTreeNode testNode = new( "testRoot", baseRecord, treeRecord );
        Assume.That( testNode.IsModified, Is.False );
        Assume.That( testNode.IsLocallyModified, Is.False );
        Assume.That( testNode.TreeDataset, Is.SameAs( treeRecord ) );
        PropertyInfo baseDatasetPropertyInfo = typeof( ZfsObjectConfigurationTreeNode ).GetProperty( "BaseDataset", BindingFlags.NonPublic | BindingFlags.Instance )!;
        Assume.That( baseDatasetPropertyInfo.GetValue( testNode ), Is.SameAs( baseRecord ) );
        Assume.That( baseDatasetPropertyInfo.GetValue( testNode ), Is.Not.SameAs( treeRecord ) );
        Assume.That( treeRecord.Enabled.Value, Is.False );
        testNode.UpdateTreeNodeProperty( ZfsPropertyNames.EnabledPropertyName, true );
        Assume.That( testNode.TreeDataset.Enabled.Value, Is.True );
        Assume.That( testNode.IsModified,Is.True  );
        Assume.That( testNode.IsLocallyModified,Is.True  );
        Assume.That( treeRecord == baseRecord, Is.False );

        testNode.CopyTreeDatasetPropertiesToBaseDataset();
        Assert.Multiple( ( ) =>
        {
            Assert.That( testNode.IsModified, Is.False );
            Assert.That( testNode.IsLocallyModified, Is.False );
            Assert.That( baseRecord.Enabled.Value, Is.True );
            Assert.That( testNode.TreeDataset.Enabled.Value, Is.True );
            Assert.That( treeRecord == baseRecord, Is.True );
            Assert.That( testNode.TreeDataset, Is.Not.SameAs( baseRecord ) );
            Assert.That( testNode.TreeDataset, Is.SameAs( treeRecord ) );
        } );
    }

    [Test]
    public void InheritPropertyFromParentTest( )
    {
        Assert.Ignore();
    }

    [Test]
    public void ToStringTest( )
    {
        Assert.Ignore();
    }

    [Test]
    public void UpdateTreeNodePropertyTest( )
    {
        Assert.Ignore();
    }

    [Test]
    public void ZfsObjectConfigurationTreeNode_Constructor( )
    {
        ZfsRecord baseRecord = new( "testRoot", ZfsPropertyValueConstants.FileSystem, "host.domain.tld", false );
        ZfsRecord treeRecord = new( "testRoot", ZfsPropertyValueConstants.FileSystem, "host.domain.tld", false );
        Assume.That( baseRecord == treeRecord, Is.True );
        ZfsObjectConfigurationTreeNode testNode = new( "testRoot", baseRecord, treeRecord );
        PropertyInfo baseDatasetPropertyInfo = typeof( ZfsObjectConfigurationTreeNode ).GetProperty( "BaseDataset", BindingFlags.NonPublic | BindingFlags.Instance )!;
        Assert.Multiple( ( ) =>
        {
            Assert.That( testNode.Text, Is.EqualTo( "testRoot" ) );
            Assert.That( testNode.Children, Is.Empty );
            Assert.That( testNode.IsModified, Is.False );
            Assert.That( testNode.IsLocallyModified, Is.False );
            Assert.That( testNode.TreeDataset, Is.SameAs( treeRecord ) );
            Assert.That( baseDatasetPropertyInfo.GetValue( testNode ), Is.SameAs( baseRecord ) );
            Assert.That( baseDatasetPropertyInfo.GetValue( testNode ), Is.Not.SameAs( treeRecord ) );
        } );
    }
}
