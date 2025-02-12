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

using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Interop.Zfs.ZfsTypes.Validation;

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes.ZfsRecordTests;

using System.Diagnostics.CodeAnalysis;

[TestFixture]
[TestOf ( typeof (ZfsRecord) )]
[Category ( "General" )]
[Parallelizable ( ParallelScope.Self )]
public class ZfsRecordTests_Constructors
{
    [Test]
    [Category ( "General" )]
    [Category ( "TypeChecks" )]
    [TestCase ( "" )]
    [TestCase ( " " )]
    [TestCase ( "  " )]
    [TestCase ( "\t" )]
    [TestCase ( "\n" )]
    [TestCase ( "\r" )]
    public void Constructor_ThrowsArgumentException_OnSourceSystemEmptyOrWhitespace ( string? sourceSystem )
    {
        Assert.That (
                     ( ) =>
                     {
                         ZfsRecord badRecord = new ( "badRecord", ZfsPropertyValueConstants.FileSystem, sourceSystem );
                     },
                     Throws.ArgumentException );
    }

    [Test]
    public void Constructor_ThrowsArgumentNullException_OnSourceSystemNull ( )
    {
        Assert.That (
                     static ( ) =>
                     {
                         ZfsRecord badRecord = new ( "badRecord", ZfsPropertyValueConstants.FileSystem, null! );
                     },
                     Throws.ArgumentNullException );
    }

    [Test]
    [Description ( "Test of the constructor that can only be used to create pool root FileSystem records" )]
    [Order ( 1 )]
    [NonParallelizable]
    [Category ( "TypeChecks" )]
    [TestCase ( "testRootZfsRecord1", ZfsPropertyValueConstants.FileSystem )]
    [TestCase ( "testRootZfsRecord2", ZfsPropertyValueConstants.FileSystem )]
    public void Constructor_ZfsRecord_Name_Kind_ExpectedPropertiesSet ( string name, string kind )
    {
        ZfsRecord dataset = new ( name, kind, "testSystem" );
        Assert.That ( dataset, Is.Not.Null );
        Assert.That ( dataset, Is.InstanceOf<ZfsRecord> ( ) );

        Assert.Multiple (
                         [SuppressMessage ( "ReSharper", "HeapView.BoxingAllocation" )]
                         ( ) =>
                         {
                             Assert.That ( dataset.Name,                                  Is.EqualTo ( name ) );
                             Assert.That ( dataset.Kind,                                  Is.EqualTo ( kind ) );
                             Assert.That ( dataset.ParentDataset,                         Is.SameAs ( dataset ) );
                             Assert.That ( dataset.IsPoolRoot,                            Is.True );
                             Assert.That ( dataset.NameValidatorRegex,                    Is.SameAs ( ZfsIdentifierRegexes.DatasetNameRegex ( ) ) );
                             Assert.That ( dataset.BytesAvailable,                        Is.Zero );
                             Assert.That ( dataset.BytesUsed,                             Is.Zero );
                             Assert.That ( dataset.Enabled,                               Is.EqualTo ( new ZfsProperty<bool> ( dataset, ZfsPropertyNames.EnabledPropertyName, false ) ).Using<ZfsProperty<bool>> ( ZfsRecordTestHelpers.BoolPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.LastDailySnapshotTimestamp,            Is.EqualTo ( new ZfsProperty<DateTimeOffset> ( dataset, ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName,    DateTimeOffset.UnixEpoch ) ).Using<ZfsProperty<DateTimeOffset>> ( ZfsRecordTestHelpers.DateTimeOffsetPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.LastFrequentSnapshotTimestamp,         Is.EqualTo ( new ZfsProperty<DateTimeOffset> ( dataset, ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ) ).Using<ZfsProperty<DateTimeOffset>> ( ZfsRecordTestHelpers.DateTimeOffsetPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.LastHourlySnapshotTimestamp,           Is.EqualTo ( new ZfsProperty<DateTimeOffset> ( dataset, ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName,   DateTimeOffset.UnixEpoch ) ).Using<ZfsProperty<DateTimeOffset>> ( ZfsRecordTestHelpers.DateTimeOffsetPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.LastMonthlySnapshotTimestamp,          Is.EqualTo ( new ZfsProperty<DateTimeOffset> ( dataset, ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName,  DateTimeOffset.UnixEpoch ) ).Using<ZfsProperty<DateTimeOffset>> ( ZfsRecordTestHelpers.DateTimeOffsetPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.LastObservedDailySnapshotTimestamp,    Is.EqualTo ( DateTimeOffset.UnixEpoch ) );
                             Assert.That ( dataset.LastObservedFrequentSnapshotTimestamp, Is.EqualTo ( DateTimeOffset.UnixEpoch ) );
                             Assert.That ( dataset.LastObservedHourlySnapshotTimestamp,   Is.EqualTo ( DateTimeOffset.UnixEpoch ) );
                             Assert.That ( dataset.LastObservedMonthlySnapshotTimestamp,  Is.EqualTo ( DateTimeOffset.UnixEpoch ) );
                             Assert.That ( dataset.LastObservedWeeklySnapshotTimestamp,   Is.EqualTo ( DateTimeOffset.UnixEpoch ) );
                             Assert.That ( dataset.LastObservedYearlySnapshotTimestamp,   Is.EqualTo ( DateTimeOffset.UnixEpoch ) );
                             Assert.That ( dataset.LastWeeklySnapshotTimestamp,           Is.EqualTo ( new ZfsProperty<DateTimeOffset> ( dataset, ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ) ).Using<ZfsProperty<DateTimeOffset>> ( ZfsRecordTestHelpers.DateTimeOffsetPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.LastYearlySnapshotTimestamp,           Is.EqualTo ( new ZfsProperty<DateTimeOffset> ( dataset, ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ) ).Using<ZfsProperty<DateTimeOffset>> ( ZfsRecordTestHelpers.DateTimeOffsetPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.PruneSnapshots,                        Is.EqualTo ( new ZfsProperty<bool> ( dataset, ZfsPropertyNames.PruneSnapshotsPropertyName, false ) ).Using<ZfsProperty<bool>> ( ZfsRecordTestHelpers.BoolPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.Recursion,                             Is.EqualTo ( new ZfsProperty<string> ( dataset, ZfsPropertyNames.RecursionPropertyName, ZfsPropertyValueConstants.SnapsInAZfs ) ).Using<ZfsProperty<string>> ( ZfsRecordTestHelpers.StringPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.SnapshotRetentionDaily,                Is.EqualTo ( new ZfsProperty<int> ( dataset, ZfsPropertyNames.SnapshotRetentionDailyPropertyName,         -1 ) ).Using<ZfsProperty<int>> ( ZfsRecordTestHelpers.IntPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.SnapshotRetentionFrequent,             Is.EqualTo ( new ZfsProperty<int> ( dataset, ZfsPropertyNames.SnapshotRetentionFrequentPropertyName,      -1 ) ).Using<ZfsProperty<int>> ( ZfsRecordTestHelpers.IntPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.SnapshotRetentionHourly,               Is.EqualTo ( new ZfsProperty<int> ( dataset, ZfsPropertyNames.SnapshotRetentionHourlyPropertyName,        -1 ) ).Using<ZfsProperty<int>> ( ZfsRecordTestHelpers.IntPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.SnapshotRetentionMonthly,              Is.EqualTo ( new ZfsProperty<int> ( dataset, ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName,       -1 ) ).Using<ZfsProperty<int>> ( ZfsRecordTestHelpers.IntPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.SnapshotRetentionPruneDeferral,        Is.EqualTo ( new ZfsProperty<int> ( dataset, ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, 0 ) ).Using<ZfsProperty<int>> ( ZfsRecordTestHelpers.IntPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.SnapshotRetentionWeekly,               Is.EqualTo ( new ZfsProperty<int> ( dataset, ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName,        -1 ) ).Using<ZfsProperty<int>> ( ZfsRecordTestHelpers.IntPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.SnapshotRetentionYearly,               Is.EqualTo ( new ZfsProperty<int> ( dataset, ZfsPropertyNames.SnapshotRetentionYearlyPropertyName,        -1 ) ).Using<ZfsProperty<int>> ( ZfsRecordTestHelpers.IntPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.TakeSnapshots,                         Is.EqualTo ( new ZfsProperty<bool> ( dataset, ZfsPropertyNames.TakeSnapshotsPropertyName, false ) ).Using<ZfsProperty<bool>> ( ZfsRecordTestHelpers.BoolPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.Template,                              Is.EqualTo ( new ZfsProperty<string> ( dataset, ZfsPropertyNames.TemplatePropertyName, ZfsPropertyValueConstants.Default ) ).Using<ZfsProperty<string>> ( ZfsRecordTestHelpers.StringPropertyComparer_Force_op_Equality ) );
                         } );
    }

    [Test]
    [Order ( 2 )]
    [NonParallelizable]
    [Category ( "TypeChecks" )]
    [TestCase ( "testRootZfsRecord1/ChildFs1",  ZfsPropertyValueConstants.FileSystem, "testRootZfsRecord1" )]
    [TestCase ( "testRootZfsRecord1/ChildVol1", ZfsPropertyValueConstants.Volume,     "testRootZfsRecord1" )]
    [TestCase ( "testRootZfsRecord2/ChildFs1",  ZfsPropertyValueConstants.FileSystem, "testRootZfsRecord2" )]
    [TestCase ( "testRootZfsRecord2/ChildVol1", ZfsPropertyValueConstants.Volume,     "testRootZfsRecord2" )]
    public void Constructor_ZfsRecord_Name_Kind_Parent_ExpectedPropertiesSet ( string name, string kind, string parentName )
    {
        ZfsRecord parentDataset = new ( parentName, ZfsPropertyValueConstants.FileSystem, "testSystem" );
        Assume.That ( parentDataset, Is.Not.Null );
        Assume.That ( parentDataset, Is.TypeOf<ZfsRecord> ( ) );
        ZfsRecord dataset = new ( name, kind, "testSystem", true, parentDataset );

        Assert.Multiple (
                         ( ) =>
                         {
                             Assert.That ( dataset, Is.Not.Null );
                             Assert.That ( dataset, Is.InstanceOf<ZfsRecord> ( ) );
                         } );

        Assert.Multiple (
                         [SuppressMessage ( "ReSharper", "HeapView.BoxingAllocation" )]
                         ( ) =>
                         {
                             Assert.That ( dataset.Name,                                  Is.EqualTo ( name ) );
                             Assert.That ( dataset.Kind,                                  Is.EqualTo ( kind ) );
                             Assert.That ( dataset.ParentDataset,                         Is.SameAs ( parentDataset ) );
                             Assert.That ( dataset.IsPoolRoot,                            Is.False );
                             Assert.That ( dataset.NameValidatorRegex,                    Is.SameAs ( ZfsIdentifierRegexes.DatasetNameRegex ( ) ) );
                             Assert.That ( dataset.BytesAvailable,                        Is.Zero );
                             Assert.That ( dataset.BytesUsed,                             Is.Zero );
                             Assert.That ( dataset.Enabled,                               Is.EqualTo ( new ZfsProperty<bool> ( dataset, ZfsPropertyNames.EnabledPropertyName, false, false ) ).Using<ZfsProperty<bool>> ( ZfsRecordTestHelpers.BoolPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.LastDailySnapshotTimestamp,            Is.EqualTo ( new ZfsProperty<DateTimeOffset> ( dataset, ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName,    DateTimeOffset.UnixEpoch ) ).Using<ZfsProperty<DateTimeOffset>> ( ZfsRecordTestHelpers.DateTimeOffsetPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.LastFrequentSnapshotTimestamp,         Is.EqualTo ( new ZfsProperty<DateTimeOffset> ( dataset, ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ) ).Using<ZfsProperty<DateTimeOffset>> ( ZfsRecordTestHelpers.DateTimeOffsetPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.LastHourlySnapshotTimestamp,           Is.EqualTo ( new ZfsProperty<DateTimeOffset> ( dataset, ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName,   DateTimeOffset.UnixEpoch ) ).Using<ZfsProperty<DateTimeOffset>> ( ZfsRecordTestHelpers.DateTimeOffsetPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.LastMonthlySnapshotTimestamp,          Is.EqualTo ( new ZfsProperty<DateTimeOffset> ( dataset, ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName,  DateTimeOffset.UnixEpoch ) ).Using<ZfsProperty<DateTimeOffset>> ( ZfsRecordTestHelpers.DateTimeOffsetPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.LastObservedDailySnapshotTimestamp,    Is.EqualTo ( DateTimeOffset.UnixEpoch ) );
                             Assert.That ( dataset.LastObservedFrequentSnapshotTimestamp, Is.EqualTo ( DateTimeOffset.UnixEpoch ) );
                             Assert.That ( dataset.LastObservedHourlySnapshotTimestamp,   Is.EqualTo ( DateTimeOffset.UnixEpoch ) );
                             Assert.That ( dataset.LastObservedMonthlySnapshotTimestamp,  Is.EqualTo ( DateTimeOffset.UnixEpoch ) );
                             Assert.That ( dataset.LastObservedWeeklySnapshotTimestamp,   Is.EqualTo ( DateTimeOffset.UnixEpoch ) );
                             Assert.That ( dataset.LastObservedYearlySnapshotTimestamp,   Is.EqualTo ( DateTimeOffset.UnixEpoch ) );
                             Assert.That ( dataset.LastWeeklySnapshotTimestamp,           Is.EqualTo ( new ZfsProperty<DateTimeOffset> ( dataset, ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ) ).Using<ZfsProperty<DateTimeOffset>> ( ZfsRecordTestHelpers.DateTimeOffsetPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.LastYearlySnapshotTimestamp,           Is.EqualTo ( new ZfsProperty<DateTimeOffset> ( dataset, ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ) ).Using<ZfsProperty<DateTimeOffset>> ( ZfsRecordTestHelpers.DateTimeOffsetPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.PruneSnapshots,                        Is.EqualTo ( new ZfsProperty<bool> ( dataset, ZfsPropertyNames.PruneSnapshotsPropertyName, false, false ) ).Using<ZfsProperty<bool>> ( ZfsRecordTestHelpers.BoolPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.Recursion,                             Is.EqualTo ( new ZfsProperty<string> ( dataset, ZfsPropertyNames.RecursionPropertyName, ZfsPropertyValueConstants.SnapsInAZfs, false ) ).Using<ZfsProperty<string>> ( ZfsRecordTestHelpers.StringPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.SnapshotRetentionDaily,                Is.EqualTo ( new ZfsProperty<int> ( dataset, ZfsPropertyNames.SnapshotRetentionDailyPropertyName,         -1, false ) ).Using<ZfsProperty<int>> ( ZfsRecordTestHelpers.IntPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.SnapshotRetentionFrequent,             Is.EqualTo ( new ZfsProperty<int> ( dataset, ZfsPropertyNames.SnapshotRetentionFrequentPropertyName,      -1, false ) ).Using<ZfsProperty<int>> ( ZfsRecordTestHelpers.IntPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.SnapshotRetentionHourly,               Is.EqualTo ( new ZfsProperty<int> ( dataset, ZfsPropertyNames.SnapshotRetentionHourlyPropertyName,        -1, false ) ).Using<ZfsProperty<int>> ( ZfsRecordTestHelpers.IntPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.SnapshotRetentionMonthly,              Is.EqualTo ( new ZfsProperty<int> ( dataset, ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName,       -1, false ) ).Using<ZfsProperty<int>> ( ZfsRecordTestHelpers.IntPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.SnapshotRetentionPruneDeferral,        Is.EqualTo ( new ZfsProperty<int> ( dataset, ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, 0,  false ) ).Using<ZfsProperty<int>> ( ZfsRecordTestHelpers.IntPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.SnapshotRetentionWeekly,               Is.EqualTo ( new ZfsProperty<int> ( dataset, ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName,        -1, false ) ).Using<ZfsProperty<int>> ( ZfsRecordTestHelpers.IntPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.SnapshotRetentionYearly,               Is.EqualTo ( new ZfsProperty<int> ( dataset, ZfsPropertyNames.SnapshotRetentionYearlyPropertyName,        -1, false ) ).Using<ZfsProperty<int>> ( ZfsRecordTestHelpers.IntPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.TakeSnapshots,                         Is.EqualTo ( new ZfsProperty<bool> ( dataset, ZfsPropertyNames.TakeSnapshotsPropertyName, false, false ) ).Using<ZfsProperty<bool>> ( ZfsRecordTestHelpers.BoolPropertyComparer_Force_op_Equality ) );
                             Assert.That ( dataset.Template,                              Is.EqualTo ( new ZfsProperty<string> ( dataset, ZfsPropertyNames.TemplatePropertyName, ZfsPropertyValueConstants.Default, false ) ).Using<ZfsProperty<string>> ( ZfsRecordTestHelpers.StringPropertyComparer_Force_op_Equality ) );
                         } );
    }
}
