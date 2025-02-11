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

using System.Collections.Concurrent;
using System.Reflection;
using SnapsInAZfs.ConfigConsole.TreeNodes;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;

namespace SnapsInAZfs.Tests.ConfigConsole.TreeNodes;

[TestFixture]
[TestOf( typeof( ZfsObjectConfigurationTreeNode ) )]
public class ZfsObjectConfigurationTreeNodeTests
{
    [Test]
    public void CopyBaseDatasetPropertiesToTreeDataset_PropertiesInherited( )
    {
        GetStandardTestNodesTwoGenerations( out ZfsRecord baseRecordGen1, out ZfsRecord baseRecordGen2, out ZfsRecord treeRecordGen1, out ZfsRecord treeRecordGen2, out ZfsObjectConfigurationTreeNode _, out ZfsObjectConfigurationTreeNode testNodeGen2 );

        Assume.That( testNodeGen2.InheritPropertyFromParent( ZfsPropertyNames.EnabledPropertyName ), Is.True );
        Assume.That( testNodeGen2.InheritPropertyFromParent( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName ), Is.True );
        Assume.That( testNodeGen2.InheritPropertyFromParent( ZfsPropertyNames.RecursionPropertyName ), Is.True );
        Assume.That( testNodeGen2.TreeDataset.Enabled.Value, Is.False );
        Assume.That( testNodeGen2.TreeDataset.Enabled.IsInherited, Is.True );
        Assume.That( testNodeGen2.TreeDataset.SnapshotRetentionFrequent.Value, Is.EqualTo( -1 ) );
        Assume.That( testNodeGen2.TreeDataset.SnapshotRetentionFrequent.IsInherited, Is.True );
        Assume.That( testNodeGen2.TreeDataset.Recursion.Value, Is.EqualTo( ZfsPropertyValueConstants.SnapsInAZfs ) );
        Assume.That( testNodeGen2.TreeDataset.Recursion.IsInherited, Is.True );
        Assume.That( testNodeGen2.IsModified, Is.True );
        Assume.That( testNodeGen2.IsLocallyModified, Is.True );
        Assume.That( treeRecordGen1 == baseRecordGen1, Is.True );
        Assume.That( treeRecordGen2 == baseRecordGen2, Is.False );

        testNodeGen2.CopyBaseDatasetPropertiesToTreeDataset( );
        Assert.Multiple( ( ) =>
        {
            Assert.That( testNodeGen2.IsModified, Is.False );
            Assert.That( testNodeGen2.IsLocallyModified, Is.False );
            Assert.That( testNodeGen2.TreeDataset.Enabled.Value, Is.True );
            Assert.That( testNodeGen2.TreeDataset.Enabled.IsInherited, Is.False );
            Assert.That( testNodeGen2.TreeDataset.SnapshotRetentionFrequent.Value, Is.EqualTo( 99 ) );
            Assert.That( testNodeGen2.TreeDataset.SnapshotRetentionFrequent.IsInherited, Is.False );
            Assert.That( testNodeGen2.TreeDataset.Recursion.Value, Is.EqualTo( ZfsPropertyValueConstants.ZfsRecursion ) );
            Assert.That( testNodeGen2.TreeDataset.Recursion.IsInherited, Is.False );
            Assert.That( baseRecordGen2 == treeRecordGen2, Is.True );
            Assert.That( testNodeGen2.TreeDataset, Is.Not.SameAs( baseRecordGen2 ) );
            Assert.That( testNodeGen2.TreeDataset, Is.SameAs( treeRecordGen2 ) );
        } );
    }

    [Test]
    public void CopyBaseDatasetPropertiesToTreeDataset_PropertiesInherited_ThrowsOnDateTimeOffsetProperty( )
    {
        GetStandardTestNodesTwoGenerations( out ZfsRecord _, out ZfsRecord _, out ZfsRecord _, out ZfsRecord _, out ZfsObjectConfigurationTreeNode _, out ZfsObjectConfigurationTreeNode testNodeGen2 );

        FieldInfo inheritedPropertiesFieldInfo = typeof( ZfsObjectConfigurationTreeNode ).GetField( "_inheritedPropertiesSinceLastSave", BindingFlags.Instance | BindingFlags.NonPublic )!;
        ConcurrentDictionary<string, IZfsProperty> inheritedPropertiesSinceLastSave = (ConcurrentDictionary<string, IZfsProperty>)inheritedPropertiesFieldInfo.GetValue( testNodeGen2 )!;
        inheritedPropertiesSinceLastSave[ ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName ] = ZfsProperty<DateTimeOffset>.CreateWithoutParent( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.Now, false );

        Assert.That( ( ) => testNodeGen2.CopyBaseDatasetPropertiesToTreeDataset( ), Throws.InvalidOperationException );
    }

    [Test]
    public void CopyBaseDatasetPropertiesToTreeDataset_PropertiesUpdated( )
    {
        GetStandardTestNodesTwoGenerations( out ZfsRecord baseRecordGen1, out ZfsRecord baseRecordGen2, out ZfsRecord treeRecordGen1, out ZfsRecord treeRecordGen2, out ZfsObjectConfigurationTreeNode _, out ZfsObjectConfigurationTreeNode testNodeGen2 );

        testNodeGen2.UpdateTreeNodeProperty( ZfsPropertyNames.EnabledPropertyName, false );
        testNodeGen2.UpdateTreeNodeProperty( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, 0 );
        testNodeGen2.UpdateTreeNodeProperty( ZfsPropertyNames.RecursionPropertyName, ZfsPropertyValueConstants.SnapsInAZfs );
        Assume.That( testNodeGen2.TreeDataset.Enabled.Value, Is.False );
        Assume.That( testNodeGen2.TreeDataset.Enabled.IsInherited, Is.False );
        Assume.That( testNodeGen2.TreeDataset.SnapshotRetentionFrequent.Value, Is.EqualTo( 0 ) );
        Assume.That( testNodeGen2.TreeDataset.SnapshotRetentionFrequent.IsInherited, Is.False );
        Assume.That( testNodeGen2.TreeDataset.Recursion.Value, Is.EqualTo( ZfsPropertyValueConstants.SnapsInAZfs ) );
        Assume.That( testNodeGen2.TreeDataset.Recursion.IsInherited, Is.False );
        Assume.That( testNodeGen2.IsModified, Is.True );
        Assume.That( testNodeGen2.IsLocallyModified, Is.True );
        Assume.That( treeRecordGen1 == baseRecordGen1, Is.True );
        Assume.That( treeRecordGen2 == baseRecordGen2, Is.False );

        testNodeGen2.CopyBaseDatasetPropertiesToTreeDataset( );
        Assert.Multiple( ( ) =>
        {
            Assert.That( testNodeGen2.IsModified, Is.False );
            Assert.That( testNodeGen2.IsLocallyModified, Is.False );
            Assert.That( testNodeGen2.TreeDataset.Enabled.Value, Is.True );
            Assert.That( testNodeGen2.TreeDataset.Enabled.IsInherited, Is.False );
            Assert.That( testNodeGen2.TreeDataset.SnapshotRetentionFrequent.Value, Is.EqualTo( 99 ) );
            Assert.That( testNodeGen2.TreeDataset.SnapshotRetentionFrequent.IsInherited, Is.False );
            Assert.That( testNodeGen2.TreeDataset.Recursion.Value, Is.EqualTo( ZfsPropertyValueConstants.ZfsRecursion ) );
            Assert.That( testNodeGen2.TreeDataset.Recursion.IsInherited, Is.False );
            Assert.That( baseRecordGen2 == treeRecordGen2, Is.True );
            Assert.That( testNodeGen2.TreeDataset, Is.Not.SameAs( baseRecordGen2 ) );
            Assert.That( testNodeGen2.TreeDataset, Is.SameAs( treeRecordGen2 ) );
        } );
    }

    [Test]
    public void CopyBaseDatasetPropertiesToTreeDataset_PropertiesUpdated_ThrowsOnDateTimeOffsetProperty( )
    {
        GetStandardTestNodesTwoGenerations( out ZfsRecord _, out ZfsRecord _, out ZfsRecord _, out ZfsRecord _, out ZfsObjectConfigurationTreeNode _, out ZfsObjectConfigurationTreeNode testNodeGen2 );

        FieldInfo modifiedPropertiesFieldInfo = typeof( ZfsObjectConfigurationTreeNode ).GetField( "_modifiedPropertiesSinceLastSave", BindingFlags.Instance | BindingFlags.NonPublic )!;
        ConcurrentDictionary<string, IZfsProperty> modifiedPropertiesSinceLastSave = (ConcurrentDictionary<string, IZfsProperty>)modifiedPropertiesFieldInfo.GetValue( testNodeGen2 )!;
        modifiedPropertiesSinceLastSave[ ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName ] = ZfsProperty<DateTimeOffset>.CreateWithoutParent( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.Now, false );

        Assert.That( ( ) => testNodeGen2.CopyBaseDatasetPropertiesToTreeDataset( ), Throws.InvalidOperationException );
    }

    [Test]
    public void CopyTreeDatasetPropertiesToBaseDataset_PropertiesInherited( )
    {
        GetStandardTestNodesTwoGenerations( out ZfsRecord baseRecordGen1, out ZfsRecord baseRecordGen2, out ZfsRecord treeRecordGen1, out ZfsRecord treeRecordGen2, out ZfsObjectConfigurationTreeNode _, out ZfsObjectConfigurationTreeNode testNodeGen2 );

        Assume.That( testNodeGen2.InheritPropertyFromParent( ZfsPropertyNames.EnabledPropertyName ), Is.True );
        Assume.That( testNodeGen2.InheritPropertyFromParent( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName ), Is.True );
        Assume.That( testNodeGen2.InheritPropertyFromParent( ZfsPropertyNames.RecursionPropertyName ), Is.True );
        Assume.That( testNodeGen2.TreeDataset.Enabled.Value, Is.False );
        Assume.That( testNodeGen2.TreeDataset.SnapshotRetentionFrequent.Value, Is.EqualTo( -1 ) );
        Assume.That( testNodeGen2.TreeDataset.Recursion.Value, Is.EqualTo( ZfsPropertyValueConstants.SnapsInAZfs ) );
        Assume.That( testNodeGen2.IsModified, Is.True );
        Assume.That( testNodeGen2.IsLocallyModified, Is.True );
        Assume.That( treeRecordGen1 == baseRecordGen1, Is.True );
        Assume.That( treeRecordGen2 == baseRecordGen2, Is.False );

        testNodeGen2.CopyTreeDatasetPropertiesToBaseDataset( );
        Assert.Multiple( ( ) =>
        {
            Assert.That( testNodeGen2.IsModified, Is.False );
            Assert.That( testNodeGen2.IsLocallyModified, Is.False );
            Assert.That( baseRecordGen2.Enabled.Value, Is.False );
            Assert.That( baseRecordGen2.SnapshotRetentionFrequent.Value, Is.EqualTo( -1 ) );
            Assert.That( baseRecordGen2.Recursion, Is.EqualTo( ZfsPropertyValueConstants.SnapsInAZfs ) );
            Assert.That( treeRecordGen2 == baseRecordGen2, Is.True );
            Assert.That( testNodeGen2.TreeDataset, Is.Not.SameAs( baseRecordGen2 ) );
            Assert.That( testNodeGen2.TreeDataset, Is.SameAs( treeRecordGen2 ) );
        } );
    }

    [Test]
    public void CopyTreeDatasetPropertiesToBaseDataset_PropertiesInherited_ThrowsOnDateTimeOffsetProperty( )
    {
        GetStandardTestNodesTwoGenerations( out ZfsRecord _, out ZfsRecord _, out ZfsRecord _, out ZfsRecord _, out ZfsObjectConfigurationTreeNode _, out ZfsObjectConfigurationTreeNode testNodeGen2 );

        FieldInfo inheritedPropertiesFieldInfo = typeof( ZfsObjectConfigurationTreeNode ).GetField( "_inheritedPropertiesSinceLastSave", BindingFlags.Instance | BindingFlags.NonPublic )!;
        ConcurrentDictionary<string, IZfsProperty> inheritedPropertiesSinceLastSave = (ConcurrentDictionary<string, IZfsProperty>)inheritedPropertiesFieldInfo.GetValue( testNodeGen2 )!;
        inheritedPropertiesSinceLastSave[ ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName ] = ZfsProperty<DateTimeOffset>.CreateWithoutParent( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.Now, false );

        Assert.That( ( ) => testNodeGen2.CopyTreeDatasetPropertiesToBaseDataset( ), Throws.InvalidOperationException );
    }

    [Test]
    public void CopyTreeDatasetPropertiesToBaseDataset_PropertiesUpdated( )
    {
        GetStandardTestNodesTwoGenerations( out ZfsRecord _, out ZfsRecord baseRecordGen2, out ZfsRecord _, out ZfsRecord treeRecordGen2, out ZfsObjectConfigurationTreeNode _, out ZfsObjectConfigurationTreeNode testNodeGen2 );

        testNodeGen2.UpdateTreeNodeProperty( ZfsPropertyNames.EnabledPropertyName, false );
        testNodeGen2.UpdateTreeNodeProperty( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, 0 );
        testNodeGen2.UpdateTreeNodeProperty( ZfsPropertyNames.RecursionPropertyName, ZfsPropertyValueConstants.SnapsInAZfs );
        Assume.That( testNodeGen2.TreeDataset.Enabled.Value, Is.False );
        Assume.That( testNodeGen2.TreeDataset.SnapshotRetentionFrequent.Value, Is.EqualTo( 0 ) );
        Assume.That( testNodeGen2.TreeDataset.Recursion.Value, Is.EqualTo( ZfsPropertyValueConstants.SnapsInAZfs ) );
        Assume.That( testNodeGen2.IsModified, Is.True );
        Assume.That( testNodeGen2.IsLocallyModified, Is.True );
        Assume.That( treeRecordGen2 == baseRecordGen2, Is.False );

        testNodeGen2.CopyTreeDatasetPropertiesToBaseDataset( );
        Assert.Multiple( ( ) =>
        {
            Assert.That( testNodeGen2.IsModified, Is.False );
            Assert.That( testNodeGen2.IsLocallyModified, Is.False );
            Assert.That( baseRecordGen2.Enabled.Value, Is.False );
            Assert.That( baseRecordGen2.Enabled.IsInherited, Is.False );
            Assert.That( baseRecordGen2.SnapshotRetentionFrequent.Value, Is.EqualTo( 0 ) );
            Assert.That( baseRecordGen2.SnapshotRetentionFrequent.IsInherited, Is.False );
            Assert.That( baseRecordGen2.Recursion.Value, Is.EqualTo( ZfsPropertyValueConstants.SnapsInAZfs ) );
            Assert.That( baseRecordGen2.Recursion.IsInherited, Is.False );
            Assert.That( treeRecordGen2 == baseRecordGen2, Is.True );
            Assert.That( testNodeGen2.TreeDataset, Is.Not.SameAs( baseRecordGen2 ) );
            Assert.That( testNodeGen2.TreeDataset, Is.SameAs( treeRecordGen2 ) );
        } );
    }

    [Test]
    public void CopyTreeDatasetPropertiesToBaseDataset_PropertiesUpdated_ThrowsOnDateTimeOffsetProperty( )
    {
        GetStandardTestNodesTwoGenerations( out ZfsRecord _, out ZfsRecord _, out ZfsRecord _, out ZfsRecord _, out ZfsObjectConfigurationTreeNode _, out ZfsObjectConfigurationTreeNode testNodeGen2 );

        FieldInfo modifiedPropertiesFieldInfo = typeof( ZfsObjectConfigurationTreeNode ).GetField( "_modifiedPropertiesSinceLastSave", BindingFlags.Instance | BindingFlags.NonPublic )!;
        ConcurrentDictionary<string, IZfsProperty> modifiedPropertiesSinceLastSave = (ConcurrentDictionary<string, IZfsProperty>)modifiedPropertiesFieldInfo.GetValue( testNodeGen2 )!;
        modifiedPropertiesSinceLastSave[ ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName ] = ZfsProperty<DateTimeOffset>.CreateWithoutParent( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.Now, false );

        Assert.That( ( ) => testNodeGen2.CopyTreeDatasetPropertiesToBaseDataset( ), Throws.InvalidOperationException );
    }

    [Test]
    [TestCase( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, ExpectedResult = false )]
    [TestCase( ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, ExpectedResult = false )]
    [TestCase( ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, ExpectedResult = false )]
    [TestCase( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, ExpectedResult = false )]
    [TestCase( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, ExpectedResult = false )]
    [TestCase( ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, ExpectedResult = false )]
    public bool InheritPropertyFromParent_ReturnsFalseForInvalidProperties( string propertyName )
    {
        GetStandardTestNodesTwoGenerations( out ZfsRecord _, out ZfsRecord _, out ZfsRecord _, out ZfsRecord _, out ZfsObjectConfigurationTreeNode _, out ZfsObjectConfigurationTreeNode testNodeGen2 );
        return testNodeGen2.InheritPropertyFromParent( propertyName );
    }

    [Test]
    [TestCase( ZfsPropertyNames.SnapshotTimestampPropertyName, ExpectedResult = false )]
    public bool InheritPropertyFromParent_ReturnsFalseForKnownButNotAcceptedProperties( string propertyName )
    {
        GetStandardTestNodesTwoGenerations( out ZfsRecord _, out ZfsRecord _, out ZfsRecord _, out ZfsRecord _, out ZfsObjectConfigurationTreeNode _, out ZfsObjectConfigurationTreeNode testNodeGen2 );
        return testNodeGen2.InheritPropertyFromParent( propertyName );
    }

    [Test]
    public void InheritPropertyFromParent_ReturnsFalseForRootNode( )
    {
        GetStandardTestNodesTwoGenerations( out ZfsRecord _, out ZfsRecord _, out ZfsRecord _, out ZfsRecord _, out ZfsObjectConfigurationTreeNode testNodeGen1, out ZfsObjectConfigurationTreeNode _ );
        Assert.That( testNodeGen1.InheritPropertyFromParent( ZfsPropertyNames.EnabledPropertyName ), Is.False );
    }

    [Test]
    [TestCase( "BadName" )]
    [TestCase( "" )]
    public void InheritPropertyFromParent_ThrowsOnUnknownPropertyNames( string propertyName )
    {
        GetStandardTestNodesTwoGenerations( out ZfsRecord _, out ZfsRecord _, out ZfsRecord _, out ZfsRecord _, out ZfsObjectConfigurationTreeNode _, out ZfsObjectConfigurationTreeNode testNodeGen2 );
        Assert.That( ( ) => testNodeGen2.InheritPropertyFromParent( propertyName ), Throws.InstanceOf<ArgumentOutOfRangeException>( ) );
    }

    [Test]
    [TestCaseSource( nameof( GetCasesFor_InheritPropertyFromParent_ValuesAndInheritanceCorrect ) )]
    public void InheritPropertyFromParent_ValuesAndInheritanceCorrect<T>( string propertyName, ZfsObjectConfigurationTreeNode gen1, ZfsObjectConfigurationTreeNode gen2, PropertyInfo testPropertyInfo, T expectedGen1PropertyValue, T expectedGen2PropertyValue, bool expectedGen2PropertyChanged )
    {
        FieldInfo inheritedPropertiesFieldInfo = typeof( ZfsObjectConfigurationTreeNode ).GetField( "_inheritedPropertiesSinceLastSave", BindingFlags.Instance | BindingFlags.NonPublic )!;

        switch ( expectedGen1PropertyValue, expectedGen2PropertyValue )
        {
            case (bool gen1Expected, bool gen2Expected):
            {
                bool gen2InitialValue = ( (ZfsProperty<bool>)testPropertyInfo.GetValue( gen2.TreeDataset )! ).Value;
                gen2.InheritPropertyFromParent( propertyName );
                ConcurrentDictionary<string, IZfsProperty> inheritedPropertiesGen2 = (ConcurrentDictionary<string, IZfsProperty>)inheritedPropertiesFieldInfo.GetValue( gen2 )!;
                ZfsProperty<bool> gen1UpdatedProperty = (ZfsProperty<bool>)testPropertyInfo.GetValue( gen1.TreeDataset )!;
                ZfsProperty<bool> gen2UpdatedProperty = (ZfsProperty<bool>)testPropertyInfo.GetValue( gen2.TreeDataset )!;
                Assert.Multiple( ( ) =>
                {
                    Assert.That( inheritedPropertiesGen2.ContainsKey( propertyName ), Is.EqualTo( expectedGen2PropertyChanged ) );
                    Assert.That( gen1UpdatedProperty.Value, Is.EqualTo( gen1Expected ) );
                    Assert.That( gen2UpdatedProperty.Value, Is.EqualTo( gen2Expected ) );
                    Assert.That( gen2UpdatedProperty.Value == gen2InitialValue, Is.Not.EqualTo( expectedGen2PropertyChanged ) );
                    Assert.That( gen1UpdatedProperty.IsInherited, Is.False );
                    Assert.That( gen2UpdatedProperty.IsInherited, Is.True );
                    Assert.That( gen1.IsModified, Is.False );
                    Assert.That( gen1.IsLocallyModified, Is.False );
                    Assert.That( gen2.IsModified, Is.EqualTo( expectedGen2PropertyChanged ) );
                    Assert.That( gen2.IsLocallyModified, Is.EqualTo( expectedGen2PropertyChanged ) );
                } );
            }
                break;
            case (int gen1Expected, int gen2Expected):
            {
                int gen2InitialValue = ( (ZfsProperty<int>)testPropertyInfo.GetValue( gen2.TreeDataset )! ).Value;
                gen2.InheritPropertyFromParent( propertyName );
                ConcurrentDictionary<string, IZfsProperty> inheritedPropertiesGen2 = (ConcurrentDictionary<string, IZfsProperty>)inheritedPropertiesFieldInfo.GetValue( gen2 )!;
                ZfsProperty<int> gen1UpdatedProperty = (ZfsProperty<int>)testPropertyInfo.GetValue( gen1.TreeDataset )!;
                ZfsProperty<int> gen2UpdatedProperty = (ZfsProperty<int>)testPropertyInfo.GetValue( gen2.TreeDataset )!;
                Assert.Multiple( ( ) =>
                {
                    Assert.That( inheritedPropertiesGen2.ContainsKey( propertyName ), Is.EqualTo( expectedGen2PropertyChanged ) );
                    Assert.That( gen1UpdatedProperty.Value, Is.EqualTo( gen1Expected ) );
                    Assert.That( gen2UpdatedProperty.Value, Is.EqualTo( gen2Expected ) );
                    Assert.That( gen2UpdatedProperty.Value == gen2InitialValue, Is.Not.EqualTo( expectedGen2PropertyChanged ) );
                    Assert.That( gen1UpdatedProperty.IsInherited, Is.False );
                    Assert.That( gen2UpdatedProperty.IsInherited, Is.True );
                    Assert.That( gen1.IsModified, Is.False );
                    Assert.That( gen1.IsLocallyModified, Is.False );
                    Assert.That( gen2.IsModified, Is.EqualTo( expectedGen2PropertyChanged ) );
                    Assert.That( gen2.IsLocallyModified, Is.EqualTo( expectedGen2PropertyChanged ) );
                } );
            }
                break;
            case (string gen1Expected, string gen2Expected):
            {
                string gen2InitialValue = ( (ZfsProperty<string>)testPropertyInfo.GetValue( gen2.TreeDataset )! ).Value;
                gen2.InheritPropertyFromParent( propertyName );
                ConcurrentDictionary<string, IZfsProperty> inheritedPropertiesGen2 = (ConcurrentDictionary<string, IZfsProperty>)inheritedPropertiesFieldInfo.GetValue( gen2 )!;
                ZfsProperty<string> gen1UpdatedProperty = (ZfsProperty<string>)testPropertyInfo.GetValue( gen1.TreeDataset )!;
                ZfsProperty<string> gen2UpdatedProperty = (ZfsProperty<string>)testPropertyInfo.GetValue( gen2.TreeDataset )!;
                Assert.Multiple( ( ) =>
                {
                    Assert.That( inheritedPropertiesGen2.ContainsKey( propertyName ), Is.EqualTo( expectedGen2PropertyChanged ) );
                    Assert.That( gen1UpdatedProperty.Value, Is.EqualTo( gen1Expected ) );
                    Assert.That( gen2UpdatedProperty.Value, Is.EqualTo( gen2Expected ) );
                    Assert.That( gen2UpdatedProperty.Value == gen2InitialValue, Is.Not.EqualTo( expectedGen2PropertyChanged ) );
                    Assert.That( gen1UpdatedProperty.IsInherited, Is.False );
                    Assert.That( gen2UpdatedProperty.IsInherited, Is.True );
                    Assert.That( gen1.IsModified, Is.False );
                    Assert.That( gen1.IsLocallyModified, Is.False );
                    Assert.That( gen2.IsModified, Is.EqualTo( expectedGen2PropertyChanged ) );
                    Assert.That( gen2.IsLocallyModified, Is.EqualTo( expectedGen2PropertyChanged ) );
                } );
            }
                break;
        }
    }

    [Test]
    [TestCase( "gen1", ExpectedResult = "gen1" )]
    [TestCase( "gen1/gen2", ExpectedResult = "gen2" )]
    [TestCase( "gen1/gen2/gen3", ExpectedResult = "gen3" )]
    [TestCase( "gen1/gen2/gen3/gen4", ExpectedResult = "gen4" )]
    [TestCase( "gen1/gen2/gen3/gen4/gen5", ExpectedResult = "gen5" )]
    public string ToStringTest( string name )
    {
        ZfsRecord baseRecord = new( name, ZfsPropertyValueConstants.FileSystem, "host.domain.tld", false );
        ZfsRecord treeRecord = new( name, ZfsPropertyValueConstants.FileSystem, "host.domain.tld", false );

        ZfsObjectConfigurationTreeNode testNode = new( name, baseRecord, treeRecord );
        return testNode.ToString( );
    }

    [Test]
    [TestCaseSource( nameof( GetCasesFor_UpdateTreeNodeProperty_ValuesAndInheritanceCorrect ) )]
    public void UpdateTreeNodeProperty_ValuesAndInheritanceCorrect<T>( string propertyName, ZfsObjectConfigurationTreeNode gen1, ZfsObjectConfigurationTreeNode gen2, PropertyInfo testPropertyInfo, T newGen1PropertyValue, T expectedGen1PropertyValue, T expectedGen2PropertyValue, bool expectedGen2PropertyInheritance )
    {
        switch ( newGen1PropertyValue, expectedGen1PropertyValue, expectedGen2PropertyValue )
        {
            case (bool newGen1Value, bool gen1Expected, bool gen2Expected):
            {
                bool gen2InitialValue = ( (ZfsProperty<bool>)testPropertyInfo.GetValue( gen2.TreeDataset )! ).Value;
                gen1.UpdateTreeNodeProperty( propertyName, newGen1Value );
                ZfsProperty<bool> gen1UpdatedProperty = (ZfsProperty<bool>)testPropertyInfo.GetValue( gen1.TreeDataset )!;
                ZfsProperty<bool> gen2UpdatedProperty = (ZfsProperty<bool>)testPropertyInfo.GetValue( gen2.TreeDataset )!;
                Assert.Multiple( ( ) =>
                {
                    Assert.That( gen1UpdatedProperty.Value, Is.EqualTo( gen1Expected ) );
                    Assert.That( gen2UpdatedProperty.Value, Is.EqualTo( gen2Expected ) );
                    Assert.That( gen2UpdatedProperty.Value == gen2InitialValue, Is.Not.EqualTo( expectedGen2PropertyInheritance ) );
                    Assert.That( gen1UpdatedProperty.IsInherited, Is.False );
                    Assert.That( gen2UpdatedProperty.IsInherited, Is.EqualTo( expectedGen2PropertyInheritance ) );
                    Assert.That( gen1.IsModified, Is.True );
                    Assert.That( gen1.IsLocallyModified, Is.True );
                    Assert.That( gen2.IsModified, Is.EqualTo( expectedGen2PropertyInheritance ) );
                    Assert.That( gen2.IsLocallyModified, Is.False );
                } );
            }
                break;
            case (int newGen1Value, int gen1Expected, int gen2Expected):
            {
                int gen2InitialValue = ( (ZfsProperty<int>)testPropertyInfo.GetValue( gen2.TreeDataset )! ).Value;
                gen1.UpdateTreeNodeProperty( propertyName, newGen1Value );
                ZfsProperty<int> gen1UpdatedProperty = (ZfsProperty<int>)testPropertyInfo.GetValue( gen1.TreeDataset )!;
                ZfsProperty<int> gen2UpdatedProperty = (ZfsProperty<int>)testPropertyInfo.GetValue( gen2.TreeDataset )!;
                Assert.Multiple( ( ) =>
                {
                    Assert.That( gen1UpdatedProperty.Value, Is.EqualTo( gen1Expected ) );
                    Assert.That( gen2UpdatedProperty.Value, Is.EqualTo( gen2Expected ) );
                    Assert.That( gen2UpdatedProperty.Value == gen2InitialValue, Is.Not.EqualTo( expectedGen2PropertyInheritance ) );
                    Assert.That( gen1UpdatedProperty.IsInherited, Is.False );
                    Assert.That( gen2UpdatedProperty.IsInherited, Is.EqualTo( expectedGen2PropertyInheritance ) );
                    Assert.That( gen1.IsModified, Is.True );
                    Assert.That( gen1.IsLocallyModified, Is.True );
                    Assert.That( gen2.IsModified, Is.EqualTo( expectedGen2PropertyInheritance ) );
                    Assert.That( gen2.IsLocallyModified, Is.False );
                } );
            }
                break;
            case (string newGen1Value, string gen1Expected, string gen2Expected):
            {
                string gen2InitialValue = ( (ZfsProperty<string>)testPropertyInfo.GetValue( gen2.TreeDataset )! ).Value;
                gen1.UpdateTreeNodeProperty( propertyName, newGen1Value );
                ZfsProperty<string> gen1UpdatedProperty = (ZfsProperty<string>)testPropertyInfo.GetValue( gen1.TreeDataset )!;
                ZfsProperty<string> gen2UpdatedProperty = (ZfsProperty<string>)testPropertyInfo.GetValue( gen2.TreeDataset )!;
                Assert.Multiple( ( ) =>
                {
                    Assert.That( gen1UpdatedProperty.Value, Is.EqualTo( gen1Expected ) );
                    Assert.That( gen2UpdatedProperty.Value, Is.EqualTo( gen2Expected ) );
                    Assert.That( gen2UpdatedProperty.Value == gen2InitialValue, Is.Not.EqualTo( expectedGen2PropertyInheritance ) );
                    Assert.That( gen1UpdatedProperty.IsInherited, Is.False );
                    Assert.That( gen2UpdatedProperty.IsInherited, Is.EqualTo( expectedGen2PropertyInheritance ) );
                    Assert.That( gen1.IsModified, Is.True );
                    Assert.That( gen1.IsLocallyModified, Is.True );
                    Assert.That( gen2.IsModified, Is.EqualTo( expectedGen2PropertyInheritance ) );
                    Assert.That( gen2.IsLocallyModified, Is.False );
                } );
            }
                break;
        }
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

    private static IEnumerable<TestCaseData> GetCasesFor_InheritPropertyFromParent_ValuesAndInheritanceCorrect( )
    {
        PropertyInfo takeSnapshotsPropertyInfo = typeof( ZfsRecord ).GetProperty( "TakeSnapshots" )!;
        PropertyInfo enabledPropertyInfo = typeof( ZfsRecord ).GetProperty( "Enabled" )!;
        PropertyInfo snapshotRetentionFrequentPropertyInfo = typeof( ZfsRecord ).GetProperty( "SnapshotRetentionFrequent" )!;
        PropertyInfo snapshotRetentionHourlyPropertyInfo = typeof( ZfsRecord ).GetProperty( "SnapshotRetentionHourly" )!;
        PropertyInfo recursionPropertyInfo = typeof( ZfsRecord ).GetProperty( "Recursion" )!;
        PropertyInfo templatePropertyInfo = typeof( ZfsRecord ).GetProperty( "Template" )!;

        GetStandardTestNodesTwoGenerations( out ZfsRecord _, out ZfsRecord _, out _, out _, out ZfsObjectConfigurationTreeNode? testNodeGen1, out ZfsObjectConfigurationTreeNode? testNodeGen2 );
        yield return new( ZfsPropertyNames.EnabledPropertyName, testNodeGen1, testNodeGen2, enabledPropertyInfo, false, false, true ) { TestName = "InheritPropertyFromParent_ValuesAndInheritanceCorrect_BoolProperty_NotInherited" };

        GetStandardTestNodesTwoGenerations( out ZfsRecord _, out ZfsRecord _, out _, out _, out testNodeGen1, out testNodeGen2 );
        yield return new( ZfsPropertyNames.TakeSnapshotsPropertyName, testNodeGen1, testNodeGen2, takeSnapshotsPropertyInfo, false, false, false ) { TestName = "InheritPropertyFromParent_ValuesAndInheritanceCorrect_BoolProperty_Inherited" };

        GetStandardTestNodesTwoGenerations( out ZfsRecord _, out ZfsRecord _, out _, out _, out testNodeGen1, out testNodeGen2 );
        yield return new( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, testNodeGen1, testNodeGen2, snapshotRetentionFrequentPropertyInfo, -1, -1, true ) { TestName = "InheritPropertyFromParent_ValuesAndInheritanceCorrect_IntProperty_NotInherited" };

        GetStandardTestNodesTwoGenerations( out ZfsRecord _, out ZfsRecord _, out _, out _, out testNodeGen1, out testNodeGen2 );
        yield return new( ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, testNodeGen1, testNodeGen2, snapshotRetentionHourlyPropertyInfo, -1, -1, false ) { TestName = "InheritPropertyFromParent_ValuesAndInheritanceCorrect_IntProperty_Inherited" };

        GetStandardTestNodesTwoGenerations( out ZfsRecord _, out ZfsRecord _, out _, out _, out testNodeGen1, out testNodeGen2 );
        yield return new( ZfsPropertyNames.RecursionPropertyName, testNodeGen1, testNodeGen2, recursionPropertyInfo, ZfsPropertyValueConstants.SnapsInAZfs, ZfsPropertyValueConstants.SnapsInAZfs, true ) { TestName = "InheritPropertyFromParent_ValuesAndInheritanceCorrect_StringProperty_NotInherited" };

        GetStandardTestNodesTwoGenerations( out ZfsRecord _, out ZfsRecord _, out _, out _, out testNodeGen1, out testNodeGen2 );
        yield return new( ZfsPropertyNames.TemplatePropertyName, testNodeGen1, testNodeGen2, templatePropertyInfo, ZfsPropertyValueConstants.Default, ZfsPropertyValueConstants.Default, false ) { TestName = "InheritPropertyFromParent_ValuesAndInheritanceCorrect_StringProperty_Inherited" };
    }

    private static IEnumerable<TestCaseData> GetCasesFor_UpdateTreeNodeProperty_ValuesAndInheritanceCorrect( )
    {
        PropertyInfo takeSnapshotsPropertyInfo = typeof( ZfsRecord ).GetProperty( "TakeSnapshots" )!;
        PropertyInfo enabledPropertyInfo = typeof( ZfsRecord ).GetProperty( "Enabled" )!;
        PropertyInfo snapshotRetentionFrequentPropertyInfo = typeof( ZfsRecord ).GetProperty( "SnapshotRetentionFrequent" )!;
        PropertyInfo snapshotRetentionHourlyPropertyInfo = typeof( ZfsRecord ).GetProperty( "SnapshotRetentionHourly" )!;
        PropertyInfo recursionPropertyInfo = typeof( ZfsRecord ).GetProperty( "Recursion" )!;
        PropertyInfo templatePropertyInfo = typeof( ZfsRecord ).GetProperty( "Template" )!;

        GetStandardTestNodesTwoGenerations( out ZfsRecord _, out ZfsRecord _, out _, out _, out ZfsObjectConfigurationTreeNode? testNodeGen1, out ZfsObjectConfigurationTreeNode? testNodeGen2 );
        yield return new( ZfsPropertyNames.EnabledPropertyName, testNodeGen1, testNodeGen2, enabledPropertyInfo, true, true, true, false ) { TestName = "UpdateTreeNodeProperty_ValuesAndInheritanceCorrect_BoolProperty_NotInherited" };

        GetStandardTestNodesTwoGenerations( out ZfsRecord _, out ZfsRecord _, out _, out _, out testNodeGen1, out testNodeGen2 );
        yield return new( ZfsPropertyNames.TakeSnapshotsPropertyName, testNodeGen1, testNodeGen2, takeSnapshotsPropertyInfo, true, true, true, true ) { TestName = "UpdateTreeNodeProperty_ValuesAndInheritanceCorrect_BoolProperty_Inherited" };

        GetStandardTestNodesTwoGenerations( out ZfsRecord _, out ZfsRecord _, out _, out _, out testNodeGen1, out testNodeGen2 );
        yield return new( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, testNodeGen1, testNodeGen2, snapshotRetentionFrequentPropertyInfo, 0, 0, 99, false ) { TestName = "UpdateTreeNodeProperty_ValuesAndInheritanceCorrect_IntProperty_NotInherited" };

        GetStandardTestNodesTwoGenerations( out ZfsRecord _, out ZfsRecord _, out _, out _, out testNodeGen1, out testNodeGen2 );
        yield return new( ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, testNodeGen1, testNodeGen2, snapshotRetentionHourlyPropertyInfo, 0, 0, 0, true ) { TestName = "UpdateTreeNodeProperty_ValuesAndInheritanceCorrect_IntProperty_Inherited" };

        GetStandardTestNodesTwoGenerations( out ZfsRecord _, out ZfsRecord _, out _, out _, out testNodeGen1, out testNodeGen2 );
        yield return new( ZfsPropertyNames.RecursionPropertyName, testNodeGen1, testNodeGen2, recursionPropertyInfo, ZfsPropertyValueConstants.ZfsRecursion, ZfsPropertyValueConstants.ZfsRecursion, ZfsPropertyValueConstants.ZfsRecursion, false ) { TestName = "UpdateTreeNodeProperty_ValuesAndInheritanceCorrect_StringProperty_NotInherited" };

        GetStandardTestNodesTwoGenerations( out ZfsRecord _, out ZfsRecord _, out _, out _, out testNodeGen1, out testNodeGen2 );
        yield return new( ZfsPropertyNames.TemplatePropertyName, testNodeGen1, testNodeGen2, templatePropertyInfo, "newString", "newString", "newString", true ) { TestName = "UpdateTreeNodeProperty_ValuesAndInheritanceCorrect_StringProperty_Inherited" };
    }

    private static void GetStandardTestNodesTwoGenerations( out ZfsRecord baseRecordGen1, out ZfsRecord baseRecordGen2, out ZfsRecord treeRecordGen1, out ZfsRecord treeRecordGen2, out ZfsObjectConfigurationTreeNode testNodeGen1, out ZfsObjectConfigurationTreeNode testNodeGen2 )
    {
        PropertyInfo baseDatasetPropertyInfo = typeof( ZfsObjectConfigurationTreeNode ).GetProperty( "BaseDataset", BindingFlags.NonPublic | BindingFlags.Instance )!;

        baseRecordGen1 = new( "gen1", ZfsPropertyValueConstants.FileSystem, "host.domain.tld", false );
        baseRecordGen2 = baseRecordGen1.CreateChildDataset( "gen1/gen2", ZfsPropertyValueConstants.FileSystem, "host.domain.tld" );
        baseRecordGen2.UpdateProperty( ZfsPropertyNames.EnabledPropertyName, true );
        baseRecordGen2.UpdateProperty( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, 99 );
        baseRecordGen2.UpdateProperty( ZfsPropertyNames.RecursionPropertyName, ZfsPropertyValueConstants.ZfsRecursion );
        treeRecordGen1 = new( "gen1", ZfsPropertyValueConstants.FileSystem, "host.domain.tld", false );
        treeRecordGen2 = treeRecordGen1.CreateChildDataset( "gen1/gen2", ZfsPropertyValueConstants.FileSystem, "host.domain.tld" );
        treeRecordGen2.UpdateProperty( ZfsPropertyNames.EnabledPropertyName, true );
        treeRecordGen2.UpdateProperty( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, 99 );
        treeRecordGen2.UpdateProperty( ZfsPropertyNames.RecursionPropertyName, ZfsPropertyValueConstants.ZfsRecursion );

        // Set up property values for the base record of gen2
        baseRecordGen2.UpdateProperty( ZfsPropertyNames.EnabledPropertyName, true );
        baseRecordGen2.InheritBoolPropertyFromParent( ZfsPropertyNames.TakeSnapshotsPropertyName );
        baseRecordGen2.UpdateProperty( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, 99 );
        baseRecordGen2.InheritIntPropertyFromParent( ZfsPropertyNames.SnapshotRetentionHourlyPropertyName );
        baseRecordGen2.UpdateProperty( ZfsPropertyNames.RecursionPropertyName, ZfsPropertyValueConstants.ZfsRecursion );
        baseRecordGen2.InheritStringPropertyFromParent( ZfsPropertyNames.TemplatePropertyName );

        // Set the same property values for the tree record of gen2
        treeRecordGen2.UpdateProperty( ZfsPropertyNames.EnabledPropertyName, true );
        treeRecordGen2.InheritBoolPropertyFromParent( ZfsPropertyNames.TakeSnapshotsPropertyName );
        treeRecordGen2.UpdateProperty( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, 99 );
        treeRecordGen2.InheritIntPropertyFromParent( ZfsPropertyNames.SnapshotRetentionHourlyPropertyName );
        treeRecordGen2.UpdateProperty( ZfsPropertyNames.RecursionPropertyName, ZfsPropertyValueConstants.ZfsRecursion );
        treeRecordGen2.InheritStringPropertyFromParent( ZfsPropertyNames.TemplatePropertyName );

        testNodeGen1 = new( "gen1", baseRecordGen1, treeRecordGen1 );
        testNodeGen2 = new( "gen1/gen2", baseRecordGen2, treeRecordGen2 );
        testNodeGen1.Children.Add( testNodeGen2 );

        Assume.That( baseRecordGen1 == treeRecordGen1, Is.True );
        Assume.That( baseRecordGen2 == treeRecordGen2, Is.True );

        Assume.That( baseRecordGen1.Enabled.Value, Is.False );
        Assume.That( baseRecordGen2.Enabled.Value, Is.True );

        Assume.That( baseRecordGen1.TakeSnapshots.Value, Is.False );
        Assume.That( baseRecordGen2.TakeSnapshots.Value, Is.False );
        Assume.That( baseRecordGen2.TakeSnapshots.IsInherited, Is.True );

        Assume.That( baseRecordGen1.SnapshotRetentionFrequent.Value, Is.EqualTo( -1 ) );
        Assume.That( baseRecordGen2.SnapshotRetentionFrequent.Value, Is.EqualTo( 99 ) );

        Assume.That( baseRecordGen1.SnapshotRetentionHourly.Value, Is.EqualTo( -1 ) );
        Assume.That( baseRecordGen2.SnapshotRetentionHourly.Value, Is.EqualTo( -1 ) );
        Assume.That( baseRecordGen2.SnapshotRetentionHourly.IsInherited, Is.True );

        Assume.That( baseRecordGen1.Recursion.Value, Is.EqualTo( ZfsPropertyValueConstants.SnapsInAZfs ) );
        Assume.That( baseRecordGen2.Recursion.Value, Is.EqualTo( ZfsPropertyValueConstants.ZfsRecursion ) );

        Assume.That( baseRecordGen1.Template.Value, Is.EqualTo( ZfsPropertyValueConstants.Default ) );
        Assume.That( baseRecordGen2.Template.Value, Is.EqualTo( ZfsPropertyValueConstants.Default ) );
        Assume.That( baseRecordGen2.Template.IsInherited, Is.True );

        Assume.That( testNodeGen1.Children, Does.Contain( testNodeGen2 ) );
        Assume.That( testNodeGen1.IsModified, Is.False );
        Assume.That( testNodeGen1.IsLocallyModified, Is.False );
        Assume.That( testNodeGen2.IsModified, Is.False );
        Assume.That( testNodeGen2.IsLocallyModified, Is.False );

        Assume.That( baseDatasetPropertyInfo.GetValue( testNodeGen2 ), Is.SameAs( baseRecordGen2 ) );
        Assume.That( baseDatasetPropertyInfo.GetValue( testNodeGen2 ), Is.Not.SameAs( treeRecordGen2 ) );
    }
}
