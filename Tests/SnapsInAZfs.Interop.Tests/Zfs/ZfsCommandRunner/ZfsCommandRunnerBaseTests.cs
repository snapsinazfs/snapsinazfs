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
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsCommandRunner;

[TestFixture]
[TestOf( typeof( ZfsCommandRunnerBase ) )]
public class ZfsCommandRunnerBaseTests
{
    [Test]
    [TestCaseSource( nameof( GetBooleanPropertyTestCases ) )]
    [TestCaseSource( nameof( GetUnrestrictedPositiveIntPropertyTestCases ) )]
    [TestCaseSource( nameof( GetRestrictedPositiveIntPropertyTestCases ) )]
    [TestCaseSource( nameof( GetDateTimeOffsetPropertyTestCases ) )]
    [TestCaseSource( nameof( GetRecursionPropertyTestCases ) )]
    [TestCaseSource( nameof( GetUnformattedStringPropertyTestCases ) )]
    [TestCaseSource( nameof( GetNativeLongPropertyTestCases ) )]
    [TestCaseSource( nameof( GetNativeTypePropertyTestCases ) )]
    public bool CheckIfPropertyIsValid( string propertyName, string propertyValue, string propertySource )
    {
        return ZfsCommandRunnerBaseProtectedMethodsTestClass.CheckIfPropertyIsValidProxy( propertyName, propertyValue, propertySource );
    }

    [Test]
    public void CheckIfPropertyIsValid_ThrowsOnNameOutOfRange( )
    {
        Assert.That( ( ) => ZfsCommandRunnerBaseProtectedMethodsTestClass.CheckIfPropertyIsValidProxy( "BOGUS PROPERTY NAME", "Unimportant Value", ZfsPropertySourceConstants.Local ), Throws.TypeOf<ArgumentOutOfRangeException>( ) );
    }

    [Test]
    public void ParseAndValidatePoolRootZfsGetLine_ReturnsFalseWhenArrayTooShort( [ValueSource( nameof( GetShortLineTokenArrays ) )] string[] lineTokens )
    {
        ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> testCollection = new( );
        Assume.That( testCollection, Is.Empty );
        bool methodResult = ZfsCommandRunnerBaseProtectedMethodsTestClass.ParseAndValidatePoolRootZfsGetLineProxy( lineTokens, ref testCollection );
        Assert.That( methodResult, Is.False );
        Assert.That( testCollection, Is.Empty );
    }

    [Test]
    public void ParseAndValidatePoolRootZfsGetLine_ReturnsTrueAndContainsPropertyValiditiesWhenValid( [ValueSource( nameof( GetValidZfsPoolRootLineTokenArrays ) )] string[] lineTokens )
    {
        ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> testCollection = new( );
        Assume.That( testCollection, Is.Empty );
        bool methodResult = ZfsCommandRunnerBaseProtectedMethodsTestClass.ParseAndValidatePoolRootZfsGetLineProxy( lineTokens, ref testCollection );
        Assert.Multiple( ( ) =>
        {
            Assert.That( methodResult, Is.True );
            Assert.That( testCollection, Is.Not.Empty );
            Assert.That( testCollection, Does.ContainKey( lineTokens[ 0 ] ) );
            Assert.That( testCollection[ lineTokens[ 0 ] ], Does.ContainKey( lineTokens[ 1 ] ) );
            Assert.That( testCollection[ lineTokens[ 0 ] ][ lineTokens[ 1 ] ], Is.True );
        } );
    }

    private static List<TestCaseData> GetBooleanPropertyTestCases( )
    {
        string[] propertyNames = [ZfsPropertyNames.EnabledPropertyName, ZfsPropertyNames.TakeSnapshotsPropertyName, ZfsPropertyNames.PruneSnapshotsPropertyName];
        (string, bool)[] propertyValues = [( "true", true ), ( "false", true ), ( "", false ), ( "Plain Text", false ), ( "1970-01-01T00:00:00Z", false )];
        (string, bool)[] propertySources = [( ZfsPropertySourceConstants.Local, true ), ( ZfsPropertySourceConstants.None, false ), ( "inherited from something", true )];
        return ( from name in propertyNames from val in propertyValues from source in propertySources select new TestCaseData( name, val.Item1, source.Item1 ) { HasExpectedResult = true, ExpectedResult = val.Item2 && source.Item2 } ).ToList( );
    }

    private static List<TestCaseData> GetDateTimeOffsetPropertyTestCases( )
    {
        string[] propertyNames = [ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName];
        (string, bool)[] propertyValues = [( "true", false ), ( "false", false ), ( "", false ), ( "Plain Text", false ), ( "1970-01-01T00:00:00Z", true ), ( "0", false ), ( "1", false ), ( "100", false ), ( int.MaxValue.ToString( ), false ), ( "-1", false )];
        (string, bool)[] propertySources = [( ZfsPropertySourceConstants.Local, true ), ( ZfsPropertySourceConstants.None, false ), ( "inherited from something", true )];
        return ( from name in propertyNames from val in propertyValues from source in propertySources select new TestCaseData( name, val.Item1, source.Item1 ) { HasExpectedResult = true, ExpectedResult = val.Item2 && source.Item2 } ).ToList( );
    }

    private static List<TestCaseData> GetNativeLongPropertyTestCases( )
    {
        string[] propertyNames = [ZfsNativePropertyNames.Available, ZfsNativePropertyNames.Used];
        (string, bool)[] propertyValues = [( "true", false ), ( "false", false ), ( "", false ), ( "Plain Text", false ), ( "1970-01-01T00:00:00Z", false ), ( "0", true ), ( ZfsPropertyValueConstants.SnapsInAZfs, false ), ( ZfsPropertyValueConstants.ZfsRecursion, false ), ( "\t", false ), ( " ", false ), ( long.MaxValue.ToString( ), true )];
        (string, bool)[] propertySources = [( ZfsPropertySourceConstants.Local, true ), ( ZfsPropertySourceConstants.None, false ), ( "inherited from something", true )];
        return ( from name in propertyNames from val in propertyValues from source in propertySources select new TestCaseData( name, val.Item1, source.Item1 ) { HasExpectedResult = true, ExpectedResult = val.Item2 && source.Item2 } ).ToList( );
    }

    private static List<TestCaseData> GetNativeTypePropertyTestCases( )
    {
        string[] propertyNames = [ZfsNativePropertyNames.Type];
        (string, bool)[] propertyValues = [( "true", false ), ( "false", false ), ( "", false ), ( "Plain Text", false ), ( "1970-01-01T00:00:00Z", false ), ( "0", false ), ( ZfsPropertyValueConstants.FileSystem, true ), ( ZfsPropertyValueConstants.Volume, true ), ( ZfsPropertyValueConstants.SnapsInAZfs, false ), ( ZfsPropertyValueConstants.ZfsRecursion, false ), ( "\t", false ), ( " ", false ), ( long.MaxValue.ToString( ), false )];
        (string, bool)[] propertySources = [( ZfsPropertySourceConstants.Local, true ), ( ZfsPropertySourceConstants.None, false ), ( "inherited from something", true )];
        return ( from name in propertyNames from val in propertyValues from source in propertySources select new TestCaseData( name, val.Item1, source.Item1 ) { HasExpectedResult = true, ExpectedResult = val.Item2 && source.Item2 } ).ToList( );
    }

    private static List<TestCaseData> GetRecursionPropertyTestCases( )
    {
        string[] propertyNames = [ZfsPropertyNames.RecursionPropertyName];
        (string, bool)[] propertyValues = [( "true", false ), ( "false", false ), ( "", false ), ( "Plain Text", false ), ( "1970-01-01T00:00:00Z", false ), ( "0", false ), ( ZfsPropertyValueConstants.SnapsInAZfs, true ), ( ZfsPropertyValueConstants.ZfsRecursion, true )];
        (string, bool)[] propertySources = [( ZfsPropertySourceConstants.Local, true ), ( ZfsPropertySourceConstants.None, false ), ( "inherited from something", true )];
        return ( from name in propertyNames from val in propertyValues from source in propertySources select new TestCaseData( name, val.Item1, source.Item1 ) { HasExpectedResult = true, ExpectedResult = val.Item2 && source.Item2 } ).ToList( );
    }

    private static List<TestCaseData> GetRestrictedPositiveIntPropertyTestCases( )
    {
        string[] propertyNames = [ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName];
        (string, bool)[] propertyValues = [( "true", false ), ( "false", false ), ( "", false ), ( "Plain Text", false ), ( "1970-01-01T00:00:00Z", false ), ( "0", true ), ( "1", true ), ( "100", true ), ( int.MaxValue.ToString( ), false ), ( "-1", false )];
        (string, bool)[] propertySources = [( ZfsPropertySourceConstants.Local, true ), ( ZfsPropertySourceConstants.None, false ), ( "inherited from something", true )];
        return ( from name in propertyNames from val in propertyValues from source in propertySources select new TestCaseData( name, val.Item1, source.Item1 ) { HasExpectedResult = true, ExpectedResult = val.Item2 && source.Item2 } ).ToList( );
    }

    private static IEnumerable<string[]> GetShortLineTokenArrays( )
    {
        yield return ["testPool"];
        yield return ["testPool", "testProperty"];
        yield return ["testPool", "testProperty", "testValue"];
    }

    private static List<TestCaseData> GetUnformattedStringPropertyTestCases( )
    {
        string[] propertyNames = [ZfsPropertyNames.TemplatePropertyName, ZfsPropertyNames.SourceSystem];
        (string, bool)[] propertyValues = [( "true", true ), ( "false", true ), ( "", false ), ( "Plain Text", true ), ( "1970-01-01T00:00:00Z", true ), ( "0", true ), ( ZfsPropertyValueConstants.SnapsInAZfs, true ), ( ZfsPropertyValueConstants.ZfsRecursion, true ), ( "\t", false ), ( " ", false )];
        (string, bool)[] propertySources = [( ZfsPropertySourceConstants.Local, true ), ( ZfsPropertySourceConstants.None, false ), ( "inherited from something", true )];
        return ( from name in propertyNames from val in propertyValues from source in propertySources select new TestCaseData( name, val.Item1, source.Item1 ) { HasExpectedResult = true, ExpectedResult = val.Item2 && source.Item2 } ).ToList( );
    }

    private static List<TestCaseData> GetUnrestrictedPositiveIntPropertyTestCases( )
    {
        string[] propertyNames = [ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, ZfsPropertyNames.SnapshotRetentionDailyPropertyName, ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, ZfsPropertyNames.SnapshotRetentionYearlyPropertyName];
        (string, bool)[] propertyValues = [( "true", false ), ( "false", false ), ( "", false ), ( "Plain Text", false ), ( "1970-01-01T00:00:00Z", false ), ( "0", true ), ( "1", true ), ( "100", true ), ( int.MaxValue.ToString( ), true ), ( "-1", false )];
        (string, bool)[] propertySources = [( ZfsPropertySourceConstants.Local, true ), ( ZfsPropertySourceConstants.None, false ), ( "inherited from something", true )];
        return ( from name in propertyNames from val in propertyValues from source in propertySources select new TestCaseData( name, val.Item1, source.Item1 ) { HasExpectedResult = true, ExpectedResult = val.Item2 && source.Item2 } ).ToList( );
    }

    private static IEnumerable<string[]> GetValidZfsPoolRootLineTokenArrays( )
    {
        yield return ["testRootA", "snapsinazfs.com:enabled", "false", "local"];
        yield return ["testRootA", "snapsinazfs.com:lastdailysnapshottimestamp", "1970-01-01T00:00:00.0000000+00:00", "local"];
        yield return ["testRootA", "snapsinazfs.com:lastfrequentsnapshottimestamp", "1970-01-01T00:00:00.0000000+00:00", "local"];
        yield return ["testRootA", "snapsinazfs.com:lasthourlysnapshottimestamp", "1970-01-01T00:00:00.0000000+00:00", "local"];
        yield return ["testRootA", "snapsinazfs.com:lastmonthlysnapshottimestamp", "1970-01-01T00:00:00.0000000+00:00", "local"];
        yield return ["testRootA", "snapsinazfs.com:lastweeklysnapshottimestamp", "1970-01-01T00:00:00.0000000+00:00", "local"];
        yield return ["testRootA", "snapsinazfs.com:lastyearlysnapshottimestamp", "1970-01-01T00:00:00.0000000+00:00", "local"];
        yield return ["testRootA", "snapsinazfs.com:prunesnapshots", "false", "local"];
        yield return ["testRootA", "snapsinazfs.com:recursion", "siaz", "local"];
        yield return ["testRootA", "snapsinazfs.com:retention:daily", "7", "local"];
        yield return ["testRootA", "snapsinazfs.com:retention:frequent", "4", "local"];
        yield return ["testRootA", "snapsinazfs.com:retention:hourly", "6", "local"];
        yield return ["testRootA", "snapsinazfs.com:retention:monthly", "12", "local"];
        yield return ["testRootA", "snapsinazfs.com:retention:prunedeferral", "0", "local"];
        yield return ["testRootA", "snapsinazfs.com:retention:weekly", "5", "local"];
        yield return ["testRootA", "snapsinazfs.com:retention:yearly", "1", "local"];
        yield return ["testRootA", "snapsinazfs.com:takesnapshots", "false", "local"];
        yield return ["testRootA", "snapsinazfs.com:template", "default", "local"];
        yield return ["testRootA", "snapsinazfs.com:sourcesystem", "host.domain.tld", "local"];
        yield return ["testRootB", "snapsinazfs.com:enabled", "false", "local"];
        yield return ["testRootB", "snapsinazfs.com:lastdailysnapshottimestamp", "1970-01-01T00:00:00.0000000+00:00", "local"];
        yield return ["testRootB", "snapsinazfs.com:lastfrequentsnapshottimestamp", "1970-01-01T00:00:00.0000000+00:00", "local"];
        yield return ["testRootB", "snapsinazfs.com:lasthourlysnapshottimestamp", "1970-01-01T00:00:00.0000000+00:00", "local"];
        yield return ["testRootB", "snapsinazfs.com:lastmonthlysnapshottimestamp", "1970-01-01T00:00:00.0000000+00:00", "local"];
        yield return ["testRootB", "snapsinazfs.com:lastweeklysnapshottimestamp", "1970-01-01T00:00:00.0000000+00:00", "local"];
        yield return ["testRootB", "snapsinazfs.com:lastyearlysnapshottimestamp", "1970-01-01T00:00:00.0000000+00:00", "local"];
        yield return ["testRootB", "snapsinazfs.com:prunesnapshots", "false", "local"];
        yield return ["testRootB", "snapsinazfs.com:recursion", "siaz", "local"];
        yield return ["testRootB", "snapsinazfs.com:retention:daily", "7", "local"];
        yield return ["testRootB", "snapsinazfs.com:retention:frequent", "4", "local"];
        yield return ["testRootB", "snapsinazfs.com:retention:hourly", "6", "local"];
        yield return ["testRootB", "snapsinazfs.com:retention:monthly", "12", "local"];
        yield return ["testRootB", "snapsinazfs.com:retention:prunedeferral", "0", "local"];
        yield return ["testRootB", "snapsinazfs.com:retention:weekly", "5", "local"];
        yield return ["testRootB", "snapsinazfs.com:retention:yearly", "1", "local"];
        yield return ["testRootB", "snapsinazfs.com:takesnapshots", "false", "local"];
        yield return ["testRootB", "snapsinazfs.com:template", "default", "local"];
        yield return ["testRootB", "snapsinazfs.com:sourcesystem", "host.domain.tld", "local"];
    }
}

/// <summary>
///     A mostly-empty class intended only for testing of inherited protected methods from the base class, for test purposes
/// </summary>
/// <remarks>
///     Overridden abstract member functions all throw <see cref="NotImplementedException" />
/// </remarks>
public abstract class ZfsCommandRunnerBaseProtectedMethodsTestClass : ZfsCommandRunnerBase
{
    /// <summary>
    ///     Calls the protected <see cref="ZfsCommandRunnerBase.CheckIfPropertyIsValid" /> function directly, and returns its value.
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="propertyValue"></param>
    /// <param name="propertySource"></param>
    /// <returns></returns>
    public static bool CheckIfPropertyIsValidProxy( string propertyName, string propertyValue, string propertySource )
    {
        return CheckIfPropertyIsValid( propertyName, propertyValue, propertySource );
    }

    public static bool ParseAndValidatePoolRootZfsGetLineProxy( string[] lineTokens, ref ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> rootsAndTheirProperties )
    {
        return ParseAndValidatePoolRootZfsGetLine( lineTokens, ref rootsAndTheirProperties );
    }

#region Overrides of ZfsCommandRunnerBase

    /// <inheritdoc />
    public override Task<ZfsCommandRunnerOperationStatus> SetZfsPropertiesAsync( bool dryRun, string zfsPath, List<IZfsProperty> properties )
    {
        throw new NotImplementedException( );
    }

    /// <inheritdoc />
    public override Task GetDatasetsAndSnapshotsFromZfsAsync( SnapsInAZfsSettings settings, ConcurrentDictionary<string, ZfsRecord> datasets, ConcurrentDictionary<string, Snapshot> snapshots )
    {
        throw new NotImplementedException( );
    }

    /// <inheritdoc />
    public override Task<ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>> GetPoolRootsAndPropertyValiditiesAsync( )
    {
        throw new NotImplementedException( );
    }

    /// <inheritdoc />
    public override Task<ZfsCommandRunnerOperationStatus> InheritZfsPropertyAsync( bool dryRun, string zfsPath, IZfsProperty propertyToInherit )
    {
        throw new NotImplementedException( );
    }

    /// <inheritdoc />
    public override bool SetDefaultValuesForMissingZfsPropertiesOnPoolAsync( SnapsInAZfsSettings settings, string poolName, string[] propertyArray )
    {
        throw new NotImplementedException( );
    }

    /// <inheritdoc />
    public override IAsyncEnumerable<string> ZpoolExecEnumerator( string verb, string args )
    {
        throw new NotImplementedException( );
    }

    /// <inheritdoc />
    public override IAsyncEnumerable<string> ZfsExecEnumeratorAsync( string verb, string args )
    {
        throw new NotImplementedException( );
    }

    /// <inheritdoc />
    public override ZfsCommandRunnerOperationStatus TakeSnapshot( ZfsRecord ds, SnapshotPeriod period, in DateTimeOffset dateTimeOffset, SnapsInAZfsSettings snapsInAZfsSettings, FormattingSettings datasetFormattingSettings, out Snapshot? snapshot )
    {
        throw new NotImplementedException( );
    }

    /// <inheritdoc />
    public override Task<ZfsCommandRunnerOperationStatus> DestroySnapshotAsync( Snapshot snapshot, SnapsInAZfsSettings settings )
    {
        throw new NotImplementedException( );
    }

    /// <inheritdoc />
    public override Task<ZfsCommandRunnerOperationStatus> SetZfsPropertiesAsync( bool dryRun, string zfsPath, params IZfsProperty[] properties )
    {
        throw new NotImplementedException( );
    }

#endregion
}
