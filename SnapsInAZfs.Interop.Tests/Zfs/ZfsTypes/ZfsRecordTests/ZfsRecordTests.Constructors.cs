// LICENSE:
// 
// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

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
    [Category( "General" )]
    [Category( "TypeChecks" )]
    [TestCase( "" )]
    [TestCase( " " )]
    [TestCase( "  " )]
    [TestCase( "\t" )]
    [TestCase( "\n" )]
    [TestCase( "\r" )]
    [TestCase( null )]
    public void Constructor_ThrowsOnSourceSystemNullEmptyOrWhitespace( string sourceSystem )
    {
        Assert.That( ( ) =>
        {
            ZfsRecord badRecord = new( "badRecord", ZfsPropertyValueConstants.FileSystem, sourceSystem );
        }, Throws.ArgumentNullException );
    }

    [Test]
    [Description( "Test of the constructor that can only be used to create pool root FileSystem records" )]
    [Order( 1 )]
    [NonParallelizable]
    [Category( "TypeChecks" )]
    [TestCase( "testRootZfsRecord1", ZfsPropertyValueConstants.FileSystem )]
    [TestCase( "testRootZfsRecord2", ZfsPropertyValueConstants.FileSystem )]
    public void Constructor_ZfsRecord_Name_Kind_ExpectedPropertiesSet( string name, string kind )
    {
        ZfsRecord dataset = new( name, kind, "testSystem" );
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
        ZfsRecord parentDataset = new( parentName, ZfsPropertyValueConstants.FileSystem, "testSystem" );
        Assume.That( parentDataset, Is.Not.Null );
        Assume.That( parentDataset, Is.TypeOf<ZfsRecord>( ) );
        ZfsRecord dataset = new( name, kind, "testSystem", true, parentDataset );
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
#pragma warning disable NUnit2010 // Use EqualConstraint for better assertion messages in case of failure
            Assert.That( dataset.Enabled.Equals( new ZfsProperty<bool>( dataset, ZfsPropertyNames.EnabledPropertyName, false, false ) ), Is.True );
#pragma warning restore NUnit2010 // Use EqualConstraint for better assertion messages in case of failure
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
