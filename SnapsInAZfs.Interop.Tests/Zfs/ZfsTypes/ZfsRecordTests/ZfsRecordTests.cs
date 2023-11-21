// LICENSE:
// 
// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Globalization;
using SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes.SnapshotTests;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes.ZfsRecordTests;

[TestFixture]
[Category( "General" )]
[Parallelizable]
[TestOf( typeof( ZfsRecord ) )]
public class ZfsRecordTests
{
    private static readonly IReadOnlyList<string> UpdateProperty_DateTimeOffset_Values = new[] { "2022-01-01T00:00:00.0000000", "2023-01-01T00:00:00.0000000", "2023-07-01T00:00:00.0000000", "2023-07-03T00:00:00.0000000", "2023-07-03T00:00:00.0000000", "2023-07-03T00:00:00.0000000", "2023-07-03T01:00:00.0000000", "2023-07-03T01:15:00.0000000", "2023-12-31T23:59:59.9999999", "2024-01-01T00:00:00.0000000" };

    [Test]
    public void BoolPropertyChangedEventHandler_SubscribersReceiveEventAndUpdateProperties_ChildPropertyIsInherited( )
    {
        ZfsRecord originalParent = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        ZfsRecord originalChild = originalParent.CreateChildDataset( "testRoot/fs1", ZfsPropertyValueConstants.FileSystem, "testSystem" );

        ZfsRecord updatedParent = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        ZfsRecord updatedChild = updatedParent.CreateChildDataset( "testRoot/fs1", ZfsPropertyValueConstants.FileSystem, "testSystem" );

        Assume.That( updatedChild.Equals( originalChild ), Is.True );
        Assume.That( updatedChild.Enabled.IsInherited, Is.True );
        Assume.That( updatedChild.TakeSnapshots.IsInherited, Is.True );
        Assume.That( updatedChild.PruneSnapshots.IsInherited, Is.True );

#pragma warning disable NUnit2010 // Use EqualConstraint for better assertion messages in case of failure
        ZfsProperty<bool> originalEnabledProperty = originalChild.Enabled;
        ZfsProperty<bool> updatedChildEnabledProperty_BeforeUpdate = updatedChild.Enabled;
        ZfsProperty<bool> updatedChildEnabledProperty_AfterUpdate = updatedParent.UpdateProperty( ZfsPropertyNames.EnabledPropertyName, false );
        Assert.That( updatedChildEnabledProperty_AfterUpdate, Is.Not.EqualTo( updatedChildEnabledProperty_BeforeUpdate ) );
        updatedChildEnabledProperty_BeforeUpdate = updatedChildEnabledProperty_AfterUpdate;
        Assert.Multiple( ( ) =>
        {
            Assert.That( updatedChild.Equals( originalChild ), Is.False );
            Assert.That( updatedChild.Enabled.Value, Is.EqualTo( updatedParent.Enabled.Value ) );
            Assert.That( updatedChildEnabledProperty_BeforeUpdate, Is.Not.EqualTo( originalEnabledProperty ) );
        } );

        ZfsProperty<bool> originalTakeSnapshotsProperty = originalChild.TakeSnapshots;
        ZfsProperty<bool> updatedChildTakeSnapshotsProperty_BeforeUpdate = updatedChild.TakeSnapshots;
        ZfsProperty<bool> updatedChildTakeSnapshotsProperty_AfterUpdate = updatedParent.UpdateProperty( ZfsPropertyNames.TakeSnapshotsPropertyName, false );
        Assert.That( updatedChildTakeSnapshotsProperty_AfterUpdate, Is.Not.EqualTo( updatedChildTakeSnapshotsProperty_BeforeUpdate ) );
        updatedChildTakeSnapshotsProperty_BeforeUpdate = updatedChildTakeSnapshotsProperty_AfterUpdate;
        Assert.Multiple( ( ) =>
        {
            Assert.That( updatedChild.Equals( originalChild ), Is.False );
            Assert.That( updatedChild.TakeSnapshots.Value, Is.EqualTo( updatedParent.TakeSnapshots.Value ) );
            Assert.That( updatedChildTakeSnapshotsProperty_BeforeUpdate, Is.Not.EqualTo( originalTakeSnapshotsProperty ) );
        } );

        ZfsProperty<bool> originalPruneSnapshotsProperty = originalChild.PruneSnapshots;
        ZfsProperty<bool> updatedChildPruneSnapshotsProperty_BeforeUpdate = updatedChild.PruneSnapshots;
        ZfsProperty<bool> updatedChildPruneSnapshotsProperty_AfterUpdate = updatedParent.UpdateProperty( ZfsPropertyNames.PruneSnapshotsPropertyName, false );
        Assert.That( updatedChildPruneSnapshotsProperty_AfterUpdate, Is.Not.EqualTo( updatedChildPruneSnapshotsProperty_BeforeUpdate ) );
        updatedChildPruneSnapshotsProperty_BeforeUpdate = updatedChildPruneSnapshotsProperty_AfterUpdate;
        Assert.Multiple( ( ) =>
        {
            Assert.That( updatedChild.Equals( originalChild ), Is.False );
            Assert.That( updatedChild.PruneSnapshots.Value, Is.EqualTo( updatedParent.PruneSnapshots.Value ) );
            Assert.That( updatedChildPruneSnapshotsProperty_BeforeUpdate, Is.Not.EqualTo( originalPruneSnapshotsProperty ) );
        } );
#pragma warning restore NUnit2010 // Use EqualConstraint for better assertion messages in case of failure
    }

    [Test]
    [Category( "General" )]
    [Category( "TypeChecks" )]
    [TestCase( "" )]
    [TestCase( " " )]
    [TestCase( "  " )]
    [TestCase( "\t" )]
    [TestCase( "\n" )]
    [TestCase( "\r" )]
    [TestCase( null )]
    public void CreateChildDataset_ThrowsOnSourceSystemNullEmptyOrWhitespace( string? sourceSystem )
    {
        ZfsRecord gen1Ds = ZfsRecordTestHelpers.GetNewTestRootFileSystem( "gen1" );
        Assert.That( ( ) => { gen1Ds.CreateChildDataset( "badChild", ZfsPropertyValueConstants.FileSystem, sourceSystem ); }, Throws.ArgumentNullException );
    }

    [Test]
    [Category( "General" )]
    [Category( "TypeChecks" )]
    [TestCase( "" )]
    [TestCase( " " )]
    [TestCase( "  " )]
    [TestCase( "\t" )]
    [TestCase( "\n" )]
    [TestCase( "\r" )]
    [TestCase( null )]
    public void CreateSnapshot_ThrowsOnSourceSystemNullEmptyOrWhitespace( string? sourceSystem )
    {
        ZfsRecord gen1Ds = ZfsRecordTestHelpers.GetNewTestRootFileSystem( "gen1" );
        FormattingSettings formattingSettings = FormattingSettings.GetDefault( );
        Assert.That( ( ) => { gen1Ds.CreateSnapshot( SnapshotPeriod.Frequent, DateTimeOffset.Now, in formattingSettings, gen1Ds.SourceSystem with { Value = sourceSystem } ); }, Throws.ArgumentException );
    }

    [Test]
    [TestCase( ZfsPropertyNames.EnabledPropertyName )]
    public void BoolPropertyChangedEventHandler_SubscribersReceiveEventAndUpdateProperties_ChildPropertyIsLocal( string propertyName )
    {
        ZfsRecord gen1Ds = ZfsRecordTestHelpers.GetNewTestRootFileSystem( "gen1" );
        ZfsRecord gen2Ds = gen1Ds.CreateChildDataset( "gen1/gen2", ZfsPropertyValueConstants.FileSystem, "testSystem" );
        ZfsRecord gen3Ds = gen2Ds.CreateChildDataset( "gen1/gen2/gen3", ZfsPropertyValueConstants.FileSystem, "testSystem" );
        ZfsRecord gen4Ds = gen3Ds.CreateChildDataset( "gen1/gen2/gen3/gen4", ZfsPropertyValueConstants.FileSystem, "testSystem" );

        // Now change the property to local on gen 3 by setting it explicitly
        gen3Ds.UpdateProperty( propertyName, true );

        ZfsProperty<bool> gen1Property = GetBoolPropertyByValueFromDataset( gen1Ds, propertyName );
        ZfsProperty<bool> gen2Property = GetBoolPropertyByValueFromDataset( gen2Ds, propertyName );
        ZfsProperty<bool> gen3Property = GetBoolPropertyByValueFromDataset( gen3Ds, propertyName );
        ZfsProperty<bool> gen4Property = GetBoolPropertyByValueFromDataset( gen4Ds, propertyName );

        // Subscribe to the property change event handlers
        bool gen1EventFired = false;
        bool gen2EventFired = false;
        bool gen3EventFired = false;
        bool gen4EventFired = false;
        gen1Ds.BoolPropertyChanged += ( ZfsRecord sender, ref ZfsProperty<bool> property ) => gen1EventFired = true;
        gen2Ds.BoolPropertyChanged += ( ZfsRecord sender, ref ZfsProperty<bool> property ) => gen2EventFired = true;
        gen3Ds.BoolPropertyChanged += ( ZfsRecord sender, ref ZfsProperty<bool> property ) => gen3EventFired = true;
        gen4Ds.BoolPropertyChanged += ( ZfsRecord sender, ref ZfsProperty<bool> property ) => gen4EventFired = true;

        // Ensure our starting conditions are what we expect
        // Gen 1 and 3 properties are local
        // Gen 2 and 4 properties are inherited
        // Gen 2 inherits from gen 1
        // Gen 4 inherits from gen 3
        Assume.That( gen1Property.IsLocal, Is.True );
        Assume.That( gen2Property.IsLocal, Is.False );
        Assume.That( gen2Property.Source, Does.EndWith( gen1Ds.Name ) );
        Assume.That( gen3Property.IsLocal, Is.True );
        Assume.That( gen4Property.IsLocal, Is.False );
        Assume.That( gen4Property.Source, Does.EndWith( gen3Ds.Name ) );

        // Set the property to the opposite of what it was on gen 1 and grab all 4 generations of that property
        ZfsProperty<bool> gen1PropertyAfterChange = gen1Ds.UpdateProperty( propertyName, !gen1Property.Value );
        ZfsProperty<bool> gen2PropertyAfterChange = GetBoolPropertyByValueFromDataset( gen2Ds, propertyName );
        ZfsProperty<bool> gen3PropertyAfterChange = GetBoolPropertyByValueFromDataset( gen3Ds, propertyName );
        ZfsProperty<bool> gen4PropertyAfterChange = GetBoolPropertyByValueFromDataset( gen4Ds, propertyName );

        // Only gen 1 and 2 event handlers should have fired
        Assert.Multiple( ( ) =>
        {
            Assert.That( gen1EventFired, Is.True, "Generation 1 event did not fire." );
            Assert.That( gen2EventFired, Is.True, "Generation 2 event did not fire." );
            Assert.That( gen3EventFired, Is.False, "Generation 3 event fired." );
            Assert.That( gen4EventFired, Is.False, "Generation 4 event fired." );
        } );

        // Values of gen 1 and 2 properties should have changed
        Assert.Multiple( ( ) =>
        {
            Assert.That( gen1PropertyAfterChange.Value, Is.Not.EqualTo( gen1Property.Value ) );
            Assert.That( gen2PropertyAfterChange.Value, Is.Not.EqualTo( gen2Property.Value ) );
            Assert.That( gen3PropertyAfterChange.Value, Is.EqualTo( gen3Property.Value ) );
            Assert.That( gen4PropertyAfterChange.Value, Is.EqualTo( gen4Property.Value ) );
        } );

        // Sources should not have changed
        Assert.Multiple( ( ) =>
        {
            Assert.That( gen1PropertyAfterChange.Source, Is.EqualTo( gen1Property.Source ) );
            Assert.That( gen2PropertyAfterChange.Source, Is.EqualTo( gen2Property.Source ) );
            Assert.That( gen3PropertyAfterChange.Source, Is.EqualTo( gen3Property.Source ) );
            Assert.That( gen4PropertyAfterChange.Source, Is.EqualTo( gen4Property.Source ) );
        } );
    }

    [Test]
    public void DeepCopyClone_NewObjectEqual( )
    {
        ZfsRecord sourceRecord = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        sourceRecord.AddSnapshot( SnapshotTestHelpers.GetStandardTestSnapshotForParent( SnapshotPeriod.Daily, DateTimeOffset.Now, sourceRecord ) );

        ZfsRecord clonedRecord = sourceRecord.DeepCopyClone( );

#pragma warning disable NUnit2010
        Assert.That( clonedRecord.Equals( sourceRecord ), Is.True );
#pragma warning restore NUnit2010
    }

    [Test]
    public void DeepCopyClone_NewObjectNotReferenceToOriginal( )
    {
        ZfsRecord sourceRecord = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        sourceRecord.AddSnapshot( SnapshotTestHelpers.GetStandardTestSnapshotForParent( SnapshotPeriod.Daily, DateTimeOffset.Now, sourceRecord ) );

        ZfsRecord clonedRecord = sourceRecord.DeepCopyClone( );

        Assert.That( ReferenceEquals( clonedRecord, sourceRecord ), Is.False );
    }

    [Test]
    [TestCase( "sameName", ZfsPropertyValueConstants.FileSystem, "sameName", ZfsPropertyValueConstants.FileSystem, ExpectedResult = true )]
    [TestCase( "differentNameA", ZfsPropertyValueConstants.FileSystem, "differentNameB", ZfsPropertyValueConstants.FileSystem, ExpectedResult = false )]
    [TestCase( "sameName", ZfsPropertyValueConstants.FileSystem, "sameName", ZfsPropertyValueConstants.Volume, ExpectedResult = false )]
    [TestCase( "differentNameA", ZfsPropertyValueConstants.FileSystem, "differentNameB", ZfsPropertyValueConstants.Volume, ExpectedResult = false )]
    public bool EqualityIsByRecordValue( string datasetAName, string datasetAKind, string datasetBName, string datasetBKind )
    {
        // This test should be enhanced to check more cases of inequality
        // Providing a test case provider method would help with that
        ZfsRecord datasetA = new( datasetAName, datasetAKind, "testSystem" );
        ZfsRecord datasetB = new( datasetBName, datasetBKind, "testSystem" );
        return datasetA == datasetB;
    }

    [Test]
    public void IntPropertyChangedEventHandler_SubscribersReceiveEventAndUpdateProperties_ChildInherits( )
    {
        ZfsRecord originalParent = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        ZfsRecord originalRecord = originalParent.CreateChildDataset( "testRoot/fs1", ZfsPropertyValueConstants.FileSystem, "testSystem" );
        ZfsRecord updatedParent = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        ZfsRecord updatedRecord = updatedParent.CreateChildDataset( "testRoot/fs1", ZfsPropertyValueConstants.FileSystem, "testSystem" );

#pragma warning disable NUnit2010 // Use EqualConstraint for better assertion messages in case of failure
        Assume.That( updatedRecord.Equals( originalRecord ), Is.True );
        ZfsProperty<bool> originalEnabledProperty = originalRecord.Enabled;
        ZfsProperty<bool> updatedEnabledProperty = updatedParent.UpdateProperty( ZfsPropertyNames.EnabledPropertyName, false );
        Assert.Multiple( ( ) =>
        {
            Assert.That( updatedRecord.Equals( originalRecord ), Is.False );
            Assert.That( updatedRecord.Enabled.Value, Is.EqualTo( updatedParent.Enabled.Value ) );
            Assert.That( updatedEnabledProperty, Is.Not.EqualTo( originalEnabledProperty ) );
        } );
        ZfsProperty<bool> originalTakeSnapshotsProperty = originalRecord.TakeSnapshots;
        ZfsProperty<bool> updatedTakeSnapshotsProperty = updatedParent.UpdateProperty( ZfsPropertyNames.TakeSnapshotsPropertyName, false );
        Assert.Multiple( ( ) =>
        {
            Assert.That( updatedRecord.Equals( originalRecord ), Is.False );
            Assert.That( updatedRecord.Enabled.Value, Is.EqualTo( updatedParent.Enabled.Value ) );
            Assert.That( updatedTakeSnapshotsProperty, Is.Not.EqualTo( originalTakeSnapshotsProperty ) );
        } );
        ZfsProperty<bool> originalPruneSnapshotsProperty = originalRecord.TakeSnapshots;
        ZfsProperty<bool> updatedPruneSnapshotsProperty = updatedParent.UpdateProperty( ZfsPropertyNames.PruneSnapshotsPropertyName, false );
        Assert.Multiple( ( ) =>
        {
            Assert.That( updatedRecord.Equals( originalRecord ), Is.False );
#pragma warning restore NUnit2010 // Use EqualConstraint for better assertion messages in case of failure
            Assert.That( updatedRecord.Enabled.Value, Is.EqualTo( updatedParent.Enabled.Value ) );
            Assert.That( updatedPruneSnapshotsProperty, Is.Not.EqualTo( originalPruneSnapshotsProperty ) );
        } );
    }

    [Test]
    [Combinatorial]
    public void UpdateProperty_Bool( [Values( ZfsPropertyNames.EnabledPropertyName, ZfsPropertyNames.TakeSnapshotsPropertyName, ZfsPropertyNames.PruneSnapshotsPropertyName )] string propertyName, [Values] bool propertyValue, [Values] bool isLocal )
    {
        ZfsRecord originalParent = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        ZfsRecord originalRecord = originalParent.CreateChildDataset( "testRoot/fs1", ZfsPropertyValueConstants.FileSystem, "testSystem" );
        ZfsRecord updatedParent = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        ZfsRecord updatedRecord = updatedParent.CreateChildDataset( "testRoot/fs1", ZfsPropertyValueConstants.FileSystem, "testSystem" );
        ZfsProperty<bool> newTestCaseProperty = new( originalRecord, propertyName, propertyValue, isLocal );
        ZfsProperty<bool> originalBoolProperty = (ZfsProperty<bool>)originalRecord[ propertyName ];

        Assume.That( updatedRecord, Is.EqualTo( originalRecord ), "Both records must be identical for this test to be valid" );
        Assume.That( updatedRecord, Is.Not.SameAs( originalRecord ) );

        ZfsProperty<bool> updatedBoolProperty = updatedRecord.UpdateProperty( propertyName, propertyValue, isLocal );

        if ( newTestCaseProperty == originalBoolProperty )
        {
            Assert.Multiple( ( ) =>
            {
                Assert.That( updatedBoolProperty, Is.EqualTo( originalBoolProperty ) );
                Assert.That( updatedBoolProperty, Is.EqualTo( newTestCaseProperty ) );
                Assert.That( updatedBoolProperty.Name, Is.EqualTo( propertyName ) );
                Assert.That( updatedBoolProperty.Value, Is.EqualTo( propertyValue ) );
                Assert.That( updatedBoolProperty.IsLocal, Is.EqualTo( isLocal ) );

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
                Assert.That( updatedBoolProperty.IsLocal, Is.EqualTo( isLocal ) );

                Assert.That( updatedRecord, Is.Not.EqualTo( originalRecord ) );
            } );
        }

        if ( !isLocal )
        {
            Assert.Multiple( ( ) =>
            {
                Assert.That( updatedRecord[ propertyName ], Has.Property( "IsLocal" ).False );
                Assert.That( updatedRecord[ propertyName ], Has.Property( "Source" ).EqualTo( $"inherited from {updatedParent.Name}" ) );
            } );
        }
    }

    [Test]
    [Combinatorial]
    public void UpdateProperty_DateTimeOffset( [Values( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName )] string propertyName, [ValueSource( nameof( UpdateProperty_DateTimeOffset_Values ) )] string propertyValueString, [Values] bool isLocal )
    {
        DateTimeOffset propertyValue = DateTimeOffset.ParseExact( propertyValueString, "O", DateTimeFormatInfo.CurrentInfo );
        ZfsRecord originalParent = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        ZfsRecord originalRecord = originalParent.CreateChildDataset( "testRoot/fs1", ZfsPropertyValueConstants.FileSystem, "testSystem" );
        ZfsRecord updatedParent = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        ZfsRecord updatedRecord = updatedParent.CreateChildDataset( "testRoot/fs1", ZfsPropertyValueConstants.FileSystem, "testSystem" );
        ZfsProperty<DateTimeOffset> newTestCaseProperty = new( originalRecord, propertyName, propertyValue, isLocal );
        ZfsProperty<DateTimeOffset> originalDateTimeOffsetProperty = (ZfsProperty<DateTimeOffset>)originalRecord[ propertyName ];

        Assume.That( updatedRecord, Is.EqualTo( originalRecord ), "Both records must be identical for this test to be valid" );
        Assume.That( updatedRecord, Is.Not.SameAs( originalRecord ) );

        ZfsProperty<DateTimeOffset> updatedDateTimeOffsetProperty = updatedRecord.UpdateProperty( propertyName, propertyValue, isLocal );

        if ( newTestCaseProperty == originalDateTimeOffsetProperty )
        {
            Assert.Multiple( ( ) =>
            {
                Assert.That( updatedDateTimeOffsetProperty, Is.EqualTo( originalDateTimeOffsetProperty ) );
                Assert.That( updatedDateTimeOffsetProperty, Is.EqualTo( newTestCaseProperty ) );
                Assert.That( updatedDateTimeOffsetProperty.Name, Is.EqualTo( propertyName ) );
                Assert.That( updatedDateTimeOffsetProperty.Value, Is.EqualTo( propertyValue ) );
                Assert.That( updatedDateTimeOffsetProperty.IsLocal, Is.EqualTo( isLocal ) );

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
                Assert.That( updatedDateTimeOffsetProperty.IsLocal, Is.EqualTo( isLocal ) );

                Assert.That( updatedRecord, Is.Not.EqualTo( originalRecord ) );
            } );
        }

        if ( !isLocal )
        {
            Assert.Multiple( ( ) =>
            {
                Assert.That( updatedRecord[ propertyName ], Has.Property( "IsLocal" ).False );
                Assert.That( updatedRecord[ propertyName ], Has.Property( "Source" ).EqualTo( $"inherited from {updatedParent.Name}" ) );
            } );
        }
    }

    [Test]
    [Combinatorial]
    public void UpdateProperty_Int( [Values( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, ZfsPropertyNames.SnapshotRetentionDailyPropertyName, ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName )] string propertyName, [Values( 0, 1, 2, 10, 100 )] int propertyValue, [Values] bool isLocal )
    {
        ZfsRecord originalParent = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        ZfsRecord originalRecord = originalParent.CreateChildDataset( "testRoot/fs1", ZfsPropertyValueConstants.FileSystem, "testSystem" );
        ZfsRecord updatedParent = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        ZfsRecord updatedRecord = updatedParent.CreateChildDataset( "testRoot/fs1", ZfsPropertyValueConstants.FileSystem, "testSystem" );
        ZfsProperty<int> newTestCaseProperty = ZfsProperty<int>.CreateWithoutParent( propertyName, propertyValue, isLocal );
        ZfsProperty<int> originalIntProperty = originalRecord.GetIntProperty( propertyName );

        Assume.That( updatedRecord, Is.EqualTo( originalRecord ), "Both records must be identical for this test to be valid" );
        Assume.That( updatedRecord, Is.Not.SameAs( originalRecord ) );

        ZfsProperty<int> updatedIntProperty = updatedRecord.UpdateProperty( propertyName, propertyValue, isLocal );

        if ( newTestCaseProperty == originalIntProperty )
        {
            Assert.Multiple( ( ) =>
            {
                Assert.That( updatedIntProperty, Is.EqualTo( originalIntProperty ) );
                Assert.That( updatedIntProperty, Is.EqualTo( newTestCaseProperty ) );
                Assert.That( updatedIntProperty.Name, Is.EqualTo( propertyName ) );
                Assert.That( updatedIntProperty.Value, Is.EqualTo( propertyValue ) );
                Assert.That( updatedIntProperty.IsLocal, Is.EqualTo( isLocal ) );

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
                Assert.That( updatedIntProperty.IsLocal, Is.EqualTo( isLocal ) );

                Assert.That( updatedRecord, Is.Not.EqualTo( originalRecord ) );
            } );
        }

        if ( !isLocal )
        {
            Assert.Multiple( ( ) =>
            {
                Assert.That( updatedRecord[ propertyName ], Has.Property( "IsLocal" ).False );
                Assert.That( updatedRecord[ propertyName ], Has.Property( "Source" ).EqualTo( $"inherited from {updatedParent.Name}" ) );
            } );
        }
    }

    [Test]
    [Combinatorial]
    public void UpdateProperty_String( [Values( ZfsPropertyNames.RecursionPropertyName, ZfsPropertyNames.TemplatePropertyName )] string propertyName, [Values( "default", "testTemplate", "template with spaces" )] string propertyValue, [Values] bool isLocal )
    {
        ZfsRecord originalParent = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        ZfsRecord originalRecord = originalParent.CreateChildDataset( "testRoot/fs1", ZfsPropertyValueConstants.FileSystem, "testSystem" );
        ZfsRecord updatedParent = ZfsRecordTestHelpers.GetNewTestRootFileSystem( );
        ZfsRecord updatedRecord = updatedParent.CreateChildDataset( "testRoot/fs1", ZfsPropertyValueConstants.FileSystem, "testSystem" );
        ZfsProperty<string> newTestCaseProperty = ZfsProperty<string>.CreateWithoutParent( propertyName, propertyValue, isLocal );
        ZfsProperty<string> originalStringProperty = (ZfsProperty<string>)originalRecord[ propertyName ];

        Assume.That( updatedRecord, Is.EqualTo( originalRecord ), "Both records must be identical for this test to be valid" );
        Assume.That( updatedRecord, Is.Not.SameAs( originalRecord ) );

        ZfsProperty<string> updatedStringProperty = updatedRecord.UpdateProperty( propertyName, propertyValue, isLocal );

        if ( newTestCaseProperty == originalStringProperty )
        {
            Assert.Multiple( ( ) =>
            {
                Assert.That( updatedStringProperty, Is.EqualTo( originalStringProperty ) );
                Assert.That( updatedStringProperty, Is.EqualTo( newTestCaseProperty ) );
                Assert.That( updatedStringProperty.Name, Is.EqualTo( propertyName ) );
                Assert.That( updatedStringProperty.Value, Is.EqualTo( propertyValue ) );
                Assert.That( updatedStringProperty.IsLocal, Is.EqualTo( isLocal ) );

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
                Assert.That( updatedStringProperty.IsLocal, Is.EqualTo( isLocal ) );

                Assert.That( updatedRecord, Is.Not.EqualTo( originalRecord ) );
            } );
        }

        if ( !isLocal )
        {
            Assert.Multiple( ( ) =>
            {
                Assert.That( updatedRecord[ propertyName ], Has.Property( "IsLocal" ).False );
                Assert.That( updatedRecord[ propertyName ], Has.Property( "Source" ).EqualTo( $"inherited from {updatedParent.Name}" ) );
            } );
        }
    }

    private static ZfsProperty<bool> GetBoolPropertyByValueFromDataset( ZfsRecord dataset, string propertyName )
    {
        return (ZfsProperty<bool>)dataset[ propertyName ];
    }
}
