// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes;

[SetUpFixture]
[FixtureLifeCycle( LifeCycle.SingleInstance )]
[Order( 1 )]
public class ZfsRecordTestData
{
    private static readonly List<UpdatePropertyTestCase> UpdatePropertyTestCasesField = new( );

    public static Dictionary<string, ZfsRecord> OriginalValidDatasets { get; } = new( );
    public static Dictionary<string, Snapshot> OriginalValidSnapshots { get; } = new( );

    public static SnapsInAZfsSettings Settings { get; set; }

    public static List<UpdatePropertyTestCase> UpdatePropertyTestCases
    {
        get
        {
            if ( UpdatePropertyTestCasesField.Count != 0 )
            {
                return UpdatePropertyTestCasesField;
            }

            // ReSharper disable once AsyncConverter.AsyncWait
            UpdatePropertyTestCasesField.AddRange( UpdatePropertyTestCases_Bool );
            UpdatePropertyTestCasesField.AddRange( UpdatePropertyTestCases_DateTimeOffset );
            UpdatePropertyTestCasesField.AddRange( UpdatePropertyTestCases_Int );
            UpdatePropertyTestCasesField.AddRange( UpdatePropertyTestCases_String );
            return UpdatePropertyTestCasesField;
        }
    }

    private static IEnumerable<UpdatePropertyTestCase> UpdatePropertyTestCases_Bool { get; } = IZfsProperty.DefaultDatasetProperties.Values.OfType<ZfsProperty<bool>>( ).Select( p => new UpdatePropertyTestCase( p.Name, p with { Value = !p.Value } ) );
    private static IEnumerable<UpdatePropertyTestCase> UpdatePropertyTestCases_DateTimeOffset { get; } = IZfsProperty.DefaultDatasetProperties.Values.OfType<ZfsProperty<DateTimeOffset>>( ).Select( p => new UpdatePropertyTestCase( p.Name, p with { Value = p.Value.AddMinutes( 15 ) } ) );
    private static IEnumerable<UpdatePropertyTestCase> UpdatePropertyTestCases_Int { get; } = IZfsProperty.DefaultDatasetProperties.Values.OfType<ZfsProperty<int>>( ).Select( p => new UpdatePropertyTestCase( p.Name, p with { Value = p.Value + 100 } ) );
    private static IEnumerable<UpdatePropertyTestCase> UpdatePropertyTestCases_String { get; } = IZfsProperty.DefaultDatasetProperties.Values.OfType<ZfsProperty<string>>( ).Select( p => new UpdatePropertyTestCase( p.Name, p with { Value = $"{p.Value} MODIFIED" } ) );

    public static ZfsRecord StandardValidTestFileSystem { get; } =
        new(
            "testroot",
            ZfsPropertyValueConstants.FileSystem,
            new( ZfsPropertyNames.EnabledPropertyName, true, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.TakeSnapshotsPropertyName, true, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.PruneSnapshotsPropertyName, true, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.RecursionPropertyName, ZfsPropertyValueConstants.SnapsInAZfs, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.TemplatePropertyName, "default", ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, 2, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, 2, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.SnapshotRetentionDailyPropertyName, 2, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, 1, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, 1, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, 1, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, 0, ZfsPropertySourceConstants.Local ),
            107374182400L, // 100 GiB
            10737418240L   // 10 GiB
        );

    public static ZfsRecord TestFileSystem { get; } =
        new(
            "testroot",
            ZfsPropertyValueConstants.FileSystem,
            new( ZfsPropertyNames.EnabledPropertyName, true, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.TakeSnapshotsPropertyName, true, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.PruneSnapshotsPropertyName, true, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.ParseExact( "2023-07-03T01:15:00.0000000-07:00", "O", DateTimeFormatInfo.CurrentInfo ), ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, DateTimeOffset.ParseExact( "2023-07-03T01:00:00.0000000-07:00", "O", DateTimeFormatInfo.CurrentInfo ), ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, DateTimeOffset.ParseExact( "2023-07-03T00:00:00.0000000-07:00", "O", DateTimeFormatInfo.CurrentInfo ), ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, DateTimeOffset.ParseExact( "2023-07-03T00:00:00.0000000-07:00", "O", DateTimeFormatInfo.CurrentInfo ), ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, DateTimeOffset.ParseExact( "2023-07-03T00:00:00.0000000-07:00", "O", DateTimeFormatInfo.CurrentInfo ), ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, DateTimeOffset.ParseExact( "2023-01-01T00:00:00.0000000-07:00", "O", DateTimeFormatInfo.CurrentInfo ), ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.RecursionPropertyName, ZfsPropertyValueConstants.SnapsInAZfs, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.TemplatePropertyName, "default", ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, 2, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, 2, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.SnapshotRetentionDailyPropertyName, 2, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, 1, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, 1, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, 1, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, 0, ZfsPropertySourceConstants.Local ),
            107374182400L, // 100 GiB
            10737418240L   // 10 GiB
        );

    public static readonly ZfsRecord StandardValidTestVolume = new(
        "validTestVolume",
        ZfsPropertyValueConstants.Volume,
        (ZfsProperty<bool>)IZfsProperty.DefaultDatasetProperties[ ZfsPropertyNames.EnabledPropertyName ],
        (ZfsProperty<bool>)IZfsProperty.DefaultDatasetProperties[ ZfsPropertyNames.TakeSnapshotsPropertyName ],
        (ZfsProperty<bool>)IZfsProperty.DefaultDatasetProperties[ ZfsPropertyNames.PruneSnapshotsPropertyName ],
        (ZfsProperty<DateTimeOffset>)IZfsProperty.DefaultDatasetProperties[ ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName ],
        (ZfsProperty<DateTimeOffset>)IZfsProperty.DefaultDatasetProperties[ ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName ],
        (ZfsProperty<DateTimeOffset>)IZfsProperty.DefaultDatasetProperties[ ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName ],
        (ZfsProperty<DateTimeOffset>)IZfsProperty.DefaultDatasetProperties[ ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName ],
        (ZfsProperty<DateTimeOffset>)IZfsProperty.DefaultDatasetProperties[ ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName ],
        (ZfsProperty<DateTimeOffset>)IZfsProperty.DefaultDatasetProperties[ ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName ],
        (ZfsProperty<string>)IZfsProperty.DefaultDatasetProperties[ ZfsPropertyNames.RecursionPropertyName ],
        (ZfsProperty<string>)IZfsProperty.DefaultDatasetProperties[ ZfsPropertyNames.TemplatePropertyName ],
        (ZfsProperty<int>)IZfsProperty.DefaultDatasetProperties[ ZfsPropertyNames.SnapshotRetentionFrequentPropertyName ],
        (ZfsProperty<int>)IZfsProperty.DefaultDatasetProperties[ ZfsPropertyNames.SnapshotRetentionHourlyPropertyName ],
        (ZfsProperty<int>)IZfsProperty.DefaultDatasetProperties[ ZfsPropertyNames.SnapshotRetentionDailyPropertyName ],
        (ZfsProperty<int>)IZfsProperty.DefaultDatasetProperties[ ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName ],
        (ZfsProperty<int>)IZfsProperty.DefaultDatasetProperties[ ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName ],
        (ZfsProperty<int>)IZfsProperty.DefaultDatasetProperties[ ZfsPropertyNames.SnapshotRetentionYearlyPropertyName ],
        (ZfsProperty<int>)IZfsProperty.DefaultDatasetProperties[ ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName ],
        107374182400L, // 100 GiB
        10737418240L   // 10 GiB
    );

    /// <summary>All valid characters in a ZFS dataset identifier component</summary>
    private const string AllowedIdentifierComponentCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-:.";

    /// <summary>
    ///     All valid characters in a ZFS snapshot identifier component (same as
    ///     <see cref="AllowedIdentifierComponentCharacters" /> plus '@')
    /// </summary>
    private const string AllowedSnapshotIdentifierComponentCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-:.@";

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

    /// <summary>
    ///     This gets an array of dataset names that are guaranteed valid
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
    public static NameValidationTestCase[] GetIllegalDatasetCases( int pathsPerLevel = 3, int pathComponentLength = 8, int pathDepth = 3 )
    {
        NameValidationTestCase[] cases = new NameValidationTestCase[pathsPerLevel * pathDepth];

        for ( int currentPathDepth = 1; currentPathDepth <= pathDepth; currentPathDepth++ )
        {
            ComposeDatasetPathsAtLevel( currentPathDepth, pathsPerLevel, pathComponentLength, false, ref cases );
        }

        return cases;
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
    ///     This gets an array of dataset names that are guaranteed valid
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
    public static NameValidationTestCase[] GetValidDatasetCases( int pathsPerLevel = 3, int pathComponentLength = 8, int pathDepth = 3 )
    {
        NameValidationTestCase[] cases = new NameValidationTestCase[pathsPerLevel * pathDepth];

        for ( int currentPathDepth = 1; currentPathDepth <= pathDepth; currentPathDepth++ )
        {
            ComposeDatasetPathsAtLevel( currentPathDepth, pathsPerLevel, pathComponentLength, true, ref cases );
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
}
