#region MIT LICENSE

// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/

#endregion

using System.Collections.Concurrent;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsCommandRunner;

[TestFixture]
[TestOf( typeof( RawZfsObject ) )]
[Category( "General" )]
public class RawZfsObjectTests
{
    [Test]
    [TestCase( "available", ZfsPropertySourceConstants.None, 0 )]
    [TestCase( ZfsPropertyNames.EnabledPropertyName, ZfsPropertySourceConstants.Local, 1 )]
    [TestCase( ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, ZfsPropertySourceConstants.Local, 2 )]
    [TestCase( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, ZfsPropertySourceConstants.Local, 3 )]
    [TestCase( ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, ZfsPropertySourceConstants.Local, 4 )]
    [TestCase( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, ZfsPropertySourceConstants.Local, 5 )]
    [TestCase( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, ZfsPropertySourceConstants.Local, 6 )]
    [TestCase( ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, ZfsPropertySourceConstants.Local, 7 )]
    [TestCase( ZfsPropertyNames.PruneSnapshotsPropertyName, ZfsPropertySourceConstants.Local, 8 )]
    [TestCase( ZfsPropertyNames.RecursionPropertyName, ZfsPropertySourceConstants.Local, 9 )]
    [TestCase( ZfsPropertyNames.SnapshotRetentionDailyPropertyName, ZfsPropertySourceConstants.Local, 10 )]
    [TestCase( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, ZfsPropertySourceConstants.Local, 11 )]
    [TestCase( ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, ZfsPropertySourceConstants.Local, 12 )]
    [TestCase( ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, ZfsPropertySourceConstants.Local, 13 )]
    [TestCase( ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, ZfsPropertySourceConstants.Local, 14 )]
    [TestCase( ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, ZfsPropertySourceConstants.Local, 15 )]
    [TestCase( ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, ZfsPropertySourceConstants.Local, 16 )]
    [TestCase( ZfsPropertyNames.SnapshotPeriodPropertyName, ZfsPropertySourceConstants.Local, 17 )]
    [TestCase( ZfsPropertyNames.SnapshotTimestampPropertyName, ZfsPropertySourceConstants.Local, 18 )]
    [TestCase( ZfsPropertyNames.SourceSystem, ZfsPropertySourceConstants.Local, 19 )]
    [TestCase( ZfsPropertyNames.TakeSnapshotsPropertyName, ZfsPropertySourceConstants.Local, 20 )]
    [TestCase( ZfsPropertyNames.TemplatePropertyName, ZfsPropertySourceConstants.Local, 21 )]
    [TestCase( "type", ZfsPropertySourceConstants.None, 22 )]
    [TestCase( "used", ZfsPropertySourceConstants.None, 23 )]
    public void AddRawProperty_AllPropertiesCorrectAndInOrder( string propertyName, string propertySource, int expectedIndex )
    {
        RawZfsObject obj = new( ZfsPropertyValueConstants.FileSystem );
        TestCaseData[] rawPropertyTestCaseArray = GetAddRawProperty_TestCases( );
        foreach ( TestCaseData p in rawPropertyTestCaseArray )
        {
            obj.AddRawProperty( p.Arguments[ 0 ]!.ToString( )!, p.Arguments[ 1 ]!.ToString( )!, p.Arguments[ 2 ]!.ToString( )! );
        }

        Assert.Multiple( ( ) =>
        {
            Assert.That( obj.Properties, Contains.Key( propertyName ) );
            Assert.That( obj.Properties, Has.ItemAt( propertyName ).InstanceOf<RawProperty>( ) );
            Assert.That( obj.Properties.IndexOfKey( propertyName ), Is.EqualTo( expectedIndex ) );
            Assert.That( obj.Properties[ propertyName ].Name, Is.EqualTo( propertyName ) );
            Assert.That( obj.Properties[ propertyName ].Value, Is.EqualTo( rawPropertyTestCaseArray.Single( rp => (string)rp.Arguments[ 0 ] == propertyName ).Arguments[ 1 ] ) );
            Assert.That( obj.Properties[ propertyName ].Source, Is.EqualTo( propertySource ) );
        } );
    }

    [Test]
    [TestCase( ZfsPropertyValueConstants.FileSystem )]
    [TestCase( ZfsPropertyValueConstants.Volume )]
    [TestCase( ZfsPropertyValueConstants.Snapshot )]
    public void Constructor_KindCorrect( string kind )
    {
        RawZfsObject obj = new( kind );
        Assert.That( obj, Has.Property( "Kind" ).EqualTo( kind ) );
    }

    [Test]
    public void Constructor_PropertiesCollectionEmpty( )
    {
        RawZfsObject obj = new( ZfsPropertyValueConstants.FileSystem );
        Assert.Multiple( ( ) =>
        {
            Assert.That( obj, Is.Not.Null );
            Assert.That( obj.Properties, Is.Not.Null );
            Assert.That( obj.Properties, Is.Empty );
        } );
    }

    [Test]
    public void ConvertToDatasetAndAddToCollection_RootFileSystemAllPropertiesExist( )
    {
        ZfsRecord expectedRecord = ZfsRecord.CreateInstanceFromAllProperties( "testRoot",
                                                                              ZfsPropertyValueConstants.FileSystem,
                                                                              ZfsProperty<bool>.CreateWithoutParent( ZfsPropertyNames.EnabledPropertyName, true ),
                                                                              ZfsProperty<bool>.CreateWithoutParent( ZfsPropertyNames.TakeSnapshotsPropertyName, true ),
                                                                              ZfsProperty<bool>.CreateWithoutParent( ZfsPropertyNames.PruneSnapshotsPropertyName, true ),
                                                                              ZfsProperty<DateTimeOffset>.CreateWithoutParent( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ),
                                                                              ZfsProperty<DateTimeOffset>.CreateWithoutParent( ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ),
                                                                              ZfsProperty<DateTimeOffset>.CreateWithoutParent( ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ),
                                                                              ZfsProperty<DateTimeOffset>.CreateWithoutParent( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ),
                                                                              ZfsProperty<DateTimeOffset>.CreateWithoutParent( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ),
                                                                              ZfsProperty<DateTimeOffset>.CreateWithoutParent( ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ),
                                                                              ZfsProperty<string>.CreateWithoutParent( ZfsPropertyNames.RecursionPropertyName, ZfsPropertyValueConstants.SnapsInAZfs ),
                                                                              ZfsProperty<string>.CreateWithoutParent( ZfsPropertyNames.TemplatePropertyName, ZfsPropertyValueConstants.Default ),
                                                                              ZfsProperty<int>.CreateWithoutParent( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, 6 ),
                                                                              ZfsProperty<int>.CreateWithoutParent( ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, 5 ),
                                                                              ZfsProperty<int>.CreateWithoutParent( ZfsPropertyNames.SnapshotRetentionDailyPropertyName, 4 ),
                                                                              ZfsProperty<int>.CreateWithoutParent( ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, 3 ),
                                                                              ZfsProperty<int>.CreateWithoutParent( ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, 2 ),
                                                                              ZfsProperty<int>.CreateWithoutParent( ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, 1 ),
                                                                              ZfsProperty<int>.CreateWithoutParent( ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, 0 ),
                                                                              ZfsProperty<string>.CreateWithoutParent( ZfsPropertyNames.SourceSystem, "StandaloneSiazSystem" ),
                                                                              54321,
                                                                              12345
        );
        RawZfsObject testObject = new( ZfsPropertyValueConstants.FileSystem );
        testObject.AddRawProperty( ZfsNativePropertyNames.Type, ZfsPropertyValueConstants.FileSystem, ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty( ZfsPropertyNames.EnabledPropertyName, "true", ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty( ZfsPropertyNames.TakeSnapshotsPropertyName, "true", ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty( ZfsPropertyNames.PruneSnapshotsPropertyName, "true", ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty( ZfsPropertyNames.RecursionPropertyName, ZfsPropertyValueConstants.SnapsInAZfs, ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty( ZfsPropertyNames.TemplatePropertyName, ZfsPropertyValueConstants.Default, ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, "6", ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty( ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, "5", ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty( ZfsPropertyNames.SnapshotRetentionDailyPropertyName, "4", ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty( ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, "3", ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty( ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, "2", ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty( ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, "1", ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty( ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, "0", ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, "1970-01-01T00:00:00Z", ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty( ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, "1970-01-01T00:00:00Z", ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty( ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, "1970-01-01T00:00:00Z", ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, "1970-01-01T00:00:00Z", ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, "1970-01-01T00:00:00Z", ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty( ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, "1970-01-01T00:00:00Z", ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty( ZfsPropertyNames.SourceSystem, "StandaloneSiazSystem", ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty( ZfsNativePropertyNames.Available, "54321", ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty( ZfsNativePropertyNames.Used, "12345", ZfsPropertySourceConstants.Local );
        string dsName = "testRoot";
        ConcurrentDictionary<string, ZfsRecord> datasets = new( );
        testObject.ConvertToDatasetAndAddToCollection( dsName, datasets );

        Assert.That( datasets, Does.ContainKey( dsName ) );
        ZfsRecord ds = datasets[ dsName ];
        Assert.That( ds, Is.EqualTo( expectedRecord ) );
    }

    private static TestCaseData[] GetAddRawProperty_TestCases( )
    {
        return IZfsProperty.KnownDatasetProperties.Union( IZfsProperty.KnownSnapshotProperties ).Select( kp => new TestCaseData( kp, $"{kp} value", ZfsPropertySourceConstants.Local ) )
                           .Prepend( new( "type", ZfsPropertyValueConstants.FileSystem, ZfsPropertySourceConstants.None ) )
                           .Append( new( "available", "54321", ZfsPropertySourceConstants.None ) )
                           .Append( new( "used", "12345", ZfsPropertySourceConstants.None ) )
                           .ToArray( );
    }
}
