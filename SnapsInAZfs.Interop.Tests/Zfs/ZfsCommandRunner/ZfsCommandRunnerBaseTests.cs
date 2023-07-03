// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using SnapsInAZfs.Interop.Zfs.ZfsTypes;

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsCommandRunner;

[TestFixture]
public class ZfsCommandRunnerBaseTests
{
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

    [Test]
    [TestCaseSource( nameof( GetValidDatasetCases ), new object?[] { 8, 12, 5 } )]
    [TestCaseSource( nameof( GetIllegalDatasetCases ), new object?[] { 8, 12, 5 } )]
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
        string path = $"{GetValidZfsPathComponent( pathLength / pathDepth )}";
        for ( int i = 1; i < pathDepth; i++ )
        {
            path = $"{path}/{GetValidZfsPathComponent( pathLength / pathDepth )}";
        }

        Assert.That( ( ) => { ZfsRecord.ValidateName( ZfsPropertyValueConstants.FileSystem, path ); }, Throws.TypeOf<ArgumentOutOfRangeException>( ) );
    }

    [Test]
    [TestCaseSource( nameof( GetValidSnapshotCases ), new object?[] { 8, 12, 5 } )]
    [TestCaseSource( nameof( GetIllegalSnapshotCases ), new object?[] { 8, 12, 5 } )]
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
    [TestCase( "validPoolName", ExpectedResult = false )]
    [TestCase( "valid-Pool Name", ExpectedResult = false )]
    [TestCase( "validPoolName/validDatasetName", ExpectedResult = false )]
    [TestCase( "bad!PoolName", ExpectedResult = false )]
    [TestCase( "bad@PoolName", ExpectedResult = true, Description = "This case is a valid case of pool@snapshot" )]
    [TestCase( "bad#PoolName", ExpectedResult = false )]
    [TestCase( "bad/PoolName", ExpectedResult = false )]
    [TestCase( "bad@PoolName/", ExpectedResult = false )]
    [TestCase( "bad#PoolName/", ExpectedResult = false )]
    [TestCase( "bad/PoolName/", ExpectedResult = false )]
    public bool CheckSnapshotNameValidation( string name )
    {
        return ZfsRecord.ValidateName( "snapshot", name );
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
            names[currentIndex].Valid = valid;
        }
    }

    /// <summary>
    ///     Gets an array <paramref name="numberOfComponents" /> strings, of length <paramref name="pathComponentLength" />,
    ///     each composed of characters from <see cref="AllowedIdentifierComponentCharacters" />, except for one
    ///     randomly-selected
    ///     character that is from <see cref="IllegalIdentifierComponentCharacters" />
    /// </summary>
    /// <param name="pathComponentLength"></param>
    /// <param name="numberOfComponents"></param>
    /// <returns></returns>
    private static string[] GetBadZfsPathComponentArray( int pathComponentLength = 8, int numberOfComponents = 1 )
    {
        string[] names = new string[numberOfComponents];
        for ( int nameIndex = 0; nameIndex < numberOfComponents; nameIndex++ )
        {
            names[ nameIndex ] = GetIllegalZfsPathComponent( pathComponentLength );
        }

        return names;
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
    private static NameValidationTestCase[] GetIllegalDatasetCases( int pathsPerLevel = 3, int pathComponentLength = 8, int pathDepth = 3 )
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
    private static NameValidationTestCase[] GetIllegalSnapshotCases( int pathsPerLevel = 3, int pathComponentLength = 8, int pathDepth = 3 )
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
    private static NameValidationTestCase[] GetValidDatasetCases( int pathsPerLevel = 3, int pathComponentLength = 8, int pathDepth = 3 )
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
    private static NameValidationTestCase[] GetValidSnapshotCases( int pathsPerLevel = 3, int pathComponentLength = 8, int pathDepth = 3 )
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

    private static string GetValidZfsPathComponent( int pathComponentLength )
    {
        return TestContext.CurrentContext.Random.GetString( pathComponentLength, AllowedIdentifierComponentCharacters );
    }

    private static string[] GetValidZfsPathComponentArray( int pathComponentLength, int numberOfStrings )
    {
        string[] components = new string[numberOfStrings];
        for ( int nameIndex = 0; nameIndex < numberOfStrings; nameIndex++ )
        {
            components[ nameIndex ] = GetValidZfsPathComponent( pathComponentLength );
        }

        return components;
    }

    private static string GetValidZfsSnapshotNameComponent( int snapshotNameComponentLength )
    {
        return $"@{TestContext.CurrentContext.Random.GetString( snapshotNameComponentLength - 1, AllowedIdentifierComponentCharacters )}";
    }

    public record struct NameValidationTestCase( string Name, bool Valid );
}
