// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Globalization;
using System.Net.NetworkInformation;

using Microsoft.Extensions.Configuration;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Settings.Settings;

// These necessarily have non-private accessibility because this is a helper class..
#pragma warning disable NUnit1028

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes.ZfsRecordTests;

[SetUpFixture]
[Order( 1 )]
public class ZfsRecordTestHelpers
{
    public static SnapsInAZfsSettings Settings { get; set; }
    private static string GetHostName( )
    {
        IPGlobalProperties hostIpGlobalProps = IPGlobalProperties.GetIPGlobalProperties( );
        return $"{hostIpGlobalProps.HostName}.{hostIpGlobalProps.DomainName}";
    }

    internal static ZfsRecord GetNewTestRootFileSystem( string name = "testRoot" )
    {
        return ZfsRecord.CreateInstanceFromAllProperties( name,
                                                          ZfsPropertyValueConstants.FileSystem,
                                                          ZfsProperty<bool>.CreateWithoutParent( ZfsPropertyNames.EnabledPropertyName, true ),
                                                          ZfsProperty<bool>.CreateWithoutParent( ZfsPropertyNames.TakeSnapshotsPropertyName, true ),
                                                          ZfsProperty<bool>.CreateWithoutParent( ZfsPropertyNames.PruneSnapshotsPropertyName, true ),
                                                          ZfsProperty<DateTimeOffset>.CreateWithoutParent( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.ParseExact( "2023-07-03T01:15:00.0000000", "O", DateTimeFormatInfo.CurrentInfo ) ),
                                                          ZfsProperty<DateTimeOffset>.CreateWithoutParent( ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, DateTimeOffset.ParseExact( "2023-07-03T01:00:00.0000000", "O", DateTimeFormatInfo.CurrentInfo ) ),
                                                          ZfsProperty<DateTimeOffset>.CreateWithoutParent( ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, DateTimeOffset.ParseExact( "2023-07-03T00:00:00.0000000", "O", DateTimeFormatInfo.CurrentInfo ) ),
                                                          ZfsProperty<DateTimeOffset>.CreateWithoutParent( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, DateTimeOffset.ParseExact( "2023-07-03T00:00:00.0000000", "O", DateTimeFormatInfo.CurrentInfo ) ),
                                                          ZfsProperty<DateTimeOffset>.CreateWithoutParent( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, DateTimeOffset.ParseExact( "2023-07-03T00:00:00.0000000", "O", DateTimeFormatInfo.CurrentInfo ) ),
                                                          ZfsProperty<DateTimeOffset>.CreateWithoutParent( ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, DateTimeOffset.ParseExact( "2023-01-01T00:00:00.0000000", "O", DateTimeFormatInfo.CurrentInfo ) ),
                                                          ZfsProperty<string>.CreateWithoutParent( ZfsPropertyNames.RecursionPropertyName, ZfsPropertyValueConstants.SnapsInAZfs ),
                                                          ZfsProperty<string>.CreateWithoutParent( ZfsPropertyNames.TemplatePropertyName, ZfsPropertyValueConstants.Default ),
                                                          ZfsProperty<int>.CreateWithoutParent( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, 2 ),
                                                          ZfsProperty<int>.CreateWithoutParent( ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, 2 ),
                                                          ZfsProperty<int>.CreateWithoutParent( ZfsPropertyNames.SnapshotRetentionDailyPropertyName, 2 ),
                                                          ZfsProperty<int>.CreateWithoutParent( ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, 1 ),
                                                          ZfsProperty<int>.CreateWithoutParent( ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, 1 ),
                                                          ZfsProperty<int>.CreateWithoutParent( ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, 1 ),
                                                          ZfsProperty<int>.CreateWithoutParent( ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, 0 ),
                                                          ZfsProperty<string>.CreateWithoutParent( ZfsPropertyNames.SourceSystem, ZfsPropertyValueConstants.StandaloneSiazSystem ),
                                                          107374182400L, // 100 GiB
                                                          10737418240L );
    }

    /// <summary>All valid characters in a ZFS dataset identifier component</summary>
    private const string AllowedIdentifierComponentCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-:.";

    /// <summary>
    ///     All valid characters in a ZFS snapshot identifier component (same as
    ///     <see cref="AllowedIdentifierComponentCharacters" /> plus '@')
    /// </summary>
#pragma warning disable IDE0052 //I'm keeping this here for reference
    // ReSharper disable once UnusedMember.Local
    private const string AllowedSnapshotIdentifierComponentCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-:.@";
#pragma warning restore IDE0052

    /// <summary>
    ///     All illegal characters in a ZFS dataset identifier (values from 0 to 255, except those in
    ///     <see cref="AllowedIdentifierComponentCharacters" />)
    /// </summary>
    private const string IllegalIdentifierComponentCharacters = "\x0\x1\x2\x3\x4\x5\x6\x7\x8\x9\xA\xB\xC\xD\xE\xF\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1A\x1B\x1C\x1D\x1E\x1F\x20\x21\x22\x23\x24\x25\x26\x27\x28\x29\x2A\x2B\x2C\x2F\x30\x31\x32\x33\x34\x35\x36\x37\x38\x39\x3B\x3C\x3D\x3E\x3F\x40\x5B\x5C\x5D\x5E\x60\x7B\x7C\x7D\x7E\x7F\x80\x81\x82\x83\x84\x85\x86\x87\x88\x89\x8A\x8B\x8C\x8D\x8E\x8F\x90\x91\x92\x93\x94\x95\x96\x97\x98\x99\x9A\x9B\x9C\x9D\x9E\x9F\xA0\xA1\xA2\xA3\xA4\xA5\xA6\xA7\xA8\xA9\xAA\xAB\xAC\xAD\xAE\xAF\xB0\xB1\xB2\xB3\xB4\xB5\xB6\xB7\xB8\xB9\xBA\xBB\xBC\xBD\xBE\xBF\xC0\xC1\xC2\xC3\xC4\xC5\xC6\xC7\xC8\xC9\xCA\xCB\xCC\xCD\xCE\xCF\xD0\xD1\xD2\xD3\xD4\xD5\xD6\xD7\xD8\xD9\xDA\xDB\xDC\xDD\xDE\xDF\xE0\xE1\xE2\xE3\xE4\xE5\xE6\xE7\xE8\xE9\xEA\xEB\xEC\xED\xEE\xEF\xF0\xF1\xF2\xF3\xF4\xF5\xF6\xF7\xF8\xF9\xFA\xFB\xFC\xFD\xFE\xFF";

    /// <summary>
    ///     All illegal characters in a ZFS dataset identifier (values from 0 to 255, except those in
    ///     <see cref="AllowedSnapshotIdentifierComponentCharacters" />)
    /// </summary>
    private const string IllegalSnapshotIdentifierComponentCharacters = "\x0\x1\x2\x3\x4\x5\x6\x7\x8\x9\xA\xB\xC\xD\xE\xF\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1A\x1B\x1C\x1D\x1E\x1F\x20\x21\x22\x23\x24\x25\x26\x27\x28\x29\x2A\x2B\x2C\x2F\x30\x31\x32\x33\x34\x35\x36\x37\x38\x39\x3B\x3C\x3D\x3E\x3F\x5B\x5C\x5D\x5E\x60\x7B\x7C\x7D\x7E\x7F\x80\x81\x82\x83\x84\x85\x86\x87\x88\x89\x8A\x8B\x8C\x8D\x8E\x8F\x90\x91\x92\x93\x94\x95\x96\x97\x98\x99\x9A\x9B\x9C\x9D\x9E\x9F\xA0\xA1\xA2\xA3\xA4\xA5\xA6\xA7\xA8\xA9\xAA\xAB\xAC\xAD\xAE\xAF\xB0\xB1\xB2\xB3\xB4\xB5\xB6\xB7\xB8\xB9\xBA\xBB\xBC\xBD\xBE\xBF\xC0\xC1\xC2\xC3\xC4\xC5\xC6\xC7\xC8\xC9\xCA\xCB\xCC\xCD\xCE\xCF\xD0\xD1\xD2\xD3\xD4\xD5\xD6\xD7\xD8\xD9\xDA\xDB\xDC\xDD\xDE\xDF\xE0\xE1\xE2\xE3\xE4\xE5\xE6\xE7\xE8\xE9\xEA\xEB\xEC\xED\xEE\xEF\xF0\xF1\xF2\xF3\xF4\xF5\xF6\xF7\xF8\xF9\xFA\xFB\xFC\xFD\xFE\xFF";

    [OneTimeSetUp]
    public static void SetUpTestData( )
    {
        IConfigurationRoot rootConfiguration = new ConfigurationBuilder( )
                                               .AddJsonFile( "SnapsInAZfs.json", true, false )
                                               .AddJsonFile( "SnapsInAZfs.local.json", true, false )
                                               .Build( );
        Settings = rootConfiguration.Get<SnapsInAZfsSettings>( ) ?? throw new InvalidOperationException( );
    }

    private static void ComposeDatasetPathsAtLevel( int currentPathDepth, int subPaths, int pathComponentLength, bool valid, ref NameValidationTestCase[] names )
    {
        int pathStartIndex = subPaths * ( currentPathDepth - 1 );
        int pathEndIndex = pathStartIndex + subPaths - 1;

        for ( int currentIndex = pathStartIndex; currentIndex <= pathEndIndex; currentIndex++ )
        {
            names[ currentIndex ] = currentPathDepth switch
            {
                > 1 when valid => new( $"{names[ currentIndex - subPaths ].Name}/{GetValidZfsPathComponent( pathComponentLength - 1 )}", valid ),
                > 1 => new( $"{names[ currentIndex - subPaths ].Name}/{GetIllegalZfsPathComponent( pathComponentLength - 1 )}", valid ),
                1 when valid => new( GetValidZfsPathComponent( pathComponentLength ), valid ),
                1 => new( GetIllegalZfsPathComponent( pathComponentLength ), valid ),
                _ => throw new ArgumentOutOfRangeException( nameof( currentPathDepth ), currentPathDepth, "Unexpected value for currentPathDepth passed to ComposeDatasetPathsAtLevel. Must be > 0" )
            };
        }
    }

    /// <summary>
    ///     Populates the passed array with valid or invalid fully-qualified snapshot paths, as requested.
    /// </summary>
    /// <param name="currentPathDepth"></param>
    /// <param name="subPaths"></param>
    /// <param name="pathComponentLength"></param>
    /// <param name="valid"></param>
    /// <param name="names"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <remarks>
    ///     These tests are almost certain to fail on windows, as the Path.GetDirectoryName function will use \ instead of / in
    ///     paths.<br />
    ///     Does not test valid or invalid snapshot names against illegal parent dataset names, but that's ok because
    ///     those are already fully tested by other tests.
    /// </remarks>
    private static void ComposeSnapshotPathsAtLevel( int currentPathDepth, int subPaths, int pathComponentLength, bool valid, ref NameValidationTestCase[] names )
    {
        int pathStartIndex = subPaths * ( currentPathDepth - 1 );
        int pathEndIndex = pathStartIndex + subPaths - 1;

        for ( int currentIndex = pathStartIndex; currentIndex <= pathEndIndex; currentIndex++ )
        {
            names[ currentIndex ].Name += valid switch
            {
                true => GetValidZfsSnapshotNameComponent( pathComponentLength ),
                _ => GetIllegalZfsSnapshotNameComponent( pathComponentLength )
            };
            names[ currentIndex ].Valid = valid;
        }
    }

    private static string GetIllegalZfsPathComponent( int pathComponentLength )
    {
        string goodComponent = GetValidZfsPathComponent( pathComponentLength - 1 );
        int randomIndexToAlter = TestContext.CurrentContext.Random.Next( 0, pathComponentLength );
        string badComponent = goodComponent.Insert( randomIndexToAlter, TestContext.CurrentContext.Random.GetString( 1, IllegalIdentifierComponentCharacters ) );
        return badComponent;
    }

    private static string GetIllegalZfsSnapshotNameComponent( int snapshotNameComponentLength )
    {
        string goodComponent = GetValidZfsSnapshotNameComponent( snapshotNameComponentLength - 1 );
        int randomIndexToAlter = TestContext.CurrentContext.Random.Next( 0, snapshotNameComponentLength - 2 );
        string badComponent = goodComponent.Insert( randomIndexToAlter, TestContext.CurrentContext.Random.GetString( 1, IllegalSnapshotIdentifierComponentCharacters ) );
        return $"@{badComponent}";
    }

    private static string GetValidZfsSnapshotNameComponent( int snapshotNameComponentLength )
    {
        return $"@{TestContext.CurrentContext.Random.GetString( snapshotNameComponentLength - 1, AllowedIdentifierComponentCharacters )}";
    }

    /// <summary>
    ///     This gets an array of fully-qualified snapshot names that are guaranteed invalid
    /// </summary>
    /// <param name="pathsPerLevel">The number of strings to generate for each path level</param>
    /// <param name="pathComponentLength">The pathComponentLength of each path component</param>
    /// <param name="pathDepth">How many levels should be generated</param>
    /// <returns>
    ///     An array of <paramref name="pathsPerLevel" /> * <paramref name="pathDepth" /> strings, each representing an invalid
    ///     ZFS dataset identifier, with <paramref name="pathsPerLevel" /> datasets at each level, up to
    ///     <paramref name="pathDepth" />
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     If the total length of the longest possible string generated with the
    ///     supplied parameters would exceed ZFS maximum identifier length (255)
    /// </exception>
    public static NameValidationTestCase[] GetIllegalSnapshotCases( int pathsPerLevel = 3, int pathComponentLength = 8, int pathDepth = 3 )
    {
        NameValidationTestCase[] cases = new NameValidationTestCase[pathsPerLevel * pathDepth];

        for ( int currentPathDepth = 1; currentPathDepth <= pathDepth; currentPathDepth++ )
        {
            ComposeDatasetPathsAtLevel( currentPathDepth, pathsPerLevel, pathComponentLength, true, ref cases );
        }

        for ( int currentPathDepth = 1; currentPathDepth <= pathDepth; currentPathDepth++ )
        {
            ComposeSnapshotPathsAtLevel( currentPathDepth, pathsPerLevel, pathComponentLength, false, ref cases );
        }

        return cases;
    }

    /// <summary>
    ///     This gets an array of fully-qualified snapshot names that are guaranteed valid
    /// </summary>
    /// <param name="pathsPerLevel">The number of strings to generate for each path level</param>
    /// <param name="pathComponentLength">The pathComponentLength of each path component</param>
    /// <param name="pathDepth">How many levels should be generated</param>
    /// <returns>
    ///     An array of <paramref name="pathsPerLevel" /> * <paramref name="pathDepth" /> strings, each representing a valid
    ///     ZFS dataset identifier, with <paramref name="pathsPerLevel" /> datasets at each level, up to
    ///     <paramref name="pathDepth" />
    /// </returns>
    /// <remarks>
    ///     This is non-exhaustive, because it does not include names that have spaces in them, but it is guaranteed to ALWAYS
    ///     return valid identifiers. Any failures indicate a problem with validation. Test cases that violate a path length
    ///     check will be ignored, instead of fail, since that indicates an invalid test case - not a failure.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     If the total length of the longest possible string generated with the
    ///     supplied parameters would exceed ZFS maximum identifier length (255)
    /// </exception>
    public static NameValidationTestCase[] GetValidSnapshotCases( int pathsPerLevel = 3, int pathComponentLength = 8, int pathDepth = 3 )
    {
        NameValidationTestCase[] cases = new NameValidationTestCase[pathsPerLevel * pathDepth];

        for ( int currentPathDepth = 1; currentPathDepth <= pathDepth; currentPathDepth++ )
        {
            ComposeDatasetPathsAtLevel( currentPathDepth, pathsPerLevel, pathComponentLength, true, ref cases );
        }

        for ( int currentPathDepth = 1; currentPathDepth <= pathDepth; currentPathDepth++ )
        {
            ComposeSnapshotPathsAtLevel( currentPathDepth, pathsPerLevel, pathComponentLength, true, ref cases );
        }

        return cases;
    }

    public static string GetValidZfsPathComponent( int pathComponentLength )
    {
        return TestContext.CurrentContext.Random.GetString( pathComponentLength, AllowedIdentifierComponentCharacters );
    }
#pragma warning restore NUnit1028
}
