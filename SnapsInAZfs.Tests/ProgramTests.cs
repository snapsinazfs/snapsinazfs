using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PowerArgs;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Tests;
[TestFixture]
[TestOf(typeof(Program))]
public class ProgramTests
{
    [Test]
    public void ApplyCommandLineArgumentOverrides_ExpectedChangesApplied_DryRun([Values]bool initialValue)
    {
        SnapsInAZfsSettings settingsObject = new( )
        {
            DryRun = initialValue
        };
        CommandLineArguments testArgs = Args.Parse<CommandLineArguments>( new[] { "--dry-run" } );
        Assume.That( testArgs.DryRun, Is.True );
        Assume.That( settingsObject.DryRun, Is.EqualTo( initialValue ) );
        Program.ApplyCommandLineArgumentOverrides( in testArgs, settingsObject );
        Assert.That( settingsObject.DryRun, Is.True );
    }

    [Test]
    public void ApplyCommandLineArgumentOverrides_ExpectedChangesApplied_NoArgsSpecified([ValueSource(nameof(GetSnapsInAZfsSettingsPropertyInfos))]PropertyInfo pi )
    {
        SnapsInAZfsSettings initialSettings = new( );
        SnapsInAZfsSettings possiblyChangedSettings = new( );
        Assume.That( pi.GetValue( initialSettings ), Is.EqualTo( pi.GetValue( possiblyChangedSettings ) ) );

        CommandLineArguments testArgs = Args.Parse<CommandLineArguments>( Array.Empty<string>( ) );
        Program.ApplyCommandLineArgumentOverrides( in testArgs, possiblyChangedSettings );
        Assert.That( pi.GetValue( initialSettings ), Is.EqualTo( pi.GetValue( possiblyChangedSettings ) ) );
    }

    [Test]
    [TestCaseSource( nameof( GetCasesForApplyCommandLineArgumentOverrides_ExpectedChangesApplied ) )]
    public void ApplyCommandLineArgumentOverrides_ExpectedChangesApplied<T>( PropertyInfo argPropertyInfo, string[] argStrings, T argValue, PropertyInfo settingsPropertyInfo, T initialSettingValue, T expectedFinalSettingValue )
    {
        SnapsInAZfsSettings initialSettings = new( );
        SnapsInAZfsSettings possiblyChangedSettings = new( );
        settingsPropertyInfo.SetValue( initialSettings, initialSettingValue );
        settingsPropertyInfo.SetValue( possiblyChangedSettings, initialSettingValue );
        CommandLineArguments testArgs = Args.Parse<CommandLineArguments>( argStrings );
        argPropertyInfo.SetValue( testArgs, argValue );
        Assume.That( settingsPropertyInfo.GetValue( initialSettings ), Is.EqualTo( initialSettingValue ) );
        Assume.That( settingsPropertyInfo.GetValue( initialSettings ), Is.EqualTo( settingsPropertyInfo.GetValue( possiblyChangedSettings ) ) );
        Assume.That( argPropertyInfo.GetValue( testArgs ), Is.EqualTo( argValue ) );

        Program.ApplyCommandLineArgumentOverrides( in testArgs, possiblyChangedSettings );
        Assert.That( settingsPropertyInfo.GetValue( possiblyChangedSettings ), Is.EqualTo( expectedFinalSettingValue ) );
    }

    private static PropertyInfo[] GetSnapsInAZfsSettingsPropertyInfos( )
    {
        return typeof( SnapsInAZfsSettings ).GetProperties( ).Where( p => p.PropertyType != typeof( MonitoringSettings ) ).ToArray( );
    }

    private static IEnumerable<TestCaseData> GetCasesForApplyCommandLineArgumentOverrides_ExpectedChangesApplied( )
    {
        yield return new( typeof( CommandLineArguments ).GetProperty( "DryRun" )!, new[] { "--dry-run" }, true, typeof( SnapsInAZfsSettings ).GetProperty( "DryRun" ), true, true );
        yield return new( typeof( CommandLineArguments ).GetProperty( "DryRun" )!, new[] { "--dry-run" }, true, typeof( SnapsInAZfsSettings ).GetProperty( "DryRun" ), false, true );
        yield return new( typeof( CommandLineArguments ).GetProperty( "DryRun" )!, Array.Empty<string>( ), false, typeof( SnapsInAZfsSettings ).GetProperty( "DryRun" ), true, true );
        yield return new( typeof( CommandLineArguments ).GetProperty( "DryRun" )!, Array.Empty<string>( ), false, typeof( SnapsInAZfsSettings ).GetProperty( "DryRun" ), false, false );
        yield return new( typeof( CommandLineArguments ).GetProperty( "Daemonize" )!, new[] { "--daemonize" }, true, typeof( SnapsInAZfsSettings ).GetProperty( "Daemonize" ), true, true );
        yield return new( typeof( CommandLineArguments ).GetProperty( "Daemonize" )!, new[] { "--daemonize" }, true, typeof( SnapsInAZfsSettings ).GetProperty( "Daemonize" ), false, true );
        yield return new( typeof( CommandLineArguments ).GetProperty( "Daemonize" )!, Array.Empty<string>( ), false, typeof( SnapsInAZfsSettings ).GetProperty( "Daemonize" ), true, true );
        yield return new( typeof( CommandLineArguments ).GetProperty( "Daemonize" )!, Array.Empty<string>( ), false, typeof( SnapsInAZfsSettings ).GetProperty( "Daemonize" ), false, false );
        yield return new( typeof( CommandLineArguments ).GetProperty( "NoDaemonize" )!, new[] { "--no-daemonize" }, true, typeof( SnapsInAZfsSettings ).GetProperty( "Daemonize" ), true, false );
        yield return new( typeof( CommandLineArguments ).GetProperty( "NoDaemonize" )!, new[] { "--no-daemonize" }, true, typeof( SnapsInAZfsSettings ).GetProperty( "Daemonize" ), false, false );
        yield return new( typeof( CommandLineArguments ).GetProperty( "NoDaemonize" )!, Array.Empty<string>( ), false, typeof( SnapsInAZfsSettings ).GetProperty( "Daemonize" ), true, true );
        yield return new( typeof( CommandLineArguments ).GetProperty( "NoDaemonize" )!, Array.Empty<string>( ), false, typeof( SnapsInAZfsSettings ).GetProperty( "Daemonize" ), false, false );
        yield return new( typeof( CommandLineArguments ).GetProperty( "TakeSnapshots" )!, new[] { "--take-snapshots" }, true, typeof( SnapsInAZfsSettings ).GetProperty( "TakeSnapshots" ), true, true );
        yield return new( typeof( CommandLineArguments ).GetProperty( "TakeSnapshots" )!, new[] { "--take-snapshots" }, true, typeof( SnapsInAZfsSettings ).GetProperty( "TakeSnapshots" ), false, true );
        yield return new( typeof( CommandLineArguments ).GetProperty( "TakeSnapshots" )!, Array.Empty<string>( ), false, typeof( SnapsInAZfsSettings ).GetProperty( "TakeSnapshots" ), true, true );
        yield return new( typeof( CommandLineArguments ).GetProperty( "TakeSnapshots" )!, Array.Empty<string>( ), false, typeof( SnapsInAZfsSettings ).GetProperty( "TakeSnapshots" ), false, false );
        yield return new( typeof( CommandLineArguments ).GetProperty( "NoTakeSnapshots" )!, new[] { "--no-take-snapshots" }, true, typeof( SnapsInAZfsSettings ).GetProperty( "TakeSnapshots" ), true, false );
        yield return new( typeof( CommandLineArguments ).GetProperty( "NoTakeSnapshots" )!, new[] { "--no-take-snapshots" }, true, typeof( SnapsInAZfsSettings ).GetProperty( "TakeSnapshots" ), false, false );
        yield return new( typeof( CommandLineArguments ).GetProperty( "NoTakeSnapshots" )!, Array.Empty<string>( ), false, typeof( SnapsInAZfsSettings ).GetProperty( "TakeSnapshots" ), true, true );
        yield return new( typeof( CommandLineArguments ).GetProperty( "NoTakeSnapshots" )!, Array.Empty<string>( ), false, typeof( SnapsInAZfsSettings ).GetProperty( "TakeSnapshots" ), false, false );
        yield return new( typeof( CommandLineArguments ).GetProperty( "PruneSnapshots" )!, new[] { "--prune-snapshots" }, true, typeof( SnapsInAZfsSettings ).GetProperty( "PruneSnapshots" ), true, true );
        yield return new( typeof( CommandLineArguments ).GetProperty( "PruneSnapshots" )!, new[] { "--prune-snapshots" }, true, typeof( SnapsInAZfsSettings ).GetProperty( "PruneSnapshots" ), false, true );
        yield return new( typeof( CommandLineArguments ).GetProperty( "PruneSnapshots" )!, Array.Empty<string>( ), false, typeof( SnapsInAZfsSettings ).GetProperty( "PruneSnapshots" ), true, true );
        yield return new( typeof( CommandLineArguments ).GetProperty( "PruneSnapshots" )!, Array.Empty<string>( ), false, typeof( SnapsInAZfsSettings ).GetProperty( "PruneSnapshots" ), false, false );
        yield return new( typeof( CommandLineArguments ).GetProperty( "NoPruneSnapshots" )!, new[] { "--no-prune-snapshots" }, true, typeof( SnapsInAZfsSettings ).GetProperty( "PruneSnapshots" ), true, false );
        yield return new( typeof( CommandLineArguments ).GetProperty( "NoPruneSnapshots" )!, new[] { "--no-prune-snapshots" }, true, typeof( SnapsInAZfsSettings ).GetProperty( "PruneSnapshots" ), false, false );
        yield return new( typeof( CommandLineArguments ).GetProperty( "NoPruneSnapshots" )!, Array.Empty<string>( ), false, typeof( SnapsInAZfsSettings ).GetProperty( "PruneSnapshots" ), true, true );
        yield return new( typeof( CommandLineArguments ).GetProperty( "NoPruneSnapshots" )!, Array.Empty<string>( ), false, typeof( SnapsInAZfsSettings ).GetProperty( "PruneSnapshots" ), false, false );
    }
}
