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

using System.Collections.Concurrent;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsCommandRunner;

[TestFixture]
[TestOf ( typeof( RawZfsObject ) )]
[Category ( "General" )]
public class RawZfsObjectTests
{
    [Test]
    [TestCase ( "available",                                                       ZfsPropertySourceConstants.None,  0 )]
    [TestCase ( ZfsPropertyNames.Enabled,                              ZfsPropertySourceConstants.Local, 1 )]
    [TestCase ( ZfsPropertyNames.DatasetLastDailySnapshotTimestamp,    ZfsPropertySourceConstants.Local, 2 )]
    [TestCase ( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestamp, ZfsPropertySourceConstants.Local, 3 )]
    [TestCase ( ZfsPropertyNames.DatasetLastHourlySnapshotTimestamp,   ZfsPropertySourceConstants.Local, 4 )]
    [TestCase ( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestamp,  ZfsPropertySourceConstants.Local, 5 )]
    [TestCase ( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestamp,   ZfsPropertySourceConstants.Local, 6 )]
    [TestCase ( ZfsPropertyNames.DatasetLastYearlySnapshotTimestamp,   ZfsPropertySourceConstants.Local, 7 )]
    [TestCase ( ZfsPropertyNames.PruneSnapshots,                       ZfsPropertySourceConstants.Local, 8 )]
    [TestCase ( ZfsPropertyNames.Recursion,                            ZfsPropertySourceConstants.Local, 9 )]
    [TestCase ( ZfsPropertyNames.SnapshotRetentionDaily,               ZfsPropertySourceConstants.Local, 10 )]
    [TestCase ( ZfsPropertyNames.SnapshotRetentionFrequent,            ZfsPropertySourceConstants.Local, 11 )]
    [TestCase ( ZfsPropertyNames.SnapshotRetentionHourly,              ZfsPropertySourceConstants.Local, 12 )]
    [TestCase ( ZfsPropertyNames.SnapshotRetentionMonthly,             ZfsPropertySourceConstants.Local, 13 )]
    [TestCase ( ZfsPropertyNames.SnapshotRetentionPruneDeferral,       ZfsPropertySourceConstants.Local, 14 )]
    [TestCase ( ZfsPropertyNames.SnapshotRetentionWeekly,              ZfsPropertySourceConstants.Local, 15 )]
    [TestCase ( ZfsPropertyNames.SnapshotRetentionYearly,              ZfsPropertySourceConstants.Local, 16 )]
    [TestCase ( ZfsPropertyNames.SnapshotPeriod,                       ZfsPropertySourceConstants.Local, 17 )]
    [TestCase ( ZfsPropertyNames.SnapshotTimestamp,                    ZfsPropertySourceConstants.Local, 18 )]
    [TestCase ( ZfsPropertyNames.SourceSystem,                                     ZfsPropertySourceConstants.Local, 19 )]
    [TestCase ( ZfsPropertyNames.TakeSnapshots,                        ZfsPropertySourceConstants.Local, 20 )]
    [TestCase ( ZfsPropertyNames.Template,                             ZfsPropertySourceConstants.Local, 21 )]
    [TestCase ( "type",                                                            ZfsPropertySourceConstants.None,  22 )]
    [TestCase ( "used",                                                            ZfsPropertySourceConstants.None,  23 )]
    public void AddRawProperty_AllPropertiesCorrectAndInOrder( string propertyName, string propertySource, int expectedIndex )
    {
        RawZfsObject   obj                      = new ( ZfsPropertyValueConstants.FileSystem );
        TestCaseData[] rawPropertyTestCaseArray = GetAddRawProperty_TestCases( );
        foreach ( TestCaseData p in rawPropertyTestCaseArray )
        {
            obj.AddRawProperty ( p.Arguments [ 0 ]!.ToString( )!, p.Arguments [ 1 ]!.ToString( )!, p.Arguments [ 2 ]!.ToString( )! );
        }

        Assert.Multiple ( ( ) =>
                          {
                              Assert.That ( obj.Properties,                             Contains.Key ( propertyName ) );
                              Assert.That ( obj.Properties,                             Has.ItemAt ( propertyName ).InstanceOf<RawProperty>( ) );
                              Assert.That ( obj.Properties.IndexOfKey ( propertyName ), Is.EqualTo ( expectedIndex ) );
                              Assert.That ( obj.Properties [ propertyName ].Name,       Is.EqualTo ( propertyName ) );
                              Assert.That ( obj.Properties [ propertyName ].Value,      Is.EqualTo ( rawPropertyTestCaseArray.Single ( rp => rp?.Arguments [ 0 ] is string pn && pn == propertyName ).Arguments [ 1 ] ) );
                              Assert.That ( obj.Properties [ propertyName ].Source,     Is.EqualTo ( propertySource ) );
                          }
                        );
    }

    [Test]
    [TestCase ( ZfsPropertyValueConstants.FileSystem )]
    [TestCase ( ZfsPropertyValueConstants.Volume )]
    [TestCase ( ZfsPropertyValueConstants.Snapshot )]
    public void Constructor_KindCorrect( string kind )
    {
        RawZfsObject obj = new ( kind );
        Assert.That ( obj, Has.Property ( "Kind" ).EqualTo ( kind ) );
    }

    [Test]
    public void Constructor_PropertiesCollectionEmpty ( )
    {
        RawZfsObject obj = new ( ZfsPropertyValueConstants.FileSystem );
        Assert.Multiple ( ( ) =>
                          {
                              Assert.That ( obj,            Is.Not.Null );
                              Assert.That ( obj.Properties, Is.Not.Null );
                              Assert.That ( obj.Properties, Is.Empty );
                          }
                        );
    }

    [Test]
    public void ConvertToDatasetAndAddToCollection_ChildFileSystem_AddedToParent ( )
    {
        ZfsRecord    expectedRootRecord  = GetTestRootRecord( );
        const string dsName              = "testRoot/child";
        ZfsRecord    expectedChildRecord = expectedRootRecord.CreateChildDataset ( dsName, ZfsPropertyValueConstants.FileSystem, ZfsPropertyValueConstants.StandaloneSiazSystem, false, 54321, 12345 );
        ZfsRecord    testRootRecord      = GetTestRootRecord( );
        RawZfsObject testObject          = new ( ZfsPropertyValueConstants.FileSystem );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Type,                                       ZfsPropertyValueConstants.FileSystem,           ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Enabled,                              "false",                                        ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.TakeSnapshots,                        "false",                                        ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.PruneSnapshots,                       "false",                                        ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Recursion,                            ZfsPropertyValueConstants.SnapsInAZfs,          ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Template,                             ZfsPropertyValueConstants.Default,              ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionFrequent,            "-1",                                           ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionHourly,              "-1",                                           ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionDaily,               "-1",                                           ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionWeekly,              "-1",                                           ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionMonthly,             "-1",                                           ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionYearly,              "-1",                                           ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionPruneDeferral,       "0",                                            ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestamp, "1970-01-01T00:00:00Z",                         ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastHourlySnapshotTimestamp,   "1970-01-01T00:00:00Z",                         ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastDailySnapshotTimestamp,    "1970-01-01T00:00:00Z",                         ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestamp,   "1970-01-01T00:00:00Z",                         ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestamp,  "1970-01-01T00:00:00Z",                         ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastYearlySnapshotTimestamp,   "1970-01-01T00:00:00Z",                         ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SourceSystem,                                     ZfsPropertyValueConstants.StandaloneSiazSystem, ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Available,                                  "54321",                                        ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Used,                                       "12345",                                        ZfsPropertySourceConstants.Local );
        ConcurrentDictionary<string, ZfsRecord> datasets = new ( )
                                                           {
                                                               [ testRootRecord.Name ] = testRootRecord
                                                           };

        Assume.That ( datasets,                                           Does.ContainKey ( testRootRecord.Name ) );
        Assume.That ( datasets [ testRootRecord.Name ].ChildDatasetCount, Is.Zero );

        bool conversionResult = testObject.ConvertToDatasetAndAddToCollection ( dsName, datasets );
        Assert.Multiple ( ( ) =>
                          {
                              Assert.That ( conversionResult,                                   Is.True );
                              Assert.That ( datasets,                                           Does.ContainKey ( dsName ) );
                              Assert.That ( datasets [ testRootRecord.Name ].ChildDatasetCount, Is.EqualTo ( 1 ) );
                              bool getChildResult = datasets [ testRootRecord.Name ].GetChild ( dsName, out ZfsRecord? retrievedChildRecord );
                              Assert.That ( getChildResult,                                       Is.True );
                              Assert.That ( retrievedChildRecord?.Equals ( expectedChildRecord ), Is.True );
                              ZfsRecord ds = datasets [ dsName ];
                              Assert.That ( ds, Is.EqualTo ( expectedChildRecord ) );
                          }
                        );
    }

    [Test]
    public void ConvertToDatasetAndAddToCollection_RootFileSystemAllPropertiesExist ( )
    {
        ZfsRecord    expectedRecord = GetTestRootRecord( );
        RawZfsObject testObject     = new ( ZfsPropertyValueConstants.FileSystem );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Type,                                       ZfsPropertyValueConstants.FileSystem,  ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Enabled,                              "true",                                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.TakeSnapshots,                        "true",                                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.PruneSnapshots,                       "true",                                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Recursion,                            ZfsPropertyValueConstants.SnapsInAZfs, ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Template,                             ZfsPropertyValueConstants.Default,     ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionFrequent,            "6",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionHourly,              "5",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionDaily,               "4",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionWeekly,              "3",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionMonthly,             "2",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionYearly,              "1",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionPruneDeferral,       "0",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestamp, "1970-01-01T00:00:00Z",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastHourlySnapshotTimestamp,   "1970-01-01T00:00:00Z",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastDailySnapshotTimestamp,    "1970-01-01T00:00:00Z",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestamp,   "1970-01-01T00:00:00Z",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestamp,  "1970-01-01T00:00:00Z",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastYearlySnapshotTimestamp,   "1970-01-01T00:00:00Z",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SourceSystem,                                     "StandaloneSiazSystem",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Available,                                  "54321",                               ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Used,                                       "12345",                               ZfsPropertySourceConstants.Local );
        const string                            dsName           = "testRoot";
        ConcurrentDictionary<string, ZfsRecord> datasets         = new ( );
        bool                                    conversionResult = testObject.ConvertToDatasetAndAddToCollection ( dsName, datasets );
        Assert.Multiple ( ( ) =>
                          {
                              Assert.That ( conversionResult, Is.True );
                              Assert.That ( datasets,         Does.ContainKey ( dsName ) );
                          }
                        );
        ZfsRecord ds = datasets [ dsName ];
        Assert.That ( ds, Is.EqualTo ( expectedRecord ) );
    }

    [Test]
    [TestCase ( "" )]
    [TestCase ( " " )]
    [TestCase ( "  " )]
    [TestCase ( "\t" )]
    [TestCase ( "\n" )]
    [TestCase ( "\r" )]
    public void ConvertToDatasetAndAddToCollection_RootFileSystemEmptyName_ThrowsArgumentNullException( string dsName )
    {
        RawZfsObject testObject = new ( ZfsPropertyValueConstants.FileSystem );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Type,                                       ZfsPropertyValueConstants.FileSystem,  ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Enabled,                              "true",                                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.TakeSnapshots,                        "true",                                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.PruneSnapshots,                       "true",                                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Recursion,                            ZfsPropertyValueConstants.SnapsInAZfs, ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Template,                             ZfsPropertyValueConstants.Default,     ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionFrequent,            "6",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionHourly,              "5",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionDaily,               "4",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionWeekly,              "3",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionMonthly,             "2",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionYearly,              "1",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionPruneDeferral,       "0",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestamp, "1970-01-01T00:00:00Z",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastHourlySnapshotTimestamp,   "1970-01-01T00:00:00Z",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastDailySnapshotTimestamp,    "1970-01-01T00:00:00Z",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestamp,   "1970-01-01T00:00:00Z",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestamp,  "1970-01-01T00:00:00Z",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastYearlySnapshotTimestamp,   "1970-01-01T00:00:00Z",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SourceSystem,                                     "StandaloneSiazSystem",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Available,                                  "54321",                               ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Used,                                       "12345",                               ZfsPropertySourceConstants.Local );
        ConcurrentDictionary<string, ZfsRecord> datasets = new ( );
        Assert.Multiple ( ( ) =>
                          {
                              Assert.That ( ( ) => testObject.ConvertToDatasetAndAddToCollection ( dsName, datasets ), Throws.ArgumentNullException );
                              Assert.That ( datasets,                                                                  Does.Not.ContainKey ( dsName ) );
                          }
                        );
    }

    [Test]
    public void ConvertToDatasetAndAddToCollection_RootFileSystemInvalidPropertyValue_ReturnsFalse ( )
    {
        RawZfsObject testObject = new ( ZfsPropertyValueConstants.FileSystem );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Type,                                       ZfsPropertyValueConstants.FileSystem,           ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Enabled,                              "true",                                         ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.TakeSnapshots,                        "true",                                         ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.PruneSnapshots,                       "true",                                         ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Recursion,                            ZfsPropertyValueConstants.SnapsInAZfs,          ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Template,                             ZfsPropertyValueConstants.Default,              ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionFrequent,            "6",                                            ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionHourly,              "5",                                            ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionDaily,               "4",                                            ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionWeekly,              "3",                                            ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionMonthly,             "2",                                            ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionYearly,              "1",                                            ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionPruneDeferral,       "0",                                            ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestamp, "BAD DATE STRING",                              ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastHourlySnapshotTimestamp,   "1970-01-01T00:00:00Z",                         ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastDailySnapshotTimestamp,    "1970-01-01T00:00:00Z",                         ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestamp,   "1970-01-01T00:00:00Z",                         ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestamp,  "1970-01-01T00:00:00Z",                         ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastYearlySnapshotTimestamp,   "1970-01-01T00:00:00Z",                         ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SourceSystem,                                     ZfsPropertyValueConstants.StandaloneSiazSystem, ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Available,                                  "54321",                                        ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Used,                                       "12345",                                        ZfsPropertySourceConstants.Local );
        string                                  dsName           = "testRoot";
        ConcurrentDictionary<string, ZfsRecord> datasets         = new ( );
        bool                                    conversionResult = testObject.ConvertToDatasetAndAddToCollection ( dsName, datasets );
        Assert.Multiple ( ( ) =>
                          {
                              Assert.That ( conversionResult, Is.False );
                              Assert.That ( datasets,         Does.Not.ContainKey ( dsName ) );
                          }
                        );
    }

    [Test]
    public void ConvertToDatasetAndAddToCollection_RootFileSystemNotEnoughProperties_ThrowsInvalidOperationException ( )
    {
        RawZfsObject testObject = new ( ZfsPropertyValueConstants.FileSystem );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Type,                                       ZfsPropertyValueConstants.FileSystem,  ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Enabled,                              "true",                                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.TakeSnapshots,                        "true",                                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.PruneSnapshots,                       "true",                                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Recursion,                            ZfsPropertyValueConstants.SnapsInAZfs, ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Template,                             ZfsPropertyValueConstants.Default,     ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionFrequent,            "6",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionHourly,              "5",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionDaily,               "4",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionWeekly,              "3",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionMonthly,             "2",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionYearly,              "1",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionPruneDeferral,       "0",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestamp, "1970-01-01T00:00:00Z",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastHourlySnapshotTimestamp,   "1970-01-01T00:00:00Z",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastDailySnapshotTimestamp,    "1970-01-01T00:00:00Z",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestamp,   "1970-01-01T00:00:00Z",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Available,                                  "54321",                               ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Used,                                       "12345",                               ZfsPropertySourceConstants.Local );
        string                                  dsName   = "testRoot";
        ConcurrentDictionary<string, ZfsRecord> datasets = new ( );
        Assert.Multiple ( ( ) =>
                          {
                              Assert.That ( ( ) => testObject.ConvertToDatasetAndAddToCollection ( dsName, datasets ), Throws.InvalidOperationException );
                              Assert.That ( datasets,                                                                  Does.Not.ContainKey ( dsName ) );
                          }
                        );
    }

    [Test]
    public void ConvertToDatasetAndAddToCollection_RootFileSystemSourceSystemEmptyString_ThrowsArgumentException ( )
    {
        RawZfsObject testObject = new ( ZfsPropertyValueConstants.FileSystem );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Type,                                       ZfsPropertyValueConstants.FileSystem,  ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Enabled,                              "true",                                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.TakeSnapshots,                        "true",                                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.PruneSnapshots,                       "true",                                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Recursion,                            ZfsPropertyValueConstants.SnapsInAZfs, ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Template,                             ZfsPropertyValueConstants.Default,     ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionFrequent,            "6",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionHourly,              "5",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionDaily,               "4",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionWeekly,              "3",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionMonthly,             "2",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionYearly,              "1",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionPruneDeferral,       "0",                                   ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestamp, "1970-01-01T00:00:00Z",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastHourlySnapshotTimestamp,   "1970-01-01T00:00:00Z",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastDailySnapshotTimestamp,    "1970-01-01T00:00:00Z",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestamp,   "1970-01-01T00:00:00Z",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestamp,  "1970-01-01T00:00:00Z",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastYearlySnapshotTimestamp,   "1970-01-01T00:00:00Z",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SourceSystem,                                     string.Empty,                          ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Available,                                  "54321",                               ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Used,                                       "12345",                               ZfsPropertySourceConstants.Local );
        string                                  dsName   = "testRoot";
        ConcurrentDictionary<string, ZfsRecord> datasets = new ( );
        Assert.Multiple ( ( ) =>
                          {
                              Assert.That ( ( ) => testObject.ConvertToDatasetAndAddToCollection ( dsName, datasets ), Throws.ArgumentException );
                              Assert.That ( datasets,                                                                  Does.Not.ContainKey ( dsName ) );
                          }
                        );
    }

    [Test]
    public void ConvertToSnapshotAndAddToCollections_OnRootFileSystemAllPropertiesExist_AddedToCollections ( )
    {
        ZfsRecord    rootRecord = GetTestRootRecord( );
        RawZfsObject testObject = new ( ZfsPropertyValueConstants.Snapshot );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Type,                                       ZfsPropertyValueConstants.Snapshot,    ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Enabled,                              "true",                                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.TakeSnapshots,                        "true",                                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.PruneSnapshots,                       "true",                                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Recursion,                            ZfsPropertyValueConstants.SnapsInAZfs, ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotPeriod,                       SnapshotPeriod.Hourly,                 ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotTimestamp,                    "1970-01-01T00:00:00Z",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Template,                             ZfsPropertyValueConstants.Default,     "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionFrequent,            "6",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionHourly,              "5",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionDaily,               "4",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionWeekly,              "3",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionMonthly,             "2",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionYearly,              "1",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionPruneDeferral,       "0",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestamp, "1970-01-01T00:00:00Z",                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastHourlySnapshotTimestamp,   "1970-01-01T00:00:00Z",                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastDailySnapshotTimestamp,    "1970-01-01T00:00:00Z",                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestamp,   "1970-01-01T00:00:00Z",                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestamp,  "1970-01-01T00:00:00Z",                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastYearlySnapshotTimestamp,   "1970-01-01T00:00:00Z",                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SourceSystem,                                     "StandaloneSiazSystem",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Used,                                       "12345",                               "inherited from testRoot" );
        const string snapName = "testRoot@autosnap_1970-01-01T00:00:00Z_hourly";
        ConcurrentDictionary<string, ZfsRecord> datasets = new ( )
                                                           {
                                                               [ rootRecord.Name ] = rootRecord
                                                           };
        ConcurrentDictionary<string, Snapshot> snapshots = new ( );
        testObject.ConvertToSnapshotAndAddToCollections ( snapName, datasets, snapshots );
        Assert.Multiple ( ( ) =>
                          {
                              Assert.That ( snapshots,                                          Does.ContainKey ( snapName ) );
                              Assert.That ( rootRecord.Snapshots [ SnapshotPeriodKind.Hourly ], Does.ContainKey ( snapName ) );
                          }
                        );
    }

    [Test]
    [TestCase ( "" )]
    [TestCase ( " " )]
    [TestCase ( "  " )]
    [TestCase ( "\t" )]
    [TestCase ( "\n" )]
    [TestCase ( "\r" )]
    public void ConvertToSnapshotAndAddToCollections_OnRootFileSystemEmptySnapNape_ThrowsArgumentNullException( string snapName )
    {
        ZfsRecord    rootRecord = GetTestRootRecord( );
        RawZfsObject testObject = new ( ZfsPropertyValueConstants.Snapshot );
        ConcurrentDictionary<string, ZfsRecord> datasets = new ( )
                                                           {
                                                               [ rootRecord.Name ] = rootRecord
                                                           };
        ConcurrentDictionary<string, Snapshot> snapshots = new ( );

        Assert.Multiple ( ( ) =>
                          {
                              Assert.That ( ( ) => testObject.ConvertToSnapshotAndAddToCollections ( snapName, datasets, snapshots ), Throws.ArgumentNullException );
                              Assert.That ( snapshots,                                                                                Does.Not.ContainKey ( snapName ) );
                              Assert.That ( rootRecord.Snapshots [ SnapshotPeriodKind.Hourly ],                                       Does.Not.ContainKey ( snapName ) );
                          }
                        );
    }

    [Test]
    public void ConvertToSnapshotAndAddToCollections_OnRootFileSystemInvalidSnapshotTimestampPropertyValue_ReturnsFalse ( )
    {
        ZfsRecord    rootRecord = GetTestRootRecord( );
        RawZfsObject testObject = new ( ZfsPropertyValueConstants.Snapshot );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Type,                                       ZfsPropertyValueConstants.Snapshot,    ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Enabled,                              "true",                                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.TakeSnapshots,                        "true",                                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.PruneSnapshots,                       "true",                                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Recursion,                            ZfsPropertyValueConstants.SnapsInAZfs, ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotPeriod,                       SnapshotPeriod.Hourly,                 ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotTimestamp,                    "INVALID DATE VALUE",                  ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Template,                             ZfsPropertyValueConstants.Default,     "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionFrequent,            "6",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionHourly,              "5",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionDaily,               "4",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionWeekly,              "3",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionMonthly,             "2",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionYearly,              "1",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionPruneDeferral,       "0",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestamp, "1970-01-01T00:00:00Z",                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastHourlySnapshotTimestamp,   "1970-01-01T00:00:00Z",                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastDailySnapshotTimestamp,    "1970-01-01T00:00:00Z",                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestamp,   "1970-01-01T00:00:00Z",                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestamp,  "1970-01-01T00:00:00Z",                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastYearlySnapshotTimestamp,   "1970-01-01T00:00:00Z",                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SourceSystem,                                     "StandaloneSiazSystem",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Used,                                       "12345",                               "inherited from testRoot" );
        const string snapName = "testRoot@autosnap_1970-01-01T00:00:00Z_hourly";
        ConcurrentDictionary<string, ZfsRecord> datasets = new ( )
                                                           {
                                                               [ rootRecord.Name ] = rootRecord
                                                           };
        ConcurrentDictionary<string, Snapshot> snapshots        = new ( );
        bool                                   conversionResult = testObject.ConvertToSnapshotAndAddToCollections ( snapName, datasets, snapshots );
        Assert.Multiple ( ( ) =>
                          {
                              Assert.That ( conversionResult,                                   Is.False );
                              Assert.That ( snapshots,                                          Does.Not.ContainKey ( snapName ) );
                              Assert.That ( rootRecord.Snapshots [ SnapshotPeriodKind.Hourly ], Does.Not.ContainKey ( snapName ) );
                          }
                        );
    }

    [Test]
    public void ConvertToSnapshotAndAddToCollections_OnRootFileSystemInvalidZfsRecordPropertyValue_ReturnsFalse ( )
    {
        ZfsRecord    rootRecord = GetTestRootRecord( );
        RawZfsObject testObject = new ( ZfsPropertyValueConstants.Snapshot );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Type,                                       ZfsPropertyValueConstants.Snapshot,    ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Enabled,                              "true",                                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.TakeSnapshots,                        "true",                                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.PruneSnapshots,                       "true",                                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Recursion,                            ZfsPropertyValueConstants.SnapsInAZfs, ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotPeriod,                       SnapshotPeriod.Hourly,                 ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotTimestamp,                    "1970-01-01T00:00:00Z",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Template,                             ZfsPropertyValueConstants.Default,     "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionFrequent,            "6",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionHourly,              "5",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionDaily,               "4",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionWeekly,              "3",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionMonthly,             "2",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionYearly,              "1",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionPruneDeferral,       "0",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestamp, "INVALID DATE VALUE",                  "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastHourlySnapshotTimestamp,   "1970-01-01T00:00:00Z",                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastDailySnapshotTimestamp,    "1970-01-01T00:00:00Z",                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestamp,   "1970-01-01T00:00:00Z",                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestamp,  "1970-01-01T00:00:00Z",                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastYearlySnapshotTimestamp,   "1970-01-01T00:00:00Z",                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SourceSystem,                                     "StandaloneSiazSystem",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Used,                                       "12345",                               "inherited from testRoot" );
        const string snapName = "testRoot@autosnap_1970-01-01T00:00:00Z_hourly";
        ConcurrentDictionary<string, ZfsRecord> datasets = new ( )
                                                           {
                                                               [ rootRecord.Name ] = rootRecord
                                                           };
        ConcurrentDictionary<string, Snapshot> snapshots        = new ( );
        bool                                   conversionResult = testObject.ConvertToSnapshotAndAddToCollections ( snapName, datasets, snapshots );
        Assert.Multiple ( ( ) =>
                          {
                              Assert.That ( conversionResult,                                   Is.False );
                              Assert.That ( snapshots,                                          Does.Not.ContainKey ( snapName ) );
                              Assert.That ( rootRecord.Snapshots [ SnapshotPeriodKind.Hourly ], Does.Not.ContainKey ( snapName ) );
                          }
                        );
    }

    [Test]
    public void ConvertToSnapshotAndAddToCollections_OnRootFileSystemMissingFileSystemProperties_ReturnsFalse ( )
    {
        RawZfsObject testObject = new ( ZfsPropertyValueConstants.Snapshot );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Type,                            ZfsPropertyValueConstants.Snapshot,    ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Enabled,                   "true",                                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.TakeSnapshots,             "true",                                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.PruneSnapshots,            "true",                                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Recursion,                 ZfsPropertyValueConstants.SnapsInAZfs, ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotPeriod,            SnapshotPeriod.Hourly,                 ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotTimestamp,         "1970-01-01T00:00:00Z",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Template,                  ZfsPropertyValueConstants.Default,     "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionFrequent, "6",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionHourly,   "5",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionDaily,    "4",                                   "inherited from testRoot" );

        // Weekly property intentionally missing as the condition of the test
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionMonthly,             "2",                    "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionYearly,              "1",                    "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionPruneDeferral,       "0",                    "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestamp, "1970-01-01T00:00:00Z", "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastHourlySnapshotTimestamp,   "1970-01-01T00:00:00Z", "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastDailySnapshotTimestamp,    "1970-01-01T00:00:00Z", "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestamp,   "1970-01-01T00:00:00Z", "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestamp,  "1970-01-01T00:00:00Z", "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastYearlySnapshotTimestamp,   "1970-01-01T00:00:00Z", "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SourceSystem,                                     "StandaloneSiazSystem", ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Used,                                       "12345",                "inherited from testRoot" );
        const string snapName = "testRoot@autosnap_1970-01-01T00:00:00Z_hourly";

        Assert.Multiple ( ( ) =>
                          {
                              ZfsRecord rootRecord = GetTestRootRecord( );

                              ConcurrentDictionary<string, ZfsRecord> datasets = new ( )
                                                                                 {
                                                                                     [ rootRecord.Name ] = rootRecord
                                                                                 };
                              ConcurrentDictionary<string, Snapshot> snapshots = [ ];
                              Assert.That ( ( ) => testObject.ConvertToSnapshotAndAddToCollections ( snapName, datasets, snapshots ), Is.False );
                              Assert.That ( snapshots,                                                                                Does.Not.ContainKey ( snapName ) );
                              Assert.That ( rootRecord.Snapshots [ SnapshotPeriodKind.Hourly ],                                       Does.Not.ContainKey ( snapName ) );
                          }
                        );
    }

    [Test]
    public void ConvertToSnapshotAndAddToCollections_OnRootFileSystemMissingSnapshotProperties_ThrowsInvalidOperationException ( )
    {
        ZfsRecord    rootRecord = GetTestRootRecord( );
        RawZfsObject testObject = new ( ZfsPropertyValueConstants.Snapshot );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Type,                                       ZfsPropertyValueConstants.Snapshot,    ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Enabled,                              "true",                                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.TakeSnapshots,                        "true",                                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.PruneSnapshots,                       "true",                                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Recursion,                            ZfsPropertyValueConstants.SnapsInAZfs, ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotTimestamp,                    "1970-01-01T00:00:00Z",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsPropertyNames.Template,                             ZfsPropertyValueConstants.Default,     "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionFrequent,            "6",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionHourly,              "5",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionDaily,               "4",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionWeekly,              "3",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionMonthly,             "2",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionYearly,              "1",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SnapshotRetentionPruneDeferral,       "0",                                   "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestamp, "1970-01-01T00:00:00Z",                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastHourlySnapshotTimestamp,   "1970-01-01T00:00:00Z",                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastDailySnapshotTimestamp,    "1970-01-01T00:00:00Z",                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestamp,   "1970-01-01T00:00:00Z",                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestamp,  "1970-01-01T00:00:00Z",                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.DatasetLastYearlySnapshotTimestamp,   "1970-01-01T00:00:00Z",                "inherited from testRoot" );
        testObject.AddRawProperty ( ZfsPropertyNames.SourceSystem,                                     "StandaloneSiazSystem",                ZfsPropertySourceConstants.Local );
        testObject.AddRawProperty ( ZfsNativePropertyNames.Used,                                       "12345",                               "inherited from testRoot" );
        const string snapName = "testRoot@autosnap_1970-01-01T00:00:00Z_hourly";
        ConcurrentDictionary<string, ZfsRecord> datasets = new ( )
                                                           {
                                                               [ rootRecord.Name ] = rootRecord
                                                           };
        ConcurrentDictionary<string, Snapshot> snapshots = new ( );

        Assert.Multiple ( ( ) =>
                          {
                              Assert.That ( ( ) => testObject.ConvertToSnapshotAndAddToCollections ( snapName, datasets, snapshots ), Throws.InvalidOperationException );
                              Assert.That ( snapshots,                                                                                Does.Not.ContainKey ( snapName ) );
                              Assert.That ( rootRecord.Snapshots [ SnapshotPeriodKind.Hourly ],                                       Does.Not.ContainKey ( snapName ) );
                          }
                        );
    }

    private static TestCaseData[] GetAddRawProperty_TestCases ( )
    {
        return IZfsProperty.AllKnownProperties.Select ( static kp => new TestCaseData ( kp, $"{kp} value", ZfsPropertySourceConstants.Local ) )
                           .Prepend ( new TestCaseData ( "type",     ZfsPropertyValueConstants.FileSystem, ZfsPropertySourceConstants.None ) )
                           .Append ( new TestCaseData ( "available", "54321",                              ZfsPropertySourceConstants.None ) )
                           .Append ( new TestCaseData ( "used",      "12345",                              ZfsPropertySourceConstants.None ) )
                           .ToArray( );
    }

    private static ZfsRecord GetTestRootRecord ( )
    {
        return ZfsRecord.CreateInstanceFromAllProperties (
                                                          "testRoot",
                                                          ZfsPropertyValueConstants.FileSystem,
                                                          ZfsProperty<bool>.CreateWithoutParent ( ZfsPropertyNames.Enabled,        true ),
                                                          ZfsProperty<bool>.CreateWithoutParent ( ZfsPropertyNames.TakeSnapshots,  true ),
                                                          ZfsProperty<bool>.CreateWithoutParent ( ZfsPropertyNames.PruneSnapshots, true ),
                                                          ZfsProperty<DateTimeOffset>.CreateWithoutParent ( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestamp, DateTimeOffset.UnixEpoch ),
                                                          ZfsProperty<DateTimeOffset>.CreateWithoutParent ( ZfsPropertyNames.DatasetLastHourlySnapshotTimestamp,   DateTimeOffset.UnixEpoch ),
                                                          ZfsProperty<DateTimeOffset>.CreateWithoutParent ( ZfsPropertyNames.DatasetLastDailySnapshotTimestamp,    DateTimeOffset.UnixEpoch ),
                                                          ZfsProperty<DateTimeOffset>.CreateWithoutParent ( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestamp,   DateTimeOffset.UnixEpoch ),
                                                          ZfsProperty<DateTimeOffset>.CreateWithoutParent ( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestamp,  DateTimeOffset.UnixEpoch ),
                                                          ZfsProperty<DateTimeOffset>.CreateWithoutParent ( ZfsPropertyNames.DatasetLastYearlySnapshotTimestamp,   DateTimeOffset.UnixEpoch ),
                                                          ZfsProperty<string>.CreateWithoutParent ( ZfsPropertyNames.Recursion, ZfsPropertyValueConstants.SnapsInAZfs ),
                                                          ZfsProperty<string>.CreateWithoutParent ( ZfsPropertyNames.Template,  ZfsPropertyValueConstants.Default ),
                                                          ZfsProperty<int>.CreateWithoutParent ( ZfsPropertyNames.SnapshotRetentionFrequent,      6 ),
                                                          ZfsProperty<int>.CreateWithoutParent ( ZfsPropertyNames.SnapshotRetentionHourly,        5 ),
                                                          ZfsProperty<int>.CreateWithoutParent ( ZfsPropertyNames.SnapshotRetentionDaily,         4 ),
                                                          ZfsProperty<int>.CreateWithoutParent ( ZfsPropertyNames.SnapshotRetentionWeekly,        3 ),
                                                          ZfsProperty<int>.CreateWithoutParent ( ZfsPropertyNames.SnapshotRetentionMonthly,       2 ),
                                                          ZfsProperty<int>.CreateWithoutParent ( ZfsPropertyNames.SnapshotRetentionYearly,        1 ),
                                                          ZfsProperty<int>.CreateWithoutParent ( ZfsPropertyNames.SnapshotRetentionPruneDeferral, 0 ),
                                                          ZfsProperty<string>.CreateWithoutParent ( ZfsPropertyNames.SourceSystem, "StandaloneSiazSystem" ),
                                                          54321,
                                                          12345
                                                         );
    }
}
