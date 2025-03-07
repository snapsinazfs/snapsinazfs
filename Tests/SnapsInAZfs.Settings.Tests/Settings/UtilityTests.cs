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

// @formatter:place_simple_method_on_single_line false
namespace SnapsInAZfs.Settings.Tests.Settings;

using NUnit.Framework.Internal;

[TestFixture ( Category = "Utilities", TestOf = typeof (Utility) )]
[Parallelizable ( ParallelScope.All )]
[FixtureLifeCycle ( LifeCycle.InstancePerTestCase )]
public class UtilityTests
{
    private static          string?              _originalPathEnvVar;
    private static readonly ReaderWriterLockSlim EnvironmentPathVariableLock      = new ( );
    private static readonly char                 EnvironmentPathVariableSeparator = OSPlatform.CurrentPlatform.IsWindows ? ';' : ':';

    [OneTimeTearDown]
    public void ResetPathEnvironmentVariable ( )
    {
        EnvironmentPathVariableLock.EnterWriteLock ( );
        Environment.SetEnvironmentVariable ( "PATH", _originalPathEnvVar, EnvironmentVariableTarget.Process );
        EnvironmentPathVariableLock.ExitWriteLock ( );
    }

    [OneTimeSetUp]
    [System.Diagnostics.CodeAnalysis.SuppressMessage ( "Parallelization",
                                                       "ParallelChecker:Issue: #0 Data race on SnapsInAZfs.Settings.Tests.Settings.UtilityTests._originalPathEnvVar",
                                                       Justification = "This race does not exist." )]
    public void SaveOriginalPathEnvironmentVariable ( )
    {
        EnvironmentPathVariableLock.EnterReadLock ( );
        _originalPathEnvVar ??= Environment.GetEnvironmentVariable ( "PATH", EnvironmentVariableTarget.Process ) ?? string.Empty;
        EnvironmentPathVariableLock.ExitReadLock ( );
    }

    [Test]
    [Description ( "Creates a real file and a symlink to it, then expects that the method returns the path of the real file when given only the file name." )]
    public void Which_ReturnsQualifiedPathForSymLinkWithValidRealFileTarget ( [Values ( "zfs", "zpool" )] string input )
    {
        DirectoryInfo? fakeUtilitiesDirectory = null;
        DirectoryInfo? fakeUtilitySymlinksDirectory = null;

        try
        {
            DirectoryInfo testWorkDirectory = new ( TestContext.CurrentContext.WorkDirectory );
            fakeUtilitiesDirectory = testWorkDirectory.CreateSubdirectory ( "fakeUtilities" );
            Assume.That ( fakeUtilitiesDirectory.Exists );

            FileInfo fakeUtilityFile = new ( Path.Combine ( fakeUtilitiesDirectory.FullName, $"{input}_realFile" ) );
            Assume.That ( fakeUtilityFile, Does.Not.Exist );
            CreateAndCheckFakeUtilityNormalFile ( fakeUtilityFile );

            fakeUtilitySymlinksDirectory = testWorkDirectory.CreateSubdirectory ( "fakeUtilitySymlinks" );
            Assume.That ( fakeUtilitySymlinksDirectory.Exists );

            FileInfo fakeUtilitySymlink = new ( Path.Combine ( fakeUtilitySymlinksDirectory.FullName, input ) );
            Assume.That ( fakeUtilitySymlink, Does.Not.Exist );

            fakeUtilitySymlink.CreateAsSymbolicLink ( fakeUtilityFile.FullName );
            Assume.That ( fakeUtilitySymlink.Exists );
            Assume.That ( fakeUtilitySymlink.LinkTarget, Is.SamePath ( fakeUtilityFile.FullName ) );

            EnvironmentPathVariableLock.EnterWriteLock ( );
            Environment.SetEnvironmentVariable ( "PATH", string.Join ( EnvironmentPathVariableSeparator, fakeUtilitiesDirectory.FullName, fakeUtilitySymlinksDirectory.FullName ) );

            FileInfo testFile = new ( Utility.Which ( input ) );

            Environment.SetEnvironmentVariable ( "PATH", _originalPathEnvVar );
            EnvironmentPathVariableLock.ExitWriteLock ( );

            Assert.Multiple ( ( ) =>
                              {
                                  Assert.That ( testFile.FullName,   Is.SamePath ( fakeUtilityFile.FullName ) );
                                  Assert.That ( testFile.LinkTarget, Is.Null );
                              } );
        }
        finally
        {
            if ( EnvironmentPathVariableLock.IsWriteLockHeld )
            {
                EnvironmentPathVariableLock.ExitWriteLock ( );
            }

            if ( fakeUtilitySymlinksDirectory is { Exists: true } )
            {
                fakeUtilitySymlinksDirectory.Delete ( true );
            }

            if ( fakeUtilitiesDirectory is { Exists: true } )
            {
                fakeUtilitiesDirectory.Delete ( true );
            }
        }
    }

    [Test]
    [Description ( "Creates a real file and then expects that the method returns the same path when given only the file name." )]
    public void Which_ReturnsQualifiedPathForValidRealFileInput ( [Values ( "zfs", "zpool" )] string input )
    {
        DirectoryInfo? fakeUtilitiesDirectory       = null;
        DirectoryInfo? fakeUtilitySymlinksDirectory = null;

        try
        {
            DirectoryInfo testWorkDirectory = new ( TestContext.CurrentContext.WorkDirectory );
            fakeUtilitiesDirectory = testWorkDirectory.CreateSubdirectory ( "fakeUtilities" );
            Assume.That ( fakeUtilitiesDirectory.Exists );

            FileInfo fakeUtilityFile = new ( Path.Combine ( fakeUtilitiesDirectory.FullName, $"{input}_realFile" ) );
            Assume.That ( fakeUtilityFile, Does.Not.Exist );
            CreateAndCheckFakeUtilityNormalFile ( fakeUtilityFile );

            fakeUtilitySymlinksDirectory = testWorkDirectory.CreateSubdirectory ( "fakeUtilitySymlinks" );
            Assume.That ( fakeUtilitySymlinksDirectory.Exists );

            EnvironmentPathVariableLock.EnterWriteLock ( );
            Environment.SetEnvironmentVariable ( "PATH", string.Join ( EnvironmentPathVariableSeparator, fakeUtilitiesDirectory.FullName, fakeUtilitySymlinksDirectory.FullName ) );

            FileInfo testFile = new ( Utility.Which ( input ) );

            Environment.SetEnvironmentVariable ( "PATH", _originalPathEnvVar );
            EnvironmentPathVariableLock.ExitWriteLock ( );

            // Validate the string itself as a valid *path* string
            Assert.That ( testFile.FullName,   Is.SamePath ( fakeUtilityFile.FullName ) );
            Assert.That ( testFile.LinkTarget, Is.Null );
        }
        finally
        {
            if ( EnvironmentPathVariableLock.IsWriteLockHeld )
            {
                EnvironmentPathVariableLock.ExitWriteLock ( );
            }

            if ( fakeUtilitySymlinksDirectory is { Exists: true } )
            {
                fakeUtilitySymlinksDirectory.Delete ( true );
            }

            if ( fakeUtilitiesDirectory is { Exists: true } )
            {
                fakeUtilitiesDirectory.Delete ( true );
            }
        }
    }

    [Test]
    [NonParallelizable]
    [RequiresThread]
    [Category ( "Exceptions" )]
    public void Which_ThrowsApplicationExceptionIfEnvironmentPathVariableEmpty ( [Values ( "zfs", "zpool" )] string input )
    {
        try
        {
            EnvironmentPathVariableLock.EnterWriteLock ( );
            Environment.SetEnvironmentVariable ( "PATH", string.Empty, EnvironmentVariableTarget.Process );

            Assume.That ( Environment.GetEnvironmentVariable ( "PATH" ), Is.Null );

            Assert.That ( ( ) => Utility.Which ( input ), Throws.TypeOf<ApplicationException> ( ) );
            Environment.SetEnvironmentVariable ( "PATH", _originalPathEnvVar, EnvironmentVariableTarget.Process );
            EnvironmentPathVariableLock.ExitWriteLock ( );
        }
        finally
        {
            if ( EnvironmentPathVariableLock.IsWriteLockHeld )
            {
                EnvironmentPathVariableLock.ExitWriteLock ( );
            }
        }
    }

    [Test]
    [Category ( "Exceptions" )]
    public void Which_ThrowsArgumentExceptionForEmptyOrWhitespaceInput ( [ValueSource ( typeof (TestHelpers), nameof (TestHelpers.StandardEmptyAndWhitespaceStringTestInputs) )] string inputComponent1, [ValueSource ( typeof (TestHelpers), nameof (TestHelpers.StandardEmptyAndWhitespaceStringTestInputs) )] string inputComponent2 )
    {
        try
        {
            EnvironmentPathVariableLock.EnterReadLock ( );
            Assert.That ( ( ) => Utility.Which ( $"{inputComponent1}{inputComponent2}" ), Throws.ArgumentException );
        }
        finally
        {
            if ( EnvironmentPathVariableLock.IsReadLockHeld )
            {
                EnvironmentPathVariableLock.ExitReadLock ( );
            }
        }
    }

    [Test]
    [Category ( "Exceptions" )]
    public void Which_ThrowsArgumentNullExceptionForNullInput ( )
    {
        Assert.That ( static ( ) => Utility.Which ( null! ), Throws.ArgumentNullException );
    }

    [Test]
    [Category ( "Exceptions" )]
    public void Which_ThrowsArgumentOutOfRangeExceptionForIllegalNonEmptyInput ( [Values ( "bash", "rm", "cat", ".", "/", ";", "#" )] string input )
    {
        Assert.That ( ( ) => Utility.Which ( input ), Throws.TypeOf<ArgumentOutOfRangeException> ( ) );
    }

    [Test]
    [Category ( "Exceptions" )]
    public void Which_ThrowsFileNotFoundIfNotFound ( [Values ( "zfs", "zpool" )] string input )
    {
        DirectoryInfo? fakeUtilitiesDirectory = null;
        DirectoryInfo? fakeUtilitySymlinksDirectory = null;

        try
        {
            DirectoryInfo testWorkDirectory = new ( TestContext.CurrentContext.WorkDirectory );
            fakeUtilitiesDirectory = testWorkDirectory.CreateSubdirectory ( "fakeUtilities" );
            Assume.That ( fakeUtilitiesDirectory.Exists );

            FileInfo fakeUtilityFile = new ( Path.Combine ( fakeUtilitiesDirectory.FullName, $"{input}_realFile" ) );
            Assume.That ( fakeUtilityFile, Does.Not.Exist );

            fakeUtilitySymlinksDirectory = testWorkDirectory.CreateSubdirectory ( "fakeUtilitySymlinks" );
            Assume.That ( fakeUtilitySymlinksDirectory.Exists );

            FileInfo fakeUtilitySymlink = new ( Path.Combine ( fakeUtilitySymlinksDirectory.FullName, input ) );
            Assume.That ( fakeUtilitySymlink, Does.Not.Exist );

            EnvironmentPathVariableLock.EnterWriteLock ( );
            Environment.SetEnvironmentVariable ( "PATH", string.Join ( EnvironmentPathVariableSeparator, fakeUtilitiesDirectory.FullName, fakeUtilitySymlinksDirectory.FullName ) );

            Assert.That ( ( ) => Utility.Which ( input ), Throws.TypeOf<FileNotFoundException> ( ) );

            Environment.SetEnvironmentVariable ( "PATH", _originalPathEnvVar );
            EnvironmentPathVariableLock.ExitWriteLock ( );
        }
        finally
        {
            if ( EnvironmentPathVariableLock.IsWriteLockHeld )
            {
                EnvironmentPathVariableLock.ExitWriteLock ( );
            }

            if ( fakeUtilitySymlinksDirectory is { Exists: true } )
            {
                fakeUtilitySymlinksDirectory.Delete ( true );
            }

            if ( fakeUtilitiesDirectory is { Exists: true } )
            {
                fakeUtilitiesDirectory.Delete ( true );
            }
        }
    }

    /// <summary>
    ///     Creates the fake utility file specified, sets permissions, writes the content, and then checks that it worked using an Assume
    /// </summary>
    /// <param name="fakeUtilityFile">The file to create</param>
    private static void CreateAndCheckFakeUtilityNormalFile ( FileInfo fakeUtilityFile )
    {
        using ( StreamWriter fakeUtility = fakeUtilityFile.CreateText ( ) )
        {
            if ( !OperatingSystem.IsWindows ( ) )
            {
                // Setting file mode 0770
                fakeUtilityFile.UnixFileMode = UnixFileMode.UserRead
                                             | UnixFileMode.UserWrite
                                             | UnixFileMode.UserExecute
                                             | UnixFileMode.GroupRead
                                             | UnixFileMode.GroupWrite
                                             | UnixFileMode.GroupExecute;
            }

            // Write out a script with a /bin/true shebang so it does nothing and always succeeds.
            fakeUtility.Write ( "#!/bin/true\n\n#This is a test file. It can be deleted if left over after test execution completes.\n" );
        }

        // Make sure it's there, is a normal file, and is in the expected mode
        Assume.That ( fakeUtilityFile,            Does.Exist.IgnoreDirectories );
        Assume.That ( fakeUtilityFile.LinkTarget, Is.Null );

        if ( !OperatingSystem.IsWindows ( ) )
        {
            Assume.That (
                         fakeUtilityFile.UnixFileMode,
                         Is.EqualTo (
                                     UnixFileMode.UserRead
                                   | UnixFileMode.UserWrite
                                   | UnixFileMode.UserExecute
                                   | UnixFileMode.GroupRead
                                   | UnixFileMode.GroupWrite
                                   | UnixFileMode.GroupExecute ) );
        }
    }
}

// @formatter:place_simple_method_on_single_line restore
