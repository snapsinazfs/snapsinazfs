// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Globalization;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes.ZfsRecordTests;

[TestFixture]
[Category( "General" )]
[Parallelizable]
[TestOf( typeof( ZfsRecord ) )]
public class ZfsRecordTests
{
    private static readonly IReadOnlyList<string> UpdateProperty_DateTimeOffset_Values = new[] { "2022-01-01T00:00:00.0000000", "2023-01-01T00:00:00.0000000", "2023-07-01T00:00:00.0000000", "2023-07-03T00:00:00.0000000", "2023-07-03T00:00:00.0000000", "2023-07-03T00:00:00.0000000", "2023-07-03T01:00:00.0000000", "2023-07-03T01:15:00.0000000", "2023-12-31T23:59:59.9999999", "2024-01-01T00:00:00.0000000" };

    [Test]
    [TestCase( "sameName", ZfsPropertyValueConstants.FileSystem, "sameName", ZfsPropertyValueConstants.FileSystem, ExpectedResult = true )]
    [TestCase( "differentNameA", ZfsPropertyValueConstants.FileSystem, "differentNameB", ZfsPropertyValueConstants.FileSystem, ExpectedResult = false )]
    [TestCase( "sameName", ZfsPropertyValueConstants.FileSystem, "sameName", ZfsPropertyValueConstants.Volume, ExpectedResult = false )]
    [TestCase( "differentNameA", ZfsPropertyValueConstants.FileSystem, "differentNameB", ZfsPropertyValueConstants.Volume, ExpectedResult = false )]
    public bool EqualityIsByRecordValue( string datasetAName, string datasetAKind, string datasetBName, string datasetBKind )
    {
        // This test should be enhanced to check more cases of inequality
        // Providing a test case provider method would help with that
        ZfsRecord datasetA = new( datasetAName, datasetAKind );
        ZfsRecord datasetB = new( datasetBName, datasetBKind );
        return datasetA == datasetB;
    }

    [Test]
    [Combinatorial]
    public void UpdateProperty_Bool( [Values( ZfsPropertyNames.EnabledPropertyName, ZfsPropertyNames.TakeSnapshotsPropertyName, ZfsPropertyNames.PruneSnapshotsPropertyName )] string propertyName, [Values] bool propertyValue, [Values( ZfsPropertySourceConstants.Local, "inherited from testRoot" )] string propertySource )
    {
        ZfsRecord originalRecord = ZfsRecordTestHelpers.GetNewTestRootFileSystemFs1( );
        ZfsRecord updatedRecord = ZfsRecordTestHelpers.GetNewTestRootFileSystemFs1( );
        ZfsProperty<bool> newTestCaseProperty = new( propertyName, propertyValue, propertySource );
        ZfsProperty<bool> originalBoolProperty = (ZfsProperty<bool>)originalRecord[ propertyName ];

        Assume.That( updatedRecord, Is.EqualTo( originalRecord ), "Both records must be identical for this test to be valid" );
        Assume.That( updatedRecord, Is.Not.SameAs(originalRecord));

        ZfsProperty<bool> updatedBoolProperty = updatedRecord.UpdateProperty( propertyName, propertyValue, propertySource );

        if ( newTestCaseProperty == originalBoolProperty )
        {
            Assert.Multiple( ( ) =>
            {
                Assert.That( updatedBoolProperty, Is.EqualTo( originalBoolProperty ) );
                Assert.That( updatedBoolProperty, Is.EqualTo( newTestCaseProperty ) );
                Assert.That( updatedBoolProperty.Name, Is.EqualTo( propertyName ) );
                Assert.That( updatedBoolProperty.Value, Is.EqualTo( propertyValue ) );
                Assert.That( updatedBoolProperty.Source, Is.EqualTo( propertySource ) );

                Assert.That( updatedRecord, Is.EqualTo( originalRecord ) );
            } );
        }
        else
        {
            Assert.Multiple( ( ) =>
            {
                Assert.That( updatedBoolProperty, Is.Not.EqualTo( originalBoolProperty ) );
                Assert.That( updatedBoolProperty, Is.EqualTo( newTestCaseProperty ) );
                Assert.That( updatedBoolProperty.Name, Is.EqualTo( propertyName ) );
                Assert.That( updatedBoolProperty.Value, Is.EqualTo( propertyValue ) );
                Assert.That( updatedBoolProperty.Source, Is.EqualTo( propertySource ) );

                Assert.That( updatedRecord, Is.Not.EqualTo( originalRecord ) );
            } );
        }

        if ( propertySource != ZfsPropertySourceConstants.Local )
        {
            Assert.Multiple( ( ) =>
            {
                Assert.That( updatedRecord[ propertyName ], Has.Property( "IsInherited" ).True );
                Assert.That( updatedRecord[ propertyName ], Has.Property( "IsLocal" ).False );
                Assert.That( updatedRecord[ propertyName ], Has.Property( "InheritedFrom" ).EqualTo( propertySource[ 15.. ] ) );
            } );
        }
    }

    [Test]
    [Combinatorial]
    public void UpdateProperty_DateTimeOffset( [Values( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName )] string propertyName, [ValueSource( nameof( UpdateProperty_DateTimeOffset_Values ) )] string propertyValueString, [Values( ZfsPropertySourceConstants.Local, "inherited from testRoot" )] string propertySource )
    {
        DateTimeOffset propertyValue = DateTimeOffset.ParseExact( propertyValueString, "O", DateTimeFormatInfo.CurrentInfo );
        ZfsRecord originalRecord = ZfsRecordTestHelpers.GetNewTestRootFileSystemFs1( );
        ZfsRecord updatedRecord = ZfsRecordTestHelpers.GetNewTestRootFileSystemFs1( );
        ZfsProperty<DateTimeOffset> newTestCaseProperty = new( propertyName, propertyValue, propertySource );
        ZfsProperty<DateTimeOffset> originalDateTimeOffsetProperty = (ZfsProperty<DateTimeOffset>)originalRecord[ propertyName ];

        Assume.That( updatedRecord, Is.EqualTo( originalRecord ), "Both records must be identical for this test to be valid" );
        Assume.That( updatedRecord, Is.Not.SameAs(originalRecord));

        ZfsProperty<DateTimeOffset> updatedDateTimeOffsetProperty = updatedRecord.UpdateProperty( propertyName, propertyValue, propertySource );

        if ( newTestCaseProperty == originalDateTimeOffsetProperty )
        {
            Assert.Multiple( ( ) =>
            {
                Assert.That( updatedDateTimeOffsetProperty, Is.EqualTo( originalDateTimeOffsetProperty ) );
                Assert.That( updatedDateTimeOffsetProperty, Is.EqualTo( newTestCaseProperty ) );
                Assert.That( updatedDateTimeOffsetProperty.Name, Is.EqualTo( propertyName ) );
                Assert.That( updatedDateTimeOffsetProperty.Value, Is.EqualTo( propertyValue ) );
                Assert.That( updatedDateTimeOffsetProperty.Source, Is.EqualTo( propertySource ) );

                Assert.That( updatedRecord, Is.EqualTo( originalRecord ) );
            } );
        }
        else
        {
            Assert.Multiple( ( ) =>
            {
                Assert.That( updatedDateTimeOffsetProperty, Is.Not.EqualTo( originalDateTimeOffsetProperty ) );
                Assert.That( updatedDateTimeOffsetProperty, Is.EqualTo( newTestCaseProperty ) );
                Assert.That( updatedDateTimeOffsetProperty.Name, Is.EqualTo( propertyName ) );
                Assert.That( updatedDateTimeOffsetProperty.Value, Is.EqualTo( propertyValue ) );
                Assert.That( updatedDateTimeOffsetProperty.Source, Is.EqualTo( propertySource ) );

                Assert.That( updatedRecord, Is.Not.EqualTo( originalRecord ) );
            } );
        }

        if ( propertySource != ZfsPropertySourceConstants.Local )
        {
            Assert.Multiple( ( ) =>
            {
                Assert.That( updatedRecord[ propertyName ], Has.Property( "IsInherited" ).True );
                Assert.That( updatedRecord[ propertyName ], Has.Property( "IsLocal" ).False );
                Assert.That( updatedRecord[ propertyName ], Has.Property( "InheritedFrom" ).EqualTo( propertySource[ 15.. ] ) );
            } );
        }
    }

    [Test]
    [Combinatorial]
    public void UpdateProperty_Int( [Values( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, ZfsPropertyNames.SnapshotRetentionDailyPropertyName, ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName )] string propertyName, [Values( 0, 1, 2, 10, 100 )] int propertyValue, [Values( ZfsPropertySourceConstants.Local, "inherited from testRoot" )] string propertySource )
    {
        ZfsRecord originalRecord = ZfsRecordTestHelpers.GetNewTestRootFileSystemFs1( );
        ZfsRecord updatedRecord = ZfsRecordTestHelpers.GetNewTestRootFileSystemFs1( );
        ZfsProperty<int> newTestCaseProperty = new( propertyName, propertyValue, propertySource );
        ZfsProperty<int> originalIntProperty = (ZfsProperty<int>)originalRecord[ propertyName ];

        Assume.That( updatedRecord, Is.EqualTo( originalRecord ), "Both records must be identical for this test to be valid" );
        Assume.That( updatedRecord, Is.Not.SameAs(originalRecord));

        ZfsProperty<int> updatedIntProperty = updatedRecord.UpdateProperty( propertyName, propertyValue, propertySource );

        if ( newTestCaseProperty == originalIntProperty )
        {
            Assert.Multiple( ( ) =>
            {
                Assert.That( updatedIntProperty, Is.EqualTo( originalIntProperty ) );
                Assert.That( updatedIntProperty, Is.EqualTo( newTestCaseProperty ) );
                Assert.That( updatedIntProperty.Name, Is.EqualTo( propertyName ) );
                Assert.That( updatedIntProperty.Value, Is.EqualTo( propertyValue ) );
                Assert.That( updatedIntProperty.Source, Is.EqualTo( propertySource ) );

                Assert.That( updatedRecord, Is.EqualTo( originalRecord ) );
            } );
        }
        else
        {
            Assert.Multiple( ( ) =>
            {
                Assert.That( updatedIntProperty, Is.Not.EqualTo( originalIntProperty ) );
                Assert.That( updatedIntProperty, Is.EqualTo( newTestCaseProperty ) );
                Assert.That( updatedIntProperty.Name, Is.EqualTo( propertyName ) );
                Assert.That( updatedIntProperty.Value, Is.EqualTo( propertyValue ) );
                Assert.That( updatedIntProperty.Source, Is.EqualTo( propertySource ) );

                Assert.That( updatedRecord, Is.Not.EqualTo( originalRecord ) );
            } );
        }

        if ( propertySource != ZfsPropertySourceConstants.Local )
        {
            Assert.Multiple( ( ) =>
            {
                Assert.That( updatedRecord[ propertyName ], Has.Property( "IsInherited" ).True );
                Assert.That( updatedRecord[ propertyName ], Has.Property( "IsLocal" ).False );
                Assert.That( updatedRecord[ propertyName ], Has.Property( "InheritedFrom" ).EqualTo( propertySource[ 15.. ] ) );
            } );
        }
    }

    [Test]
    [Combinatorial]
    public void UpdateProperty_String( [Values( ZfsPropertyNames.RecursionPropertyName, ZfsPropertyNames.TemplatePropertyName )] string propertyName, [Values( "default", "testTemplate", "template with spaces" )] string propertyValue, [Values( ZfsPropertySourceConstants.Local, "inherited from testRoot" )] string propertySource )
    {
        ZfsRecord originalRecord = ZfsRecordTestHelpers.GetNewTestRootFileSystemFs1( );
        ZfsRecord updatedRecord = ZfsRecordTestHelpers.GetNewTestRootFileSystemFs1( );
        ZfsProperty<string> newTestCaseProperty = new( propertyName, propertyValue, propertySource );
        ZfsProperty<string> originalStringProperty = (ZfsProperty<string>)originalRecord[ propertyName ];

        Assume.That( updatedRecord, Is.EqualTo( originalRecord ), "Both records must be identical for this test to be valid" );
        Assume.That( updatedRecord, Is.Not.SameAs(originalRecord));

        ZfsProperty<string> updatedStringProperty = (ZfsProperty<string>)updatedRecord.UpdateProperty( propertyName, propertyValue, propertySource );

        if ( newTestCaseProperty == originalStringProperty )
        {
            Assert.Multiple( ( ) =>
            {
                Assert.That( updatedStringProperty, Is.EqualTo( originalStringProperty ) );
                Assert.That( updatedStringProperty, Is.EqualTo( newTestCaseProperty ) );
                Assert.That( updatedStringProperty.Name, Is.EqualTo( propertyName ) );
                Assert.That( updatedStringProperty.Value, Is.EqualTo( propertyValue ) );
                Assert.That( updatedStringProperty.Source, Is.EqualTo( propertySource ) );

                Assert.That( updatedRecord, Is.EqualTo( originalRecord ) );
            } );
        }
        else
        {
            Assert.Multiple( ( ) =>
            {
                Assert.That( updatedStringProperty, Is.Not.EqualTo( originalStringProperty ) );
                Assert.That( updatedStringProperty, Is.EqualTo( newTestCaseProperty ) );
                Assert.That( updatedStringProperty.Name, Is.EqualTo( propertyName ) );
                Assert.That( updatedStringProperty.Value, Is.EqualTo( propertyValue ) );
                Assert.That( updatedStringProperty.Source, Is.EqualTo( propertySource ) );

                Assert.That( updatedRecord, Is.Not.EqualTo( originalRecord ) );
            } );
        }

        if ( propertySource != ZfsPropertySourceConstants.Local )
        {
            Assert.Multiple( ( ) =>
            {
                Assert.That( updatedRecord[ propertyName ], Has.Property( "IsInherited" ).True );
                Assert.That( updatedRecord[ propertyName ], Has.Property( "IsLocal" ).False );
                Assert.That( updatedRecord[ propertyName ], Has.Property( "InheritedFrom" ).EqualTo( propertySource[ 15.. ] ) );
            } );
        }
    }
}
