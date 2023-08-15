#region MIT LICENSE

// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/

#endregion

using PowerArgs;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

#pragma warning disable CS1591

namespace SnapsInAZfs;

/// <summary>
///     The command line arguments that SnapsInAZfs can accept
/// </summary>
[ArgExceptionBehavior( ArgExceptionPolicy.StandardExceptionHandling )]
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class CommandLineArguments
{
    [ArgDescription( "Checks the property schema for SnapsInAZfs in zfs and reports any missing properties for pool roots." )]
    [ArgShortcut( "--check-zfs-properties" )]
    [ArgCantBeCombinedWith( "PrepareZfsProperties" )]
    public bool CheckZfsProperties { get; set; }

    [ArgDescription( "Launches SnapsInAZfs' built-in interactive configuration console" )]
    [ArgShortcut( "--configconsole" )]
    [ArgShortcut( "--config-console" )]
    [ArgCantBeCombinedWith( "DryRun|TakeSnapshots|PruneSnapshots|ForcePrune|CheckZfsProperties|PrepareZfsProperties" )]
    public bool ConfigConsole { get; set; }

    [ArgDescription( "A comma-separated list of configuration files to load" )]
    [ArgShortcut( "--config" )]
    [ArgShortcut( "--config-file" )]
    [ArgShortcut( "--config-files" )]
    public string[] ConfigFiles { get; set; } = Array.Empty<string>( );

    [ArgDescription( "Create snapshots and prune expired snapshots. Equivalent to --take-snapshots --prune-snapshots" )]
    [ArgShortcut( "--cron" )]
    public bool Cron { get; set; }

    [ArgDescription( "Run SnapsInAZfs as a daemon" )]
    [ArgShortcut( "--daemonize" )]
    [ArgShortcut( "-D" )]
    [ArgCantBeCombinedWith( "NoDaemonize|CheckZfsProperties|PrepareZfsProperties|ConfigConsole|Version|Help" )]
    public bool Daemonize { get; set; }

    [ArgDescription( "Override the configured daemon event processing timer. Specified as a whole number of seconds." )]
    [ArgShortcut( "--daemon-timer-interval" )]
    public uint DaemonTimerInterval { get; set; } = 0;

    [ArgDescription( "Debug level output logging. Change log level in SnapsInAZfs.nlog.json for normal usage." )]
    [ArgShortcut( "--debug" )]
    [ArgShortcut( "vv" )]
    [ArgCantBeCombinedWith( "Verbose|Quiet|ReallyQuiet|Trace" )]
    public bool Debug { get; set; }

    [ArgDescription( "Skip creation/deletion of snapshots (Simulate)." )]
    [ArgShortcut( "--readonly" )]
    [ArgShortcut( "--read-only" )]
    [ArgShortcut( "--dryrun" )]
    [ArgShortcut( "--dry-run" )]
    [ArgShortcut( "n" )]
    public bool DryRun { get; set; }

    [ArgDescription( "Provided only for backward-compatibility with sanoid. Has no effect, as SnapsInAZfs uses deferred destroy for pruning." )]
    [ArgShortcut( "--force-prune" )]
    [ArgShortcut( "--force-prune-snapshots" )]
    [ArgCantBeCombinedWith( "NoPruneSnapshots" )]
    public bool ForcePrune { get; set; }

    [ArgDescription( "Shows this help" )]
    [ArgShortcut( "h" )]
    [ArgShortcut( "--help" )]
    [HelpHook]
    public bool Help { get; set; }

    [ArgDescription( "Enables the monitoring endpoints" )]
    [ArgShortcut( "--monitor" )]
    [ArgCantBeCombinedWith( "NoMonitor|NoDaemonize|ConfigConsole" )]
    public bool Monitor { get; set; }

    [ArgDescription( "Force SnapsInAZfs to NOT run as a daemon" )]
    [ArgShortcut( "--no-daemonize" )]
    [ArgCantBeCombinedWith( "Daemonize|Cron" )]
    public bool NoDaemonize { get; set; }

    [ArgDescription( "Disables the monitoring endpoints and prevents kestrel from loading at all" )]
    [ArgShortcut( "--no-monitor" )]
    [ArgCantBeCombinedWith( "Monitor" )]
    public bool NoMonitor { get; set; }

    [ArgDescription( "Disables snapshot pruning." )]
    [ArgShortcut( "--no-prune-snapshots" )]
    [ArgCantBeCombinedWith( "PruneSnapshots" )]
    public bool NoPruneSnapshots { get; set; }

    [ArgDescription( "Disables new snapshot processing." )]
    [ArgShortcut( "--no-take-snapshots" )]
    [ArgCantBeCombinedWith( "TakeSnapshots" )]
    public bool NoTakeSnapshots { get; set; }

    [ArgDescription( "Updates the property schema for SnapsInAZfs in zfs, using default values. Will not overwrite values that are already set." )]
    [ArgShortcut( "--prepare-zfs-properties" )]
    public bool PrepareZfsProperties { get; set; }

    [ArgDescription( "Prunes expired snapshots" )]
    [ArgShortcut( "--prune-snapshots" )]
    [ArgCantBeCombinedWith( "NoPruneSnapshots" )]
    public bool PruneSnapshots { get; set; }

    [ArgDescription( "Suppress non-error output. WILL WARN BEFORE SETTING IS APPLIED. Configure in SnapsInAZfs.nlog.json for normal usage." )]
    [ArgShortcut( "--quiet" )]
    [ArgShortcut( "q" )]
    [ArgCantBeCombinedWith( "Debug|Verbose|Trace|DryRun" )]
    public bool Quiet { get; set; }

    [ArgDescription( "No output logging. Change log level to Off in SnapsInAZfs.nlog.json to set for normal usage. Will not warn when used." )]
    [ArgShortcut( "--really-quiet" )]
    [ArgShortcut( "qq" )]
    [ArgCantBeCombinedWith( "Debug|Verbose|DryRun|Trace|Quiet" )]
    public bool ReallyQuiet { get; set; }

    [ArgDescription( "Enables new snapshot processing. Respects dry-run argument." )]
    [ArgShortcut( "--take-snapshots" )]
    [ArgCantBeCombinedWith( "NoTakeSnapshots" )]
    public bool TakeSnapshots { get; set; }

    [ArgDescription( "Trace level output logging. Change log level in SnapsInAZfs.nlog.json for normal usage. Has no effect until configuration is parsed." )]
    [ArgShortcut( "--trace" )]
    [ArgShortcut( "vvv" )]
    [ArgCantBeCombinedWith( "Verbose|Debug|Quiet|ReallyQuiet" )]
    public bool Trace { get; set; }

    [ArgDescription( "Verbose (Info level) output logging. Change log level in SnapsInAZfs.nlog.json for normal usage." )]
    [ArgShortcut( "v" )]
    [ArgShortcut( "--verbose" )]
    [ArgEnforceCase]
    [ArgCantBeCombinedWith( "Debug|Quiet|ReallyQuiet|Trace" )]
    public bool Verbose { get; set; }

    [ArgDescription( "Outputs SnapsInAZfs version to configured logging targets and exits, making no changes." )]
    [ArgShortcut( "V" )]
    [ArgShortcut( "--version" )]
    [ArgEnforceCase]
    public bool Version { get; set; }
}
