#region MIT LICENSE

// Copyright 2026 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/

#endregion

using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using PowerArgs;
using SnapsInAZfs.ConfigConsole;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Tests;

using CLA = CommandLineArgumentsReflectionHelpers;
using SS = SnapsInAZfsSettingsReflectionHelpers;

[TestFixture]
[TestOf ( typeof (Program) )]
public class ProgramTests
{
  [Test]
  [TestCaseSource ( nameof (GetCasesForApplyCommandLineArgumentOverrides_ExpectedChangesApplied) )]
  public void ApplyCommandLineArgumentOverrides_ExpectedChangesApplied<T> ( PropertyInfo argPropertyInfo, string[] argStrings, T argValue, PropertyInfo settingsPropertyInfo, T initialSettingValue, T expectedFinalSettingValue )
  {
    SnapsInAZfsSettings initialSettings         = new ( );
    SnapsInAZfsSettings possiblyChangedSettings = new ( );
    settingsPropertyInfo.SetValue ( initialSettings, initialSettingValue );
    settingsPropertyInfo.SetValue ( possiblyChangedSettings, initialSettingValue );
    CommandLineArguments testArgs = Args.Parse<CommandLineArguments> ( argStrings );
    Assume.That ( testArgs, Is.Not.Null );
    argPropertyInfo.SetValue ( testArgs, argValue );
    Assume.That ( settingsPropertyInfo.GetValue ( initialSettings ), Is.EqualTo ( initialSettingValue ) );
    Assume.That ( settingsPropertyInfo.GetValue ( initialSettings ), Is.EqualTo ( settingsPropertyInfo.GetValue ( possiblyChangedSettings ) ) );
    Assume.That ( argPropertyInfo.GetValue ( testArgs ), Is.EqualTo ( argValue ) );

    Program.ApplyCommandLineArgumentOverrides ( in testArgs, possiblyChangedSettings );
    Assert.That ( settingsPropertyInfo.GetValue ( possiblyChangedSettings ), Is.EqualTo ( expectedFinalSettingValue ) );
  }

  [Test]
  public void ApplyCommandLineArgumentOverrides_ExpectedChangesApplied_DryRun ( [Values] bool initialValue )
  {
    SnapsInAZfsSettings settingsObject = new ( )
                                         {
                                           DryRun = initialValue
                                         };
    CommandLineArguments testArgs = Args.Parse<CommandLineArguments> ( "--dry-run" );
    Assume.That ( testArgs.DryRun, Is.True );
    Assume.That ( settingsObject.DryRun, Is.EqualTo ( initialValue ) );
    Program.ApplyCommandLineArgumentOverrides ( in testArgs, settingsObject );
    Assert.That ( settingsObject.DryRun, Is.True );
  }

  [Test]
  public void ApplyCommandLineArgumentOverrides_ExpectedChangesApplied_NoArgsSpecified ( [ValueSource ( nameof (GetSnapsInAZfsSettingsPropertyInfos) )] PropertyInfo pi )
  {
    SnapsInAZfsSettings initialSettings         = new ( );
    SnapsInAZfsSettings possiblyChangedSettings = new ( );

    Assume.That ( pi.GetValue ( initialSettings ), Is.EqualTo ( pi.GetValue ( possiblyChangedSettings ) ) );

    CommandLineArguments testArgs = Args.Parse<CommandLineArguments> ( );
    Program.ApplyCommandLineArgumentOverrides ( in testArgs, possiblyChangedSettings );
    Assert.That ( pi.GetValue ( initialSettings ), Is.EqualTo ( pi.GetValue ( possiblyChangedSettings ) ) );
  }

  [Test]
  [TestCaseSource ( nameof (GetCasesForApplyCommandLineArgumentOverrides_Monitor_ExpectedChangesApplied) )]
  public void ApplyCommandLineArgumentOverrides_Monitor_ExpectedChangesApplied ( PropertyInfo argPropertyInfo, string[] argStrings, bool argValue, PropertyInfo settingsPropertyInfo, bool initialSettingValue, bool expectedFinalSettingValue )
  {
    SnapsInAZfsSettings initialSettings         = new ( );
    SnapsInAZfsSettings possiblyChangedSettings = new ( );
    settingsPropertyInfo.SetValue ( initialSettings.Monitoring, initialSettingValue );
    settingsPropertyInfo.SetValue ( possiblyChangedSettings.Monitoring, initialSettingValue );
    CommandLineArguments testArgs = Args.Parse<CommandLineArguments> ( argStrings );
    Assume.That ( testArgs, Is.Not.Null );
    argPropertyInfo.SetValue ( testArgs, argValue );
    Assume.That ( settingsPropertyInfo.GetValue ( initialSettings.Monitoring ), Is.EqualTo ( initialSettingValue ) );
    Assume.That ( settingsPropertyInfo.GetValue ( initialSettings.Monitoring ), Is.EqualTo ( settingsPropertyInfo.GetValue ( possiblyChangedSettings.Monitoring ) ) );
    Assume.That ( argPropertyInfo.GetValue ( testArgs ), Is.EqualTo ( argValue ) );

    Program.ApplyCommandLineArgumentOverrides ( in testArgs, possiblyChangedSettings );
    Assert.That ( settingsPropertyInfo.GetValue ( possiblyChangedSettings.Monitoring ), Is.EqualTo ( expectedFinalSettingValue ) );
  }

  [Test]
  [TestCase ( [ "SnapsInAZfs.json", "SnapsInAZfs.local.json", "fakeMonitoringSettingsForRoundTripTest.json" ] )]
  [TestCase ( [ "CombinedConfigurationForRoundTripTest.json" ] )]
  public void LoadConfigurationFromConfigurationFiles_RoundTripSafe ( params string[] filePaths )
  {
    Assume.That ( filePaths.Length > 0 );
    foreach ( string filePath in filePaths )
    {
      Assume.That ( filePath, Does.Exist );
    }

    ConfigurationBuilder builder = new ( );
    builder.AddJsonFile ( "SnapsInAZfs.json", true, false );
    builder.AddJsonFile ( "SnapsInAZfs.local.json", true, false );
    builder.AddJsonFile ( "fakeMonitoringSettingsForRoundTripTest.json", true, false );
    IConfigurationRoot configurationRoot = builder.Build ( );
    string             serializedJson    = configurationRoot.SerializeToJson ( )!.ToJsonString ( new ( ) { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull } );
    string             expectedJson      = File.ReadAllText ( "CombinedConfigurationForRoundTripTest.json" );
    Assert.That ( serializedJson, Is.EqualTo ( expectedJson ) );
  }

  [SetUp]
  public void SetUpCleanProgramRun ( )
  {
    Program.Settings                  = null;
    Program.ZfsCommandRunnerSingleton = null;
    ResetNLogToNoOutput ( );
  }

  [Test]
  [NonParallelizable]
  [RequiresThread ( ApartmentState.MTA )]
  public void TryGetZfsCommandRunner_CanGetSingleton ( )
  {
    SnapsInAZfsSettings initialSettings = new ( );
    Assume.That ( Program.ZfsCommandRunnerSingleton, Is.Null );
    bool createSingletonResult = Program.TryGetZfsCommandRunner ( initialSettings, out IZfsCommandRunner? zfsCommandRunnerA );
    Assume.That ( createSingletonResult, Is.True );
    Assume.That ( Program.ZfsCommandRunnerSingleton, Is.Not.Null );
    bool getSingletonResult = Program.TryGetZfsCommandRunner ( initialSettings, out IZfsCommandRunner? zfsCommandRunnerB );
    using ( Assert.EnterMultipleScope ( ) )
    {
      Assert.That ( getSingletonResult, Is.True );
      Assert.That ( zfsCommandRunnerB, Is.SameAs ( zfsCommandRunnerA ) );
      Assert.That ( zfsCommandRunnerB, Is.SameAs ( Program.ZfsCommandRunnerSingleton ) );
    }
  }

  [Test]
  public void TryGetZfsCommandRunner_DoesNotCreateSingletonWhenReuseSingletonFalse ( )
  {
    SnapsInAZfsSettings initialSettings = new ( );
    Assume.That ( Program.ZfsCommandRunnerSingleton, Is.Null );
    bool createSingletonResult = Program.TryGetZfsCommandRunner ( initialSettings, out IZfsCommandRunner? zfsCommandRunnerA, false );
    Assume.That ( createSingletonResult, Is.True );
    using ( Assert.EnterMultipleScope ( ) )
    {
      Assert.That ( Program.ZfsCommandRunnerSingleton, Is.Null );
      Assert.That ( zfsCommandRunnerA, Is.Not.Null );
    }

    bool getSingletonResult = Program.TryGetZfsCommandRunner ( initialSettings, out IZfsCommandRunner? zfsCommandRunnerB, false );
    using ( Assert.EnterMultipleScope ( ) )
    {
      Assert.That ( getSingletonResult, Is.True );
      Assert.That ( zfsCommandRunnerB, Is.Not.Null );
      Assert.That ( zfsCommandRunnerB, Is.Not.SameAs ( zfsCommandRunnerA ) );
      Assert.That ( Program.ZfsCommandRunnerSingleton, Is.Null );
    }
  }

  [Test]
  public void TryGetZfsCommandRunner_ReturnsFalseOnEmptyZfsPaths ( [Values ( "", " ", "\t", "\n", "\r" )] string zfsPath )
  {
    SnapsInAZfsSettings initialSettings = new ( )
                                          {
                                            ZfsPath = zfsPath
                                          };
    Assume.That ( Program.ZfsCommandRunnerSingleton, Is.Null );
    bool result = Program.TryGetZfsCommandRunner ( initialSettings, out _ );
    Assert.That ( result, Is.False );
  }

  [Test]
  public void TryGetZfsCommandRunner_ReturnsFalseOnEmptyZpoolPaths ( [Values ( "", " ", "\t", "\n", "\r" )] string zpoolPath )
  {
    SnapsInAZfsSettings initialSettings = new ( )
                                          {
                                            ZpoolPath = zpoolPath
                                          };
    Assume.That ( Program.ZfsCommandRunnerSingleton, Is.Null );
    bool result = Program.TryGetZfsCommandRunner ( initialSettings, out _ );
    Assert.That ( result, Is.False );
  }

  private static IEnumerable<TestCaseData> GetCasesForApplyCommandLineArgumentOverrides_ExpectedChangesApplied ( )
  {
    yield return new ( CLA.CronProperty, (string[])[ "--cron" ], true, SS.TakeSnapshotsProperty, true, true );
    yield return new ( CLA.CronProperty, (string[])[ "--cron" ], true, SS.TakeSnapshotsProperty, false, true );
    yield return new ( CLA.CronProperty, Array.Empty<string> ( ), false, SS.TakeSnapshotsProperty, true, true );
    yield return new ( CLA.CronProperty, Array.Empty<string> ( ), false, SS.TakeSnapshotsProperty, false, false );
    yield return new ( CLA.CronProperty, (string[])[ "--cron" ], true, SS.PruneSnapshotsProperty, true, true );
    yield return new ( CLA.CronProperty, (string[])[ "--cron" ], true, SS.PruneSnapshotsProperty, false, true );
    yield return new ( CLA.CronProperty, Array.Empty<string> ( ), false, SS.PruneSnapshotsProperty, true, true );
    yield return new ( CLA.CronProperty, Array.Empty<string> ( ), false, SS.PruneSnapshotsProperty, false, false );
    yield return new ( CLA.DryRunProperty, (string[])[ "--dry-run" ], true, SS.DryRunProperty, true, true );
    yield return new ( CLA.DryRunProperty, (string[])[ "--dry-run" ], true, SS.DryRunProperty, true, true );
    yield return new ( CLA.DryRunProperty, (string[])[ "--dry-run" ], true, SS.DryRunProperty, false, true );
    yield return new ( CLA.DryRunProperty, Array.Empty<string> ( ), false, SS.DryRunProperty, true, true );
    yield return new ( CLA.DryRunProperty, Array.Empty<string> ( ), false, SS.DryRunProperty, false, false );
    yield return new ( CLA.DaemonizeProperty, (string[])[ "--daemonize" ], true, SS.DaemonizeProperty, true, true );
    yield return new ( CLA.DaemonizeProperty, (string[])[ "--daemonize" ], true, SS.DaemonizeProperty, false, true );
    yield return new ( CLA.DaemonizeProperty, Array.Empty<string> ( ), false, SS.DaemonizeProperty, true, true );
    yield return new ( CLA.DaemonizeProperty, Array.Empty<string> ( ), false, SS.DaemonizeProperty, false, false );
    yield return new ( CLA.NoDaemonizeProperty, (string[])[ "--no-daemonize" ], true, SS.DaemonizeProperty, true, false );
    yield return new ( CLA.NoDaemonizeProperty, (string[])[ "--no-daemonize" ], true, SS.DaemonizeProperty, false, false );
    yield return new ( CLA.NoDaemonizeProperty, Array.Empty<string> ( ), false, SS.DaemonizeProperty, true, true );
    yield return new ( CLA.NoDaemonizeProperty, Array.Empty<string> ( ), false, SS.DaemonizeProperty, false, false );
    yield return new ( CLA.TakeSnapshotsProperty, (string[])[ "--take-snapshots" ], true, SS.TakeSnapshotsProperty, true, true );
    yield return new ( CLA.TakeSnapshotsProperty, (string[])[ "--take-snapshots" ], true, SS.TakeSnapshotsProperty, false, true );
    yield return new ( CLA.TakeSnapshotsProperty, Array.Empty<string> ( ), false, SS.TakeSnapshotsProperty, true, true );
    yield return new ( CLA.TakeSnapshotsProperty, Array.Empty<string> ( ), false, SS.TakeSnapshotsProperty, false, false );
    yield return new ( CLA.NoTakeSnapshotsProperty, (string[])[ "--no-take-snapshots" ], true, SS.TakeSnapshotsProperty, true, false );
    yield return new ( CLA.NoTakeSnapshotsProperty, (string[])[ "--no-take-snapshots" ], true, SS.TakeSnapshotsProperty, false, false );
    yield return new ( CLA.NoTakeSnapshotsProperty, Array.Empty<string> ( ), false, SS.TakeSnapshotsProperty, true, true );
    yield return new ( CLA.NoTakeSnapshotsProperty, Array.Empty<string> ( ), false, SS.TakeSnapshotsProperty, false, false );
    yield return new ( CLA.PruneSnapshotsProperty, (string[])[ "--prune-snapshots" ], true, SS.PruneSnapshotsProperty, true, true );
    yield return new ( CLA.PruneSnapshotsProperty, (string[])[ "--prune-snapshots" ], true, SS.PruneSnapshotsProperty, false, true );
    yield return new ( CLA.PruneSnapshotsProperty, Array.Empty<string> ( ), false, SS.PruneSnapshotsProperty, true, true );
    yield return new ( CLA.PruneSnapshotsProperty, Array.Empty<string> ( ), false, SS.PruneSnapshotsProperty, false, false );
    yield return new ( CLA.NoPruneSnapshotsProperty, (string[])[ "--no-prune-snapshots" ], true, SS.PruneSnapshotsProperty, true, false );
    yield return new ( CLA.NoPruneSnapshotsProperty, (string[])[ "--no-prune-snapshots" ], true, SS.PruneSnapshotsProperty, false, false );
    yield return new ( CLA.NoPruneSnapshotsProperty, Array.Empty<string> ( ), false, SS.PruneSnapshotsProperty, true, true );
    yield return new ( CLA.NoPruneSnapshotsProperty, Array.Empty<string> ( ), false, SS.PruneSnapshotsProperty, false, false );
    yield return new ( CLA.DaemonTimerIntervalProperty, (string[])[ "--daemon-timer-interval=0" ], 0u, SS.DaemonTimerIntervalSecondsProperty, 10u, 10u );
    yield return new ( CLA.DaemonTimerIntervalProperty, (string[])[ "--daemon-timer-interval=0" ], 0u, SS.DaemonTimerIntervalSecondsProperty, 10u, 10u );
    yield return new ( CLA.DaemonTimerIntervalProperty, (string[])[ "--daemon-timer-interval=10" ], 10u, SS.DaemonTimerIntervalSecondsProperty, 10u, 10u );
    yield return new ( CLA.DaemonTimerIntervalProperty, (string[])[ "--daemon-timer-interval=10" ], 10u, SS.DaemonTimerIntervalSecondsProperty, 10u, 10u );
    yield return new ( CLA.DaemonTimerIntervalProperty, (string[])[ "--daemon-timer-interval=20" ], 20u, SS.DaemonTimerIntervalSecondsProperty, 10u, 20u );
    yield return new ( CLA.DaemonTimerIntervalProperty, (string[])[ "--daemon-timer-interval=20" ], 20u, SS.DaemonTimerIntervalSecondsProperty, 10u, 20u );
    yield return new ( CLA.DaemonTimerIntervalProperty, (string[])[ "--daemon-timer-interval=61" ], 61u, SS.DaemonTimerIntervalSecondsProperty, 10u, 60u );
    yield return new ( CLA.DaemonTimerIntervalProperty, (string[])[ "--daemon-timer-interval=61" ], 61u, SS.DaemonTimerIntervalSecondsProperty, 10u, 60u );
    yield return new ( CLA.DaemonTimerIntervalProperty, Array.Empty<string> ( ), 0u, SS.DaemonTimerIntervalSecondsProperty, 10u, 10u );
  }

  private static IEnumerable<TestCaseData> GetCasesForApplyCommandLineArgumentOverrides_Monitor_ExpectedChangesApplied ( )
  {
    yield return new ( typeof (CommandLineArguments).GetProperty ( "Monitor" )!, (string[])[ "--monitor" ], true, typeof (MonitoringSettings).GetProperty ( "EnableHttp" ), true, true );
    yield return new ( typeof (CommandLineArguments).GetProperty ( "Monitor" )!, (string[])[ "--monitor" ], true, typeof (MonitoringSettings).GetProperty ( "EnableHttp" ), false, true );
    yield return new ( typeof (CommandLineArguments).GetProperty ( "Monitor" )!, Array.Empty<string> ( ), false, typeof (MonitoringSettings).GetProperty ( "EnableHttp" ), true, true );
    yield return new ( typeof (CommandLineArguments).GetProperty ( "Monitor" )!, Array.Empty<string> ( ), false, typeof (MonitoringSettings).GetProperty ( "EnableHttp" ), false, false );
    yield return new ( typeof (CommandLineArguments).GetProperty ( "NoMonitor" )!, (string[])[ "--no-monitor" ], true, typeof (MonitoringSettings).GetProperty ( "EnableHttp" ), true, false );
    yield return new ( typeof (CommandLineArguments).GetProperty ( "NoMonitor" )!, (string[])[ "--no-monitor" ], true, typeof (MonitoringSettings).GetProperty ( "EnableHttp" ), false, false );
    yield return new ( typeof (CommandLineArguments).GetProperty ( "NoMonitor" )!, Array.Empty<string> ( ), false, typeof (MonitoringSettings).GetProperty ( "EnableHttp" ), true, true );
    yield return new ( typeof (CommandLineArguments).GetProperty ( "NoMonitor" )!, Array.Empty<string> ( ), false, typeof (MonitoringSettings).GetProperty ( "EnableHttp" ), false, false );
  }

  private static PropertyInfo[] GetSnapsInAZfsSettingsPropertyInfos ( )
  {
    return [ .. typeof (SnapsInAZfsSettings).GetProperties ( ).Where ( static pi => pi.Name is not nameof (SnapsInAZfsSettings.Templates) and not nameof (SnapsInAZfsSettings.Monitoring) ) ];
  }

  private static void ResetNLogToNoOutput ( )
  {
    if ( LogManager.Configuration is not null )
    {
      LogManager.Shutdown ( );
    }

    LogManager.Setup ( ).LoadConfiguration ( static builder => { builder.ForLogger ( ).FilterLevels ( LogLevel.Trace, LogLevel.Off ).WriteToNil ( ); } );
    LogManager.ReconfigExistingLoggers ( true );
  }
}

file static class CommandLineArgumentsReflectionHelpers
{
  internal static PropertyInfo CronProperty => field ??= typeof (CommandLineArguments).GetProperty ( "Cron" )!;
  internal static PropertyInfo DaemonTimerIntervalProperty => field ??= typeof (CommandLineArguments).GetProperty ( "DaemonTimerInterval" )!;
  internal static PropertyInfo DaemonizeProperty => field ??= typeof (CommandLineArguments).GetProperty ( "Daemonize" )!;
  internal static PropertyInfo DryRunProperty => field ??= typeof (CommandLineArguments).GetProperty ( "DryRun" )!;
  internal static PropertyInfo NoDaemonizeProperty => field ??= typeof (CommandLineArguments).GetProperty ( "NoDaemonize" )!;
  internal static PropertyInfo NoPruneSnapshotsProperty => field ??= typeof (CommandLineArguments).GetProperty ( "NoPruneSnapshots" )!;
  internal static PropertyInfo NoTakeSnapshotsProperty => field ??= typeof (CommandLineArguments).GetProperty ( "NoTakeSnapshots" )!;
  internal static PropertyInfo PruneSnapshotsProperty => field ??= typeof (CommandLineArguments).GetProperty ( "PruneSnapshots" )!;
  internal static PropertyInfo TakeSnapshotsProperty => field ??= typeof (CommandLineArguments).GetProperty ( "TakeSnapshots" )!;
}

file static class SnapsInAZfsSettingsReflectionHelpers
{
  internal static PropertyInfo DaemonTimerIntervalSecondsProperty => field ??= typeof (SnapsInAZfsSettings).GetProperty ( "DaemonTimerIntervalSeconds" )!;
  internal static PropertyInfo DaemonizeProperty => field ??= typeof (SnapsInAZfsSettings).GetProperty ( "Daemonize" )!;
  internal static PropertyInfo DryRunProperty => field ??= typeof (SnapsInAZfsSettings).GetProperty ( "DryRun" )!;
  internal static PropertyInfo PruneSnapshotsProperty => field ??= typeof (SnapsInAZfsSettings).GetProperty ( "PruneSnapshots" )!;
  internal static PropertyInfo TakeSnapshotsProperty => field ??= typeof (SnapsInAZfsSettings).GetProperty ( "TakeSnapshots" )!;
}
