// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Sanoid.Common.Configuration.Datasets;

namespace Sanoid.Common.Tests.Configuration.Datasets;

[TestFixture]
public class DatasetTests
{
    [Test]
    [TestCase( "/" )]
    [TestCase( "pool1" )]
    [TestCase( "pool1/dataset1" )]
    [TestCase( "pool1/dataset1/leaf" )]
    [TestCase( "pool1/dataset2" )]
    [TestCase( "pool1/dataset3" )]
    [TestCase( "pool1/zvol1" )]
    public void CheckVirtualPathIsRooted( string datasetZfsPath )
    {
        // Ensure expected virtual path for a dataset is rooted at the fake root '/'
        Dataset testDataset = new( datasetZfsPath );

        Assert.That( Path.IsPathRooted( testDataset.VirtualPath ), Is.True );
    }

    [Test]
    [TestCase( "/", ExpectedResult = false )]
    [TestCase( "pool1", ExpectedResult = true )]
    [TestCase( "pool1/dataset1", ExpectedResult = true )]
    [TestCase( "pool1/dataset1/leaf", ExpectedResult = true )]
    [TestCase( "pool1/dataset2", ExpectedResult = true )]
    [TestCase( "pool1/dataset3", ExpectedResult = true )]
    [TestCase( "pool1/zvol1", ExpectedResult = true )]
    public bool CheckVirtualPathNotEqualsPath( string datasetZfsPath )
    {
        // Except for the virtual root dataset, no dataset's VirtualPath should equal its real Path
        Dataset testDataset = new( datasetZfsPath );

        return testDataset.Path != testDataset.VirtualPath;
    }

    [Test]
    public void CheckHierarchyMaintainedOnParentLinkage( )
    {
        // Ensure that, when a parent dataset is added to a dataset, the parent's Children dictionary gets updated to include
        // a reference to the child the parent was added to.
        Dataset parentDataset = new( "zpool1/parent" );
        Dataset childDataset = new( "zpool1/parent/child" )
        {
            Parent = parentDataset
        };

        Assert.Multiple( ( ) =>
        {
            // Assert that the children of parentDataset contains a key equal to childDataset.VirtualPath
            Assert.That( parentDataset.Children, Contains.Key( childDataset.VirtualPath ) );
            // Assert that the object parentDataset.Children with that key is a reference to the original childDataset object.
            Assert.That( parentDataset.Children[ childDataset.VirtualPath ], Is.SameAs( childDataset ) );
        } );
    }
}
