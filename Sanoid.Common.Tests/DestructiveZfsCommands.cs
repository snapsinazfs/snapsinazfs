// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Diagnostics;
using System.Runtime.InteropServices;
using Sanoid.Interop.Libc.Enums;
using NativeMethods = Sanoid.Interop.Libc.NativeMethods;

namespace Sanoid.Common.Tests;

[TestFixture]
[Explicit( "These tests involve creating and deleting files, zpools, datasets, zvols, and snapshots." )]
[Category( "Dangerous" )]
[Order( 101 )]
[NonParallelizable]
public class DestructiveZfsCommands
{
    [OneTimeSetUp]
    public void Setup( )
    {
        _thisDirectory = NativeMethods.CanonicalizeFileName( "." ).Trim( );
        _zpoolFileName = Path.Combine( _thisDirectory, "Sanoid.net.test.zpool" );
    }

    [OneTimeTearDown]
    public void Teardown( )
    {
        if ( _state.HasFlag( TestState.ZpoolDestroyed ) && _state.HasFlag( TestState.ZpoolFileCreated ) )
        {
            NativeMethods.Unlink( _zpoolFileName );
        }
    }

    private static string _thisDirectory;
    private static string _zpoolFileName;
    private const string ZpoolName = "SanoidDotnetTestZpool";

    private static TestState _state = TestState.Init;

    [Test]
    [Order( 1 )]
    public void CreateTestZpoolFile( )
    {
        // I'd love to put this in the setup, but there's no console output there, so I'm putting this and
        // other critical operations in tests, so that problems can be reported to the user

        // Create a 512MB sparse file (if the file system supports it) that we will make our test zpool on
        int zpoolFileDescriptor = NativeMethods.Open( _zpoolFileName, UnixFileFlags.O_CREAT | UnixFileFlags.O_TRUNC | UnixFileFlags.O_WRONLY, UnixFileMode.UserRead | UnixFileMode.UserWrite );
        if ( zpoolFileDescriptor > 0 )
        {
            _state |= TestState.ZpoolFileCreated;
            int closeReturn = NativeMethods.Close( zpoolFileDescriptor );
            if ( closeReturn == 0 )
            {
                _state |= TestState.ZpoolFileClosed;
                int truncateReturn = NativeMethods.Truncate( _zpoolFileName, 536870912L );
                if ( truncateReturn == 0 )
                {
                    _state |= TestState.ZpoolFileTruncated;
                }
            }
        }
        else
        {
            Console.WriteLine( "Error: " + (Errno)Marshal.GetLastPInvokeError( ) );
        }

        Assert.That( _state, Is.EqualTo( TestState.Stage1Complete ) );
    }

    [Test]
    [Order( 2 )]
    public void CreateTestZpool( )
    {
        if ( !_state.HasFlag( TestState.ZpoolFileCreated ) )
        {
            Assert.Inconclusive( "Previous step failed." );
            return;
        }

        Console.WriteLine( $"Creating zpool {ZpoolName} on {_zpoolFileName}" );
        ProcessStartInfo zpoolStartInfo = new( "zpool", $"create {ZpoolName} {_zpoolFileName}" )
        {
            CreateNoWindow = true
        };

        using ( Process? zpoolProcess = Process.Start( zpoolStartInfo ) )
        {
            zpoolProcess?.WaitForExit( 10000 );
            if ( zpoolProcess?.ExitCode == 0 )
            {
                _state |= TestState.ZpoolCreatedOnTestFile;
            }
        }

        Assert.That( _state, Is.EqualTo( TestState.Stage2Complete ) );
    }

    [Test]
    [Order( 3 )]
    public void CreateTestDataset( )
    {
        if ( !_state.HasFlag( TestState.ZpoolCreatedOnTestFile ) )
        {
            Assert.Inconclusive("Zpool doesn't exist.");
            return;
        }
        Console.WriteLine( $"Creating dataset {ZpoolName}/Dataset1" );
        ProcessStartInfo zfsProcess = new( "zfs", $"create {ZpoolName}/Dataset1" )
        {
            CreateNoWindow = true
        };

        using ( Process? zpoolProcess = Process.Start( zfsProcess ) )
        {
            zpoolProcess?.WaitForExit( 10000 );
            if ( zpoolProcess?.ExitCode == 0 )
            {
                Console.WriteLine( $"Dataset {ZpoolName}/Dataset1 created" );
                _state |= TestState.DatasetCreated;
            }
        }

        Assert.That( _state, Is.EqualTo( TestState.Stage3Complete ) );
    }

    [Test]
    [Order( 4 )]
    public void CreateSnapshotOnTestPool( )
    {
        if ( !_state.HasFlag( TestState.DatasetCreated ) )
        {
            Assert.Inconclusive( "Dataset doesn't exist." );
            return;
        }
        Console.WriteLine( $"Creating snapshot {ZpoolName}/Dataset1@snapshot1" );
        ProcessStartInfo zpoolStartInfo = new( "zfs", $"snapshot {ZpoolName}/Dataset1@snapshot1" )
        {
            CreateNoWindow = true
        };

        using ( Process? zfsProcess = Process.Start( zpoolStartInfo ) )
        {
            zfsProcess?.WaitForExit( 10000 );
            if ( zfsProcess?.ExitCode == 0 )
            {
                Console.WriteLine( $"Snapshot {ZpoolName}/Dataset1@snapshot1 created" );
                _state |= TestState.SnapshotCreated;
            }
        }

        Assert.That( _state, Is.EqualTo( TestState.Stage4Complete ) );
    }

    [Test]
    [Order( 5 )]
    public void DeleteSnapshotFromTestPool( )
    {
        if ( !_state.HasFlag( TestState.SnapshotCreated ) )
        {
            Assert.Inconclusive( "Snapshot doesn't exist." );
            return;
        }
        Console.WriteLine( $"Destroying snapshot {ZpoolName}/Dataset1@snapshot1" );
        ProcessStartInfo zpoolStartInfo = new( "zfs", $"destroy {ZpoolName}/Dataset1@snapshot1" )
        {
            CreateNoWindow = true
        };

        using ( Process? zfsProcess = Process.Start( zpoolStartInfo ) )
        {
            zfsProcess?.WaitForExit( 10000 );
            if ( zfsProcess?.ExitCode == 0 )
            {
                Console.WriteLine( $"Snapshot {ZpoolName}/Dataset1@snapshot1 destroyed" );
                _state |= TestState.SnapshotDestroyed;
            }
        }

        Assert.That( _state, Is.EqualTo( TestState.Stage5Complete ) );
    }

    [Test]
    [Order( 99 )]
    public void DestroyTestZpool( )
    {
        if ( !_state.HasFlag( TestState.ZpoolCreatedOnTestFile ) )
        {
            Assert.Inconclusive( "Zpool doesn't exist." );
            return;
        }

        Console.WriteLine( $"Destroying zpool {ZpoolName} on {_zpoolFileName}" );
        ProcessStartInfo zpoolStartInfo = new( "zpool", $"destroy -f {ZpoolName}" )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true
        };

        using ( Process? zpoolProcess = Process.Start( zpoolStartInfo ) )
        {
            zpoolProcess?.WaitForExit( 10000 );
            if ( zpoolProcess?.ExitCode == 0 )
            {
                Console.WriteLine( $"zpool {ZpoolName} destroyed" );
                _state |= TestState.ZpoolDestroyed;
            }
        }

        Assert.That( _state, Is.EqualTo( TestState.FinalStageComplete ) );
    }

    [Flags]
    private enum TestState
    {
        Init = 0,

        ZpoolFileCreated = 1,
        ZpoolFileClosed = 1 << 1,
        ZpoolFileTruncated = 1 << 2,
        Stage1Complete = ZpoolFileCreated | ZpoolFileClosed | ZpoolFileTruncated,
        ZpoolCreatedOnTestFile = 1 << 3,
        Stage2Complete = Stage1Complete | ZpoolCreatedOnTestFile,
        DatasetCreated = 1 << 4,
        Stage3Complete = Stage2Complete | DatasetCreated,
        SnapshotCreated = 1<< 5,
        Stage4Complete = Stage3Complete | SnapshotCreated,
        SnapshotDestroyed = 1 << 6,
        Stage5Complete = Stage4Complete | SnapshotDestroyed,
        ZpoolDestroyed = 1 << 16,
        FinalStageComplete = Stage5Complete | ZpoolDestroyed,
    }

}
