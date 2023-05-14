// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Sanoid.Common.Posix;

namespace Sanoid.Common.Tests;

[TestFixture]
[Platform( Exclude = "WIN32NT" )]
[Explicit( "These tests involve creating and deleting files, zpools, datasets, zvols, and snapshots." )]
[Category( "Dangerous" )]
public class DestructivePrerequisiteTests
{
    [OneTimeSetUp]
    public void OneTimeSetup( )
    {
        string[] programNames = { "cp", "install", "ln", "mkdir", "mv", "pwd", "rm", "zfs", "zpool" };
        foreach ( string programName in programNames )
        {
            ProcessStartInfo whichStartInfo = new( "which", programName )
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };
            using ( Process? whichProcess = Process.Start( whichStartInfo ) )
            {
                string programPath = whichProcess.StandardOutput.ReadToEnd( );
                whichProcess?.WaitForExit( 1000 );
                ProgramPathDictionary.TryAdd( programName, programPath.Trim( ) );
            }
        }
    }

    private static readonly ConcurrentDictionary<string, string> ProgramPathDictionary = new( );

    [Test]
    [Order( 1 )]
    [TestCase( "cp" )]
    [TestCase( "install" )]
    [TestCase( "ln" )]
    [TestCase( "mkdir" )]
    [TestCase( "mv" )]
    [TestCase( "pwd" )]
    [TestCase( "rm" )]
    [TestCase( "zfs" )]
    [TestCase( "zpool" )]
    public void CheckUserCanExecute( string command )
    {
        string programPath = ProgramPathDictionary[ command ];
        Console.Write( $"Checking if user can execute {programPath}: " );
        int returnValue = NativeFunctions.euidaccess( programPath, UnixFileTestFlags.CanExecute );
        Console.Write( returnValue == 0 ? "yes" : "no" );
        Assert.That( returnValue, Is.EqualTo( 0 ), GetExceptionMessageForExecuteCheck( programPath ) );
    }

    /// <summary>
    ///     This is in a separate method to prevent the call to GetLastPInvokeError unless the test actually fails.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    private string? GetExceptionMessageForExecuteCheck( string command )
    {
        return $"User cannot execute {command}. Error: {(Errno)Marshal.GetLastPInvokeError( )}";
    }

    [Test]
    [Order( 2 )]
    [TestCase( "." )]
    [TestCase( "/etc" )]
    [TestCase( "/etc/sanoid" )]
    [TestCase( "/usr/local" )]
    [TestCase( "/usr/local/bin" )]
    [TestCase( "/usr/local/share" )]
    [TestCase( "/usr/local/share/Sanoid.net" )]
    public void CheckUserCanWriteTo( string path )
    {
        string canonicalPath = NativeFunctions.canonicalize_file_name( path );
        Console.Write( $"Checking if user can write to {canonicalPath}: " );
        int returnValue = NativeFunctions.euidaccess( canonicalPath, UnixFileTestFlags.CanWrite );
        Console.Write( returnValue == 0 ? "yes" : "no" );
        Assert.That( returnValue, Is.EqualTo( 0 ), GetExceptionMessageForWriteCheck( canonicalPath ) );
    }

    private string? GetExceptionMessageForWriteCheck( string path )
    {
        return $"User cannot write to {path}. Error: {(Errno)Marshal.GetLastPInvokeError( )}";
    }

    [TestFixture]
    [Category( "Dangerous" )]
    public class ZfsCommands
    {
        [OneTimeSetUp]
        public void Setup( )
        {
        }
    }
}

public static class NativeFunctions
{
    [DllImport( "libc", CharSet = CharSet.Auto )]
    public static extern string canonicalize_file_name( string path );

    [DllImport( "libc", CharSet = CharSet.Auto, SetLastError = true )]
    public static extern int euidaccess( string pathname, UnixFileTestFlags mode );
}

[Flags]
public enum UnixFileTestFlags
{
    /// <summary>The file exists</summary>
    /// <remarks>Equivalent to F_OK in unistd.h</remarks>
    Exists = 0,

    /// <summary>Allowed to execute the file</summary>
    /// <remarks>Equivalent to X_OK in unistd.h</remarks>
    CanExecute = 1,

    /// <summary>Allowed to write to the file</summary>
    /// <remarks>Equivalent to W_OK in unistd.h</remarks>
    CanWrite = 2,

    /// <summary>Allowed to read the file</summary>
    /// <remarks>Equivalent to R_OK in unistd.h</remarks>
    CanRead = 4
}
