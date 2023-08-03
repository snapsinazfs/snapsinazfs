#region MIT LICENSE

// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the �Software�), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED �AS IS�, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/

#endregion

using System.Reflection;
using PowerArgs;
using SnapsInAZfs.Interop.Libc.Enums;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Tests;

[TestFixture]
[TestOf( typeof( Program ) )]
public class ProgramTests
{
    [Test]
    [TestCaseSource( nameof( GetCasesForApplyCommandLineArgumentOverrides_ExpectedChangesApplied ) )]
    public void ApplyCommandLineArgumentOverrides_ExpectedChangesApplied<T>( PropertyInfo argPropertyInfo, string[] argStrings, T argValue, PropertyInfo settingsPropertyInfo, T initialSettingValue, T expectedFinalSettingValue )
    {
        SnapsInAZfsSettings initialSettings = new( );
        SnapsInAZfsSettings possiblyChangedSettings = new( );
        settingsPropertyInfo.SetValue( initialSettings, initialSettingValue );
        settingsPropertyInfo.SetValue( possiblyChangedSettings, initialSettingValue );
        CommandLineArguments testArgs = Args.Parse<CommandLineArguments>( argStrings );
        Assume.That( testArgs, Is.Not.Null );
        argPropertyInfo.SetValue( testArgs, argValue );
        Assume.That( settingsPropertyInfo.GetValue( initialSettings ), Is.EqualTo( initialSettingValue ) );
        Assume.That( settingsPropertyInfo.GetValue( initialSettings ), Is.EqualTo( settingsPropertyInfo.GetValue( possiblyChangedSettings ) ) );
        Assume.That( argPropertyInfo.GetValue( testArgs ), Is.EqualTo( argValue ) );

        Program.ApplyCommandLineArgumentOverrides( in testArgs, possiblyChangedSettings );
        Assert.That( settingsPropertyInfo.GetValue( possiblyChangedSettings ), Is.EqualTo( expectedFinalSettingValue ) );
    }

    [Test]
    public void ApplyCommandLineArgumentOverrides_ExpectedChangesApplied_DryRun( [Values] bool initialValue )
    {
        SnapsInAZfsSettings settingsObject = new( )
        {
            DryRun = initialValue
        };
        CommandLineArguments testArgs = Args.Parse<CommandLineArguments>( "--dry-run" );
        Assume.That( testArgs.DryRun, Is.True );
        Assume.That( settingsObject.DryRun, Is.EqualTo( initialValue ) );
        Program.ApplyCommandLineArgumentOverrides( in testArgs, settingsObject );
        Assert.That( settingsObject.DryRun, Is.True );
    }

    [Test]
    public void ApplyCommandLineArgumentOverrides_ExpectedChangesApplied_NoArgsSpecified( [ValueSource( nameof( GetSnapsInAZfsSettingsPropertyInfos ) )] PropertyInfo pi )
    {
        SnapsInAZfsSettings initialSettings = new( );
        SnapsInAZfsSettings possiblyChangedSettings = new( );
        Assume.That( pi.GetValue( initialSettings ), Is.EqualTo( pi.GetValue( possiblyChangedSettings ) ) );

        CommandLineArguments testArgs = Args.Parse<CommandLineArguments>( Array.Empty<string>( ) );
        Program.ApplyCommandLineArgumentOverrides( in testArgs, possiblyChangedSettings );
        Assert.That( pi.GetValue( initialSettings ), Is.EqualTo( pi.GetValue( possiblyChangedSettings ) ) );
    }

    [Test]
    public async Task Main_ReturnsECancelledOnHelpOption( [ValueSource( nameof( GetHelpArgStrings ) )] string[] args )
    {
        TextWriter stdout = Console.Out;
        Console.SetOut( TextWriter.Null );
        Errno result = (Errno)await Program.Main( args );
        Assert.That( result, Is.EqualTo( Errno.ECANCELED ) );
        Console.SetOut( stdout );
    }

    [Test]
    public async Task Main_ReturnsECancelledOnVersionOption( [ValueSource( nameof( GetVersionArgStrings ) )] string[] args )
    {
        TextWriter stdout = Console.Out;
        Console.SetOut( TextWriter.Null );
        Errno result = (Errno)await Program.Main( args );
        Assert.That( result, Is.EqualTo( Errno.ECANCELED ) );
        Console.SetOut( stdout );
    }

    [Test]
    [NonParallelizable]
    public void TryGetZfsCommandRunner_CanGetSingleton( )
    {
        Program.ZfsCommandRunnerSingleton = null;
        SnapsInAZfsSettings initialSettings = new( );
        Assume.That( Program.ZfsCommandRunnerSingleton, Is.Null );
        bool createSingletonResult = Program.TryGetZfsCommandRunner( initialSettings, out IZfsCommandRunner? zfsCommandRunnerA );
        Assume.That( createSingletonResult, Is.True );
        Assume.That( Program.ZfsCommandRunnerSingleton, Is.Not.Null );
        bool getSingletonResult = Program.TryGetZfsCommandRunner( initialSettings, out IZfsCommandRunner? zfsCommandRunnerB );
        Assert.Multiple( ( ) =>
        {
            Assert.That( getSingletonResult, Is.True );
            Assert.That( zfsCommandRunnerB, Is.SameAs( zfsCommandRunnerA ) );
            Assert.That( zfsCommandRunnerB, Is.SameAs( Program.ZfsCommandRunnerSingleton ) );
        } );
    }

    [Test]
    [NonParallelizable]
    public void TryGetZfsCommandRunner_DoesNotCreateSingletonWhenReuseSingletonFalse( )
    {
        Program.ZfsCommandRunnerSingleton = null;
        SnapsInAZfsSettings initialSettings = new( );
        Assume.That( Program.ZfsCommandRunnerSingleton, Is.Null );
        bool createSingletonResult = Program.TryGetZfsCommandRunner( initialSettings, out IZfsCommandRunner? zfsCommandRunnerA, false );
        Assume.That( createSingletonResult, Is.True );
        Assert.Multiple( ( ) =>
        {
            Assert.That( Program.ZfsCommandRunnerSingleton, Is.Null );
            Assert.That( zfsCommandRunnerA, Is.Not.Null );
        } );
        bool getSingletonResult = Program.TryGetZfsCommandRunner( initialSettings, out IZfsCommandRunner? zfsCommandRunnerB, false );
        Assert.Multiple( ( ) =>
        {
            Assert.That( getSingletonResult, Is.True );
            Assert.That( zfsCommandRunnerB, Is.Not.Null );
            Assert.That( zfsCommandRunnerB, Is.Not.SameAs( zfsCommandRunnerA ) );
            Assert.That( Program.ZfsCommandRunnerSingleton, Is.Null );
        } );
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
        yield return new( typeof( CommandLineArguments ).GetProperty( "DaemonTimerInterval" )!, new[] { "--daemon-timer-interval=0" }, 0u, typeof( SnapsInAZfsSettings ).GetProperty( "DaemonTimerIntervalSeconds" ), 10u, 10u );
        yield return new( typeof( CommandLineArguments ).GetProperty( "DaemonTimerInterval" )!, new[] { "--daemon-timer-interval=0" }, 0u, typeof( SnapsInAZfsSettings ).GetProperty( "DaemonTimerIntervalSeconds" ), 10u, 10u );
        yield return new( typeof( CommandLineArguments ).GetProperty( "DaemonTimerInterval" )!, new[] { "--daemon-timer-interval=10" }, 10u, typeof( SnapsInAZfsSettings ).GetProperty( "DaemonTimerIntervalSeconds" ), 10u, 10u );
        yield return new( typeof( CommandLineArguments ).GetProperty( "DaemonTimerInterval" )!, new[] { "--daemon-timer-interval=10" }, 10u, typeof( SnapsInAZfsSettings ).GetProperty( "DaemonTimerIntervalSeconds" ), 10u, 10u );
        yield return new( typeof( CommandLineArguments ).GetProperty( "DaemonTimerInterval" )!, new[] { "--daemon-timer-interval=20" }, 20u, typeof( SnapsInAZfsSettings ).GetProperty( "DaemonTimerIntervalSeconds" ), 10u, 20u );
        yield return new( typeof( CommandLineArguments ).GetProperty( "DaemonTimerInterval" )!, new[] { "--daemon-timer-interval=20" }, 20u, typeof( SnapsInAZfsSettings ).GetProperty( "DaemonTimerIntervalSeconds" ), 10u, 20u );
        yield return new( typeof( CommandLineArguments ).GetProperty( "DaemonTimerInterval" )!, new[] { "--daemon-timer-interval=61" }, 61u, typeof( SnapsInAZfsSettings ).GetProperty( "DaemonTimerIntervalSeconds" ), 10u, 60u );
        yield return new( typeof( CommandLineArguments ).GetProperty( "DaemonTimerInterval" )!, new[] { "--daemon-timer-interval=61" }, 61u, typeof( SnapsInAZfsSettings ).GetProperty( "DaemonTimerIntervalSeconds" ), 10u, 60u );
        yield return new( typeof( CommandLineArguments ).GetProperty( "DaemonTimerInterval" )!, Array.Empty<string>( ), 0u, typeof( SnapsInAZfsSettings ).GetProperty( "DaemonTimerIntervalSeconds" ), 10u, 10u );
    }

    private static IEnumerable<string[]> GetHelpArgStrings( )
    {
        yield return new[] { "-h" };
        yield return new[] { "-Help" };
        yield return new[] { "--help" };
    }

    private static PropertyInfo[] GetSnapsInAZfsSettingsPropertyInfos( )
    {
        return typeof( SnapsInAZfsSettings ).GetProperties( ).ToArray( );
    }

    private static IEnumerable<string[]> GetVersionArgStrings( )
    {
        yield return new[] { "-V" };
        yield return new[] { "-Version" };
        yield return new[] { "--version" };
    }
}
