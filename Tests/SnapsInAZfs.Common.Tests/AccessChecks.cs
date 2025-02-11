using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SnapsInAZfs.Interop.Libc.Enums;
using NativeMethods = SnapsInAZfs.Interop.Libc.NativeMethods;

namespace SnapsInAZfs.Common.Tests;

[TestFixture]
[Platform( Include = "Unix" )]
[Order( 100 )]
[NonParallelizable]
[Category( "General" )]
[Category( "Access" )]
public class AccessChecks
{
    [OneTimeSetUp]
    public void OneTimeSetup( )
    {
        string[] programNames = { "cp", "install", "ln", "mkdir", "mv", "rm", "zfs" };
        foreach ( string programName in programNames )
        {
            ProcessStartInfo whichStartInfo = new( "which", programName )
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };
            using ( Process? whichProcess = Process.Start( whichStartInfo ) )
            {
                string? programPath = whichProcess?.StandardOutput.ReadToEnd( );
                whichProcess?.WaitForExit( 1000 );
                ProgramPathDictionary.TryAdd( programName, programPath!.Trim( ) );
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
    [TestCase( "rm" )]
    [TestCase( "zfs" )]
    [TestCase( "zpool" )]
    public void CheckUserCanExecute( string command )
    {
        string programPath = ProgramPathDictionary[ command ];
        Console.Write( $"Checking if user can execute {programPath}: " );
        int returnValue = NativeMethods.EuidAccess( programPath, UnixFileTestMode.Execute );
        Console.Write( returnValue == 0 ? "yes" : "no" );
        Assert.That( returnValue, Is.EqualTo( 0 ), GetExceptionMessageForExecuteCheck( programPath ) );
    }

    /// <summary>
    ///     This is in a separate method to prevent the call to GetLastPInvokeError unless the test actually fails.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    private static string? GetExceptionMessageForExecuteCheck( string command )
    {
        return $"User cannot execute {command}. Error: {(Errno)Marshal.GetLastPInvokeError( )}";
    }

    [Test]
    [Order( 2 )]
    [TestCase( "." )]
    [TestCase( "/etc" )]
    [TestCase( "/etc/SnapsInAZfs" )]
    [TestCase( "/usr/local" )]
    [TestCase( "/usr/local/bin" )]
    [TestCase( "/usr/local/share" )]
    [TestCase( "/usr/local/share/SnapsInAZfs" )]
    public void CheckUserCanWriteTo( string path )
    {
        string canonicalPath = NativeMethods.CanonicalizeFileName( path );
        Console.Write( $"Checking if user can write to {canonicalPath}: " );
        int returnValue = NativeMethods.EuidAccess( canonicalPath, UnixFileTestMode.Write );
        Console.Write( returnValue == 0 ? "yes" : "no" );
        Assert.That( returnValue, Is.EqualTo( 0 ), GetExceptionMessageForWriteCheck( canonicalPath ) );
    }

    private static string? GetExceptionMessageForWriteCheck( string path )
    {
        return $"User cannot write to {path}. Error: {(Errno)Marshal.GetLastPInvokeError( )}";
    }
}
