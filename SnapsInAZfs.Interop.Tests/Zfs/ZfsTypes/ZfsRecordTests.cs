// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes;

[TestFixture]
[Category( "General" )]
[FixtureLifeCycle( LifeCycle.SingleInstance )]
[Order( 20 )]
[TestOf( typeof( ZfsRecord ) )]
public class ZfsRecordTests
{
    [Test]
    public void AddSnapshot_LastObservedTimestampsUpdated( )
    {
        Assert.Ignore( "Test not implemented" );
    }

    [Test]
    [TestCase( SnapshotPeriodKind.Frequent )]
    [TestCase( SnapshotPeriodKind.Hourly )]
    [TestCase( SnapshotPeriodKind.Daily )]
    [TestCase( SnapshotPeriodKind.Weekly )]
    [TestCase( SnapshotPeriodKind.Monthly )]
    [TestCase( SnapshotPeriodKind.Yearly )]
    public void AddSnapshot_SnapshotsInCorrectCollection( SnapshotPeriodKind period )
    {
        ZfsRecord dataset = new( "dsName", ZfsPropertyValueConstants.FileSystem );
        Snapshot snapshot = new( period.ToString( "G" ), true, period, DateTimeOffset.UnixEpoch, dataset );
        dataset.AddSnapshot( snapshot );
        Assert.That( dataset.Snapshots, Contains.Key( period ) );
        Assert.That( dataset.Snapshots[ period ], Contains.Key( snapshot.Name ) );
        Assert.That( dataset.Snapshots[ period ], Contains.Value( snapshot ) );
    }

    [Test]
    [TestCaseSource( typeof( ZfsRecordTestData ), nameof( ZfsRecordTestData.GetValidDatasetCases ), new object?[] { 8, 12, 5 } )]
    [TestCaseSource( typeof( ZfsRecordTestData ), nameof( ZfsRecordTestData.GetIllegalDatasetCases ), new object?[] { 8, 12, 5 } )]
    [Category( "General" )]
    [Category( "ZFS" )]
    public void CheckDatasetNameValidation( NameValidationTestCase testCase )
    {
        if ( testCase.Name.Length >= 255 )
        {
            Assert.Ignore( "Total path depth requested would exceed valid ZFS identifier length (255)" );
        }

        string nameToTest = testCase.Name;
        bool fsValidationResult = ZfsRecord.ValidateName( ZfsPropertyValueConstants.FileSystem, nameToTest ) == testCase.Valid;
        bool volValidationResult = ZfsRecord.ValidateName( ZfsPropertyValueConstants.Volume, nameToTest ) == testCase.Valid;
        Assert.Multiple( ( ) =>
        {
            Assert.That( fsValidationResult, Is.True );
            Assert.That( volValidationResult, Is.True );
        } );
    }

    [Test]
    [Category( "General" )]
    [Category( "ZFS" )]
    [Category( "TypeChecks" )]
    public void CheckNameValidationThrowsOnNameTooLong( [Values( 256, 512 )] int pathLength, [Range( 1, 5 )] int pathDepth )
    {
        string path = $"{ZfsRecordTestData.GetValidZfsPathComponent( pathLength / pathDepth )}";
        for ( int i = 1; i < pathDepth; i++ )
        {
            path = $"{path}/{ZfsRecordTestData.GetValidZfsPathComponent( pathLength / pathDepth )}";
        }

        Assert.That( ( ) => { ZfsRecord.ValidateName( ZfsPropertyValueConstants.FileSystem, path ); }, Throws.TypeOf<ArgumentOutOfRangeException>( ) );
    }

    [Test]
    [TestCaseSource( typeof( ZfsRecordTestData ), nameof( ZfsRecordTestData.GetValidSnapshotCases ), new object?[] { 8, 12, 5 } )]
    [TestCaseSource( typeof( ZfsRecordTestData ), nameof( ZfsRecordTestData.GetIllegalSnapshotCases ), new object?[] { 8, 12, 5 } )]
    [Category( "General" )]
    [Category( "ZFS" )]
    public void CheckSnapshotNameValidation( NameValidationTestCase testCase )
    {
        if ( testCase.Name.Length >= 255 )
        {
            Assert.Ignore( "Total path depth requested would exceed valid ZFS identifier length (255)" );
        }

        string nameToTest = testCase.Name;
        bool snapshotValidationResult = ZfsRecord.ValidateName( ZfsPropertyValueConstants.Snapshot, nameToTest ) == testCase.Valid;
        Assert.That( snapshotValidationResult, Is.True );
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
        ZfsRecord datasetA = new( datasetAName, datasetAKind );
        ZfsRecord datasetB = new( datasetBName, datasetBKind );
        return datasetA == datasetB;
    }

    [Test]
    public void GetSnapshotsToPrune( )
    {
        Assert.Ignore( "Test not implemented" );
    }

    [Test]
    public void IsDailySnapshotNeeded( )
    {
        Assert.Ignore( "Test not implemented" );
    }

    [Test]
    [TestCaseSource( typeof( ZfsRecordTestData ), nameof( ZfsRecordTestData.IsSnapshotNeededTestCases ) )]
    public void IsFrequentSnapshotNeeded( IsSnapshotNeededTestCase testCase )
    {
        ZfsRecord dataset = testCase.Dataset;
        bool isFrequentSnapshotNeeded = dataset.IsFrequentSnapshotNeeded( ZfsRecordTestData.Settings.Templates[ dataset.Template.Value ].SnapshotTiming, testCase.Timestamp );
        Assert.That( isFrequentSnapshotNeeded, Is.EqualTo( testCase.IsSnapshotNeededExpected ) );
    }

    [Test]
    public void IsHourlySnapshotNeeded( )
    {
        Assert.Ignore( "Test not implemented" );
    }

    [Test]
    public void IsMonthlySnapshotNeeded( )
    {
        Assert.Ignore( "Test not implemented" );
    }

    [Test]
    public void IsWeeklySnapshotNeeded( )
    {
        Assert.Ignore( "Test not implemented" );
    }

    [Test]
    public void IsYearlySnapshotNeeded( )
    {
        Assert.Ignore( "Test not implemented" );
    }

    [Test]
    [TestCaseSource( typeof( ZfsRecordTestData ), nameof( ZfsRecordTestData.UpdatePropertyTestCases ) )]
    public void UpdateProperty_FileSystems( UpdatePropertyTestCase testCase )
    {
        ZfsRecord originalRecord = ZfsRecordTestData.StandardValidTestFileSystem with { };
        ZfsRecord updatedRecord = ZfsRecordTestData.StandardValidTestFileSystem with { };
        Assert.That( updatedRecord, Is.EqualTo( originalRecord ) );
        IZfsProperty updatedProperty = updatedRecord.UpdateProperty( testCase.PropertyToChange.Name, testCase.PropertyToChange.ValueString, testCase.PropertyToChange.Source );
        Assert.Multiple( ( ) =>
        {
            switch ( testCase.PropertyToChange )
            {
                case ZfsProperty<int>:
                    ZfsProperty<int> originalIntProperty = (ZfsProperty<int>)originalRecord[ testCase.PropertyToChange.Name ];
                    ZfsProperty<int> updatedIntProperty = (ZfsProperty<int>)updatedRecord[ testCase.PropertyToChange.Name ];
                    ZfsProperty<int> testCaseIntProperty = (ZfsProperty<int>)updatedRecord[ testCase.PropertyToChange.Name ];
                    Assert.That( updatedIntProperty.Value, Is.Not.EqualTo( originalIntProperty.Value ) );
                    Assert.That( updatedIntProperty.Value, Is.EqualTo( testCaseIntProperty.Value ) );
                    goto default;
                case ZfsProperty<string>:
                    ZfsProperty<string> originalStringProperty = (ZfsProperty<string>)originalRecord[ testCase.PropertyToChange.Name ];
                    ZfsProperty<string> updatedStringProperty = (ZfsProperty<string>)updatedRecord[ testCase.PropertyToChange.Name ];
                    ZfsProperty<string> testCaseStringProperty = (ZfsProperty<string>)updatedRecord[ testCase.PropertyToChange.Name ];
                    Assert.That( updatedStringProperty.Value, Is.Not.EqualTo( originalStringProperty.Value ) );
                    Assert.That( updatedStringProperty.Value, Is.EqualTo( testCaseStringProperty.Value ) );
                    goto default;
                case ZfsProperty<bool>:
                    ZfsProperty<bool> originalBoolProperty = (ZfsProperty<bool>)originalRecord[ testCase.PropertyToChange.Name ];
                    ZfsProperty<bool> updatedBoolProperty = (ZfsProperty<bool>)updatedRecord[ testCase.PropertyToChange.Name ];
                    ZfsProperty<bool> testCaseBoolProperty = (ZfsProperty<bool>)updatedRecord[ testCase.PropertyToChange.Name ];
                    Assert.That( updatedBoolProperty.Value, Is.Not.EqualTo( originalBoolProperty.Value ) );
                    Assert.That( updatedBoolProperty.Value, Is.EqualTo( testCaseBoolProperty.Value ) );
                    goto default;
                case ZfsProperty<DateTimeOffset>:
                    ZfsProperty<DateTimeOffset> originalDateTimeOffsetProperty = (ZfsProperty<DateTimeOffset>)originalRecord[ testCase.PropertyToChange.Name ];
                    ZfsProperty<DateTimeOffset> updatedDateTimeOffsetProperty = (ZfsProperty<DateTimeOffset>)updatedRecord[ testCase.PropertyToChange.Name ];
                    ZfsProperty<DateTimeOffset> testCaseDateTimeOffsetProperty = (ZfsProperty<DateTimeOffset>)updatedRecord[ testCase.PropertyToChange.Name ];
                    Assert.That( updatedDateTimeOffsetProperty.Value, Is.Not.EqualTo( originalDateTimeOffsetProperty.Value ) );
                    Assert.That( updatedDateTimeOffsetProperty.Value, Is.EqualTo( testCaseDateTimeOffsetProperty.Value ) );
                    goto default;
                default:
                    Assert.That( updatedProperty, Is.EqualTo( testCase.PropertyToChange ) );
                    Assert.That( updatedRecord, Is.Not.EqualTo( originalRecord ) );
                    break;
            }
        } );
    }

    [Test]
    [TestCaseSource( typeof( ZfsRecordTestData ), nameof( ZfsRecordTestData.UpdatePropertyTestCases ) )]
    public void UpdateProperty_Volumes( UpdatePropertyTestCase testCase )
    {
        ZfsRecord originalRecord = ZfsRecordTestData.StandardValidTestVolume with { };
        ZfsRecord updatedRecord = ZfsRecordTestData.StandardValidTestVolume with { };
        Assert.That( updatedRecord, Is.EqualTo( originalRecord ) );
        IZfsProperty updatedProperty = updatedRecord.UpdateProperty( testCase.PropertyToChange.Name, testCase.PropertyToChange.ValueString, testCase.PropertyToChange.Source );
        Assert.Multiple( ( ) =>
        {
            switch ( testCase.PropertyToChange )
            {
                case ZfsProperty<int>:
                    ZfsProperty<int> originalIntProperty = (ZfsProperty<int>)originalRecord[ testCase.PropertyToChange.Name ];
                    ZfsProperty<int> updatedIntProperty = (ZfsProperty<int>)updatedRecord[ testCase.PropertyToChange.Name ];
                    ZfsProperty<int> testCaseIntProperty = (ZfsProperty<int>)updatedRecord[ testCase.PropertyToChange.Name ];
                    Assert.That( updatedIntProperty.Value, Is.Not.EqualTo( originalIntProperty.Value ) );
                    Assert.That( updatedIntProperty.Value, Is.EqualTo( testCaseIntProperty.Value ) );
                    goto default;
                case ZfsProperty<string>:
                    ZfsProperty<string> originalStringProperty = (ZfsProperty<string>)originalRecord[ testCase.PropertyToChange.Name ];
                    ZfsProperty<string> updatedStringProperty = (ZfsProperty<string>)updatedRecord[ testCase.PropertyToChange.Name ];
                    ZfsProperty<string> testCaseStringProperty = (ZfsProperty<string>)updatedRecord[ testCase.PropertyToChange.Name ];
                    Assert.That( updatedStringProperty.Value, Is.Not.EqualTo( originalStringProperty.Value ) );
                    Assert.That( updatedStringProperty.Value, Is.EqualTo( testCaseStringProperty.Value ) );
                    goto default;
                case ZfsProperty<bool>:
                    ZfsProperty<bool> originalBoolProperty = (ZfsProperty<bool>)originalRecord[ testCase.PropertyToChange.Name ];
                    ZfsProperty<bool> updatedBoolProperty = (ZfsProperty<bool>)updatedRecord[ testCase.PropertyToChange.Name ];
                    ZfsProperty<bool> testCaseBoolProperty = (ZfsProperty<bool>)updatedRecord[ testCase.PropertyToChange.Name ];
                    Assert.That( updatedBoolProperty.Value, Is.Not.EqualTo( originalBoolProperty.Value ) );
                    Assert.That( updatedBoolProperty.Value, Is.EqualTo( testCaseBoolProperty.Value ) );
                    goto default;
                case ZfsProperty<DateTimeOffset>:
                    ZfsProperty<DateTimeOffset> originalDateTimeOffsetProperty = (ZfsProperty<DateTimeOffset>)originalRecord[ testCase.PropertyToChange.Name ];
                    ZfsProperty<DateTimeOffset> updatedDateTimeOffsetProperty = (ZfsProperty<DateTimeOffset>)updatedRecord[ testCase.PropertyToChange.Name ];
                    ZfsProperty<DateTimeOffset> testCaseDateTimeOffsetProperty = (ZfsProperty<DateTimeOffset>)updatedRecord[ testCase.PropertyToChange.Name ];
                    Assert.That( updatedDateTimeOffsetProperty.Value, Is.Not.EqualTo( originalDateTimeOffsetProperty.Value ) );
                    Assert.That( updatedDateTimeOffsetProperty.Value, Is.EqualTo( testCaseDateTimeOffsetProperty.Value ) );
                    goto default;
                default:
                    Assert.That( updatedProperty, Is.EqualTo( testCase.PropertyToChange ) );
                    Assert.That( updatedRecord, Is.Not.EqualTo( originalRecord ) );
                    break;
            }
        } );
    }
}

public record struct UpdatePropertyTestCase( string TestName, IZfsProperty PropertyToChange );

public record struct NameValidationTestCase( string Name, bool Valid );
