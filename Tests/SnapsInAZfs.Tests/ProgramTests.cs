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

namespace SnapsInAZfs.Tests;

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Interop.Zfs.ZfsCommandRunner;
using Microsoft.Extensions.Configuration;
using NLog;
using PowerArgs;
using SnapsInAZfs.ConfigConsole;

[TestFixture]
[TestOf ( typeof( Program ) )]
public class ProgramTests
{
    [Test]
    [TestCase ( [ "SnapsInAZfs.json", "SnapsInAZfs.local.json", "fakeMonitoringSettingsForRoundTripTest.json" ] )]
    [TestCase ( [ "CombinedConfigurationForRoundTripTest.json" ] )]
    public void LoadConfigurationFromConfigurationFiles_RoundTripSafe( params string[] filePaths )
    {
        Assume.That ( filePaths.Length > 0 );

        foreach ( string filePath in filePaths )
        {
            Assume.That ( filePath, Does.Exist );
        }

        ConfigurationBuilder builder = new ( );
        builder.AddJsonFile ( "SnapsInAZfs.json",                            true, false );
        builder.AddJsonFile ( "SnapsInAZfs.local.json",                      true, false );
        builder.AddJsonFile ( "fakeMonitoringSettingsForRoundTripTest.json", true, false );
        IConfigurationRoot configurationRoot = builder.Build( );
        string             serializedJson    = configurationRoot.SerializeToJson( )!.ToJsonString ( new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull } );
        string             expectedJson      = File.ReadAllText ( "CombinedConfigurationForRoundTripTest.json" );
        Assert.That ( serializedJson, Is.EqualTo ( expectedJson ) );
    }

    [SetUp]
    public void SetUpCleanProgramRun ( )
    {
        Program.Settings                  = null;
        Program.ZfsCommandRunnerSingleton = null;
        ResetNLogToNoOutput( );
    }

    [Test]
    [NonParallelizable]
    [RequiresThread ( ApartmentState.MTA )]
    public void TryGetZfsCommandRunner_CanGetSingleton ( )
    {
      SnapsInAZfsSettings initialSettings = new ( ) { ZpoolPath = ";", ZfsPath = ";" };
        Assume.That ( Program.ZfsCommandRunnerSingleton, Is.Null );
        bool createSingletonResult = Program.TryGetZfsCommandRunner<ZfsCommandRunner> ( initialSettings, out IZfsCommandRunner zfsCommandRunnerA );
        Assume.That ( createSingletonResult,             Is.True );
        Assume.That ( Program.ZfsCommandRunnerSingleton, Is.Not.Null );
        bool getSingletonResult = Program.TryGetZfsCommandRunner<ZfsCommandRunner> ( initialSettings, out IZfsCommandRunner zfsCommandRunnerB );

        Assert.Multiple ( ( ) =>
                          {
                              Assert.That ( getSingletonResult, Is.True );
                              Assert.That ( zfsCommandRunnerB,  Is.SameAs ( zfsCommandRunnerA ) );
                              Assert.That ( zfsCommandRunnerB,  Is.SameAs ( Program.ZfsCommandRunnerSingleton ) );
                          }
                        );
    }

    [Test]
    public void TryGetZfsCommandRunner_DoesNotCreateSingletonWhenReuseSingletonFalse ( )
    {
      SnapsInAZfsSettings initialSettings = new ( ) { ZpoolPath = ";", ZfsPath = ";" };
        Assume.That ( Program.ZfsCommandRunnerSingleton, Is.Null );
        bool createSingletonResult = Program.TryGetZfsCommandRunner<ZfsCommandRunner> ( initialSettings, out IZfsCommandRunner zfsCommandRunnerA, false );
        Assume.That ( createSingletonResult, Is.True );

        Assert.Multiple ( ( ) =>
                          {
                              Assert.That ( Program.ZfsCommandRunnerSingleton, Is.Null );
                              Assert.That ( zfsCommandRunnerA,                 Is.Not.Null );
                          }
                        );
        bool getSingletonResult = Program.TryGetZfsCommandRunner<ZfsCommandRunner> ( initialSettings, out IZfsCommandRunner zfsCommandRunnerB, false );

        Assert.Multiple ( ( ) =>
                          {
                              Assert.That ( getSingletonResult,                Is.True );
                              Assert.That ( zfsCommandRunnerB,                 Is.Not.Null );
                              Assert.That ( zfsCommandRunnerB,                 Is.Not.SameAs ( zfsCommandRunnerA ) );
                              Assert.That ( Program.ZfsCommandRunnerSingleton, Is.Null );
                          }
                        );
    }

    [Test]
    public void TryGetZfsCommandRunner_ReturnsFalseOnEmptyZfsPaths( [Values ( "", " ", "\t", "\n", "\r" )] string zfsPath )
    {
      SnapsInAZfsSettings initialSettings = new ( )
                                            {
                                              ZfsPath   = zfsPath,
                                              ZpoolPath = ";"
                                            };
        Assume.That ( Program.ZfsCommandRunnerSingleton, Is.Null );
        bool result = Program.TryGetZfsCommandRunner<ZfsCommandRunner> ( initialSettings, out _ );
        Assert.That ( result, Is.False );
    }

    [Test]
    public void TryGetZfsCommandRunner_ReturnsFalseOnEmptyZpoolPaths( [Values ( "", " ", "\t", "\n", "\r" )] string zpoolPath )
    {
      SnapsInAZfsSettings initialSettings = new ( )
                                            {
                                              ZfsPath   = ";",
                                              ZpoolPath = zpoolPath
                                            };
        Assume.That ( Program.ZfsCommandRunnerSingleton, Is.Null );
        bool result = Program.TryGetZfsCommandRunner<ZfsCommandRunner> ( initialSettings, out _ );
        Assert.That ( result, Is.False );
    }

    private static PropertyInfo[] GetSnapsInAZfsSettingsPropertyInfos ( )
    {
        return typeof( SnapsInAZfsSettings ).GetProperties( ).Where ( pi => pi.Name is not nameof (SnapsInAZfsSettings.Templates) and not nameof (SnapsInAZfsSettings.Monitoring) ).ToArray( );
    }

    private static void ResetNLogToNoOutput ( )
    {
        if ( LogManager.Configuration is not null )
        {
            LogManager.Shutdown( );
        }

        LogManager.Setup( ).LoadConfiguration ( builder => { builder.ForLogger( ).FilterLevels ( LogLevel.Trace, LogLevel.Off ).WriteToNil( ); } );
        LogManager.ReconfigExistingLoggers ( true );
    }
}
