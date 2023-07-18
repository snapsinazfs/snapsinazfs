// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using PowerArgs;

#pragma warning disable CS1591

namespace SnapsInAZfs;

/// <summary>
///     The command line arguments that SnapsInAZfs can accept
/// </summary>
[ArgExceptionBehavior( ArgExceptionPolicy.StandardExceptionHandling )]
// ReSharper disable once ClassNeverInstantiated.Global
public class CommandLineArguments
{
    [ArgDescription( "Checks the property schema for SnapsInAZfs in zfs and reports any missing properties for pool roots." )]
    [ArgShortcut( "--check-zfs-properties" )]
    [ArgCantBeCombinedWith("PrepareZfsProperties")]
    public bool CheckZfsProperties { get; set; }

    [ArgDescription( "Launches SnapsInAZfs' built-in interactive configuration console" )]
    [ArgShortcut( "--configconsole" )]
    [ArgShortcut( "--config-console" )]
    [ArgCantBeCombinedWith( "DryRun|TakeSnapshots|PruneSnapshots|ForcePrune|CheckZfsProperties|PrepareZfsProperties" )]
    public bool ConfigConsole { get; set; }

    [ArgDescription( "Create snapshots and prune expired snapshots. Equivalent to --take-snapshots --prune-snapshots" )]
    [ArgShortcut( "--cron" )]
    public bool Cron { get; set; }

    [ArgDescription( "Run SnapsInAZfs as a daemon" )]
    [ArgShortcut( "--daemonize" )]
    [ArgShortcut( "-D" )]
    [ArgCantBeCombinedWith( "NoDaemonize|CheckZfsProperties|PrepareZfsProperties|ConfigConsole|Version|Help" )]
    public bool Daemonize { get; set; }

    [ArgDescription( "Override the configured daemon event processing timer. Specified as a whole number of seconds. Default: 10" )]
    [ArgShortcut( "--daemon-timer-interval" )]
    [ArgDefaultValue( 0 )]
    public int DaemonTimerInterval { get; set; } = 0;

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
    public bool ForcePrune { get; set; }

    [ArgDescription( "Shows this help" )]
    [ArgShortcut( "-h" )]
    [ArgShortcut( "--help" )]
    [HelpHook]
    public bool Help { get; set; }

    [ArgDescription( "This option is designed to be run by a Nagios monitoring system. It reports on the capacity of the zpool your filesystems are on." )]
    [ArgShortcut( "--monitor-capacity" )]
    [ArgShortcut( "--monitor-capacity-nagios" )]
    public bool MonitorCapacity { get; set; }

    [ArgDescription( "This option is designed to be run by a Nagios monitoring system. It reports on the health of the zpool your filesystems are on." )]
    [ArgShortcut( "--monitor-health" )]
    [ArgShortcut( "--monitor-health-nagios" )]
    public bool MonitorHealth { get; set; }

    [ArgDescription( "This option is designed to be run by a Nagios monitoring system. It reports on the health of your snapshots." )]
    [ArgShortcut( "--monitor-snapshots" )]
    [ArgShortcut( "--monitor-snapshots-nagios" )]
    public bool MonitorSnapshots { get; set; }

    [ArgDescription( "Force SnapsInAZfs to NOT run as a daemon" )]
    [ArgShortcut( "--no-daemonize" )]
    [ArgCantBeCombinedWith( "Daemonize|Cron|DaemonTimerInterval" )]
    public bool NoDaemonize { get; set; }

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
