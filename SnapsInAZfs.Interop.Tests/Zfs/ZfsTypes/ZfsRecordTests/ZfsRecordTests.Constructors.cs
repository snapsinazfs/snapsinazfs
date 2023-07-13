// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Interop.Zfs.ZfsTypes.Validation;

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes.ZfsRecordTests;

[TestFixture]
[TestOf( typeof( ZfsRecord ) )]
[Category( "General" )]
[Parallelizable( ParallelScope.Self )]
public class ZfsRecordTests_Constructors
{
    [Test]
    [Description( "Test of the constructor that can only be used to create pool root FileSystem records" )]
    [Order( 1 )]
    [NonParallelizable]
    [Category( "TypeChecks" )]
    [TestCase( "testRootZfsRecord1", ZfsPropertyValueConstants.FileSystem )]
    [TestCase( "testRootZfsRecord2", ZfsPropertyValueConstants.FileSystem )]
    public void Constructor_ZfsRecord_Name_Kind_ExpectedPropertiesSet( string name, string kind )
    {
        ZfsRecord dataset = new( name, kind );
        Assert.That( dataset, Is.Not.Null );
        Assert.That( dataset, Is.InstanceOf<ZfsRecord>( ) );
        Assert.Multiple( ( ) =>
        {
            Assert.That( dataset.Name, Is.EqualTo( name ) );
            Assert.That( dataset.Kind, Is.EqualTo( kind ) );
            Assert.That( dataset.ParentDataset, Is.SameAs( dataset ) );
            Assert.That( dataset.IsPoolRoot, Is.True );
            Assert.That( dataset.NameValidatorRegex, Is.SameAs( ZfsIdentifierRegexes.DatasetNameRegex( ) ) );
            Assert.That( dataset.BytesAvailable, Is.Zero );
            Assert.That( dataset.BytesUsed, Is.Zero );
            Assert.That( dataset.Enabled, Is.EqualTo( new ZfsProperty<bool>( dataset, ZfsPropertyNames.EnabledPropertyName, false ) ) );
            Assert.That( dataset.LastDailySnapshotTimestamp, Is.EqualTo( new ZfsProperty<DateTimeOffset>( dataset, ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ) ) );
            Assert.That( dataset.LastFrequentSnapshotTimestamp, Is.EqualTo( new ZfsProperty<DateTimeOffset>( dataset, ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ) ) );
            Assert.That( dataset.LastHourlySnapshotTimestamp, Is.EqualTo( new ZfsProperty<DateTimeOffset>( dataset, ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ) ) );
            Assert.That( dataset.LastMonthlySnapshotTimestamp, Is.EqualTo( new ZfsProperty<DateTimeOffset>( dataset, ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ) ) );
            Assert.That( dataset.LastObservedDailySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
            Assert.That( dataset.LastObservedFrequentSnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
            Assert.That( dataset.LastObservedHourlySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
            Assert.That( dataset.LastObservedMonthlySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
            Assert.That( dataset.LastObservedWeeklySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
            Assert.That( dataset.LastObservedYearlySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
            Assert.That( dataset.LastWeeklySnapshotTimestamp, Is.EqualTo( new ZfsProperty<DateTimeOffset>( dataset, ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ) ) );
            Assert.That( dataset.LastYearlySnapshotTimestamp, Is.EqualTo( new ZfsProperty<DateTimeOffset>( dataset, ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ) ) );
            Assert.That( dataset.PruneSnapshots, Is.EqualTo( new ZfsProperty<bool>( dataset, ZfsPropertyNames.PruneSnapshotsPropertyName, false ) ) );
            Assert.That( dataset.Recursion, Is.EqualTo( new ZfsProperty<string>( dataset, ZfsPropertyNames.RecursionPropertyName, ZfsPropertyValueConstants.SnapsInAZfs ) ) );
            Assert.That( dataset.SnapshotRetentionDaily, Is.EqualTo( new ZfsProperty<int>( dataset, ZfsPropertyNames.SnapshotRetentionDailyPropertyName, -1 ) ) );
            Assert.That( dataset.SnapshotRetentionFrequent, Is.EqualTo( new ZfsProperty<int>( dataset, ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, -1 ) ) );
            Assert.That( dataset.SnapshotRetentionHourly, Is.EqualTo( new ZfsProperty<int>( dataset, ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, -1 ) ) );
            Assert.That( dataset.SnapshotRetentionMonthly, Is.EqualTo( new ZfsProperty<int>( dataset, ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, -1 ) ) );
            Assert.That( dataset.SnapshotRetentionPruneDeferral, Is.EqualTo( new ZfsProperty<int>( dataset, ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, 0 ) ) );
            Assert.That( dataset.SnapshotRetentionWeekly, Is.EqualTo( new ZfsProperty<int>( dataset, ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, -1 ) ) );
            Assert.That( dataset.SnapshotRetentionYearly, Is.EqualTo( new ZfsProperty<int>( dataset, ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, -1 ) ) );
            Assert.That( dataset.TakeSnapshots, Is.EqualTo( new ZfsProperty<bool>( dataset, ZfsPropertyNames.TakeSnapshotsPropertyName, false ) ) );
            Assert.That( dataset.Template, Is.EqualTo( new ZfsProperty<string>( dataset, ZfsPropertyNames.TemplatePropertyName, ZfsPropertyValueConstants.Default ) ) );
        } );
    }

    [Test]
    [Order( 2 )]
    [NonParallelizable]
    [Category( "TypeChecks" )]
    [TestCase( "testRootZfsRecord1/ChildFs1", ZfsPropertyValueConstants.FileSystem, "testRootZfsRecord1" )]
    [TestCase( "testRootZfsRecord1/ChildVol1", ZfsPropertyValueConstants.Volume, "testRootZfsRecord1" )]
    [TestCase( "testRootZfsRecord2/ChildFs1", ZfsPropertyValueConstants.FileSystem, "testRootZfsRecord2" )]
    [TestCase( "testRootZfsRecord2/ChildVol1", ZfsPropertyValueConstants.Volume, "testRootZfsRecord2" )]
    public void Constructor_ZfsRecord_Name_Kind_Parent_ExpectedPropertiesSet( string name, string kind, string parentName )
    {
        ZfsRecord parentDataset = new( parentName, ZfsPropertyValueConstants.FileSystem );
        Assume.That( parentDataset, Is.Not.Null );
        Assume.That( parentDataset, Is.TypeOf<ZfsRecord>( ) );
        ZfsRecord dataset = new( name, kind, true, parentDataset );
        Assert.Multiple( ( ) =>
        {
            Assert.That( dataset, Is.Not.Null );
            Assert.That( dataset, Is.InstanceOf<ZfsRecord>( ) );
        } );
        Assert.Multiple( ( ) =>
        {
            Assert.That( dataset.Name, Is.EqualTo( name ) );
            Assert.That( dataset.Kind, Is.EqualTo( kind ) );
            Assert.That( dataset.ParentDataset, Is.SameAs( parentDataset ) );
            Assert.That( dataset.IsPoolRoot, Is.False );
            Assert.That( dataset.NameValidatorRegex, Is.SameAs( ZfsIdentifierRegexes.DatasetNameRegex( ) ) );
            Assert.That( dataset.BytesAvailable, Is.Zero );
            Assert.That( dataset.BytesUsed, Is.Zero );
            Assert.That( dataset.Enabled.Equals( new ZfsProperty<bool>( dataset, ZfsPropertyNames.EnabledPropertyName, false, false ) ), Is.True );
            Assert.That( dataset.LastDailySnapshotTimestamp, Is.EqualTo( new ZfsProperty<DateTimeOffset>( dataset, ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ) ) );
            Assert.That( dataset.LastFrequentSnapshotTimestamp, Is.EqualTo( new ZfsProperty<DateTimeOffset>( dataset, ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ) ) );
            Assert.That( dataset.LastHourlySnapshotTimestamp, Is.EqualTo( new ZfsProperty<DateTimeOffset>( dataset, ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ) ) );
            Assert.That( dataset.LastMonthlySnapshotTimestamp, Is.EqualTo( new ZfsProperty<DateTimeOffset>( dataset, ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ) ) );
            Assert.That( dataset.LastObservedDailySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
            Assert.That( dataset.LastObservedFrequentSnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
            Assert.That( dataset.LastObservedHourlySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
            Assert.That( dataset.LastObservedMonthlySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
            Assert.That( dataset.LastObservedWeeklySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
            Assert.That( dataset.LastObservedYearlySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
            Assert.That( dataset.LastWeeklySnapshotTimestamp, Is.EqualTo( new ZfsProperty<DateTimeOffset>( dataset, ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ) ) );
            Assert.That( dataset.LastYearlySnapshotTimestamp, Is.EqualTo( new ZfsProperty<DateTimeOffset>( dataset, ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ) ) );
            Assert.That( dataset.PruneSnapshots, Is.EqualTo( new ZfsProperty<bool>( dataset, ZfsPropertyNames.PruneSnapshotsPropertyName, false, false ) ) );
            Assert.That( dataset.Recursion, Is.EqualTo( new ZfsProperty<string>( dataset, ZfsPropertyNames.RecursionPropertyName, ZfsPropertyValueConstants.SnapsInAZfs, false ) ) );
            Assert.That( dataset.SnapshotRetentionDaily, Is.EqualTo( new ZfsProperty<int>( dataset, ZfsPropertyNames.SnapshotRetentionDailyPropertyName, -1, false ) ) );
            Assert.That( dataset.SnapshotRetentionFrequent, Is.EqualTo( new ZfsProperty<int>( dataset, ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, -1, false ) ) );
            Assert.That( dataset.SnapshotRetentionHourly, Is.EqualTo( new ZfsProperty<int>( dataset, ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, -1, false ) ) );
            Assert.That( dataset.SnapshotRetentionMonthly, Is.EqualTo( new ZfsProperty<int>( dataset, ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, -1, false ) ) );
            Assert.That( dataset.SnapshotRetentionPruneDeferral, Is.EqualTo( new ZfsProperty<int>( dataset, ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, 0, false ) ) );
            Assert.That( dataset.SnapshotRetentionWeekly, Is.EqualTo( new ZfsProperty<int>( dataset, ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, -1, false ) ) );
            Assert.That( dataset.SnapshotRetentionYearly, Is.EqualTo( new ZfsProperty<int>( dataset, ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, -1, false ) ) );
            Assert.That( dataset.TakeSnapshots, Is.EqualTo( new ZfsProperty<bool>( dataset, ZfsPropertyNames.TakeSnapshotsPropertyName, false, false ) ) );
            Assert.That( dataset.Template, Is.EqualTo( new ZfsProperty<string>( dataset, ZfsPropertyNames.TemplatePropertyName, ZfsPropertyValueConstants.Default, false ) ) );
        } );
    }
}
