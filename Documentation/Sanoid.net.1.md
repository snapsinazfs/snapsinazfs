% Sanoid.net(1) Sanoid.net 1.0.0-Beta1
% Brandon Thetford
% June 2023

# Name
Sanoid.net - Snapshot manager for ZFS on Linux

# Synopsis
**Sanoid.net** \[options\]

# Description
Sanoid.net is a snapshot manager for ZFS on Linux.

# Options

**\-\-cache-dir**, **-CacheDir**
: Cache directory for sanoid, without trailing slash

**\-\-check-zfs-properties**, **-CheckZfsProperties**
: Causes Sanoid.net to check all pool root filesystems for expected properties
that must be defined, and performs minimal validation on the correctness of
their values, if defined, and then exits without performing any further
actions. Missing or incorrect properties will be reported via nlog. Exit
status will indicate the general result of the check.

**\-\-configconsole**, **\-\-config-console**, **-ConfigConsole**
: Launches Sanoid.net's built-in interactive configuration console. Upon exit
from the configuration console, Sanoid.net exits. Cannot be specified with
most other command line options.

**\-\-cron**, **-Cron**
: Create snapshots and prune expired snapshots. Equivalent to specifying both
\-\-take-snapshots and \-\-prune-snapshots.

**-vv**, **\-\-debug**, **-Debug**
: Debug level output logging. Change log level in Sanoid.nlog.json for normal
usage. Very verbose and logs many operations that are not relevant outside of
a troubleshooting or debugging context. Not recommended for use in normal
operation.

**-n**, **\-\-dryrun**, **\-\-dry-run**, **\-\-readonly**, **\-\-read-only**, **-DryRun**
: Skip creation/deletion of snapshots (Simulate). Sanoid.net will pretend to
perform configured actions and report what it _would_ have done. No operations
that create or destroy ZFS snapshots will be performed, regardless of
configuration or other command-line options. If configuration or command line
options request to create or prune snapshots, this option will _simulate_
those actions without making changes to ZFS.

**-h**, **\-\-help**, **-Help**
: Displays usage information on the command line.

**\-\-no-prune-snapshots**, **-NoPruneSnapshots**
: Opposite of \-\-prune-snapshots. Will prevent ANY snapshots from being
pruned, regardless of configured values. Supercedes \-\-prune-snapshots and
\-\-force-prune.

**\-\-no-take-snapshots**, **-NoTakeSnapshots**
: Opposite of \-\-prune-snapshots. Will prevent ANY new snapshots from being
taken, regardless of configured values. Supercedes \-\-take-snapshots.

**\-\-prepare-zfs-properties**, **-PrepareZfsProperties**
: Causes Sanoid.net to check all pool root filesystems for expected properties
that must be defined, and performs minimal validation on the correctness of
their values, if defined. Missing properties will be created and set to default
values, and invalid properties will be set to default values. Valid existing
values will not be changed. Exit status indicates the status of the zfs
actions performed.

**\-\-prune-snapshots**, **-PruneSnapshots**
: Causes Sanoid.net to prune existing eligible snapshots according to the
policy configuration specified in ZFS and associated templates, except for
file systems or volumes that are currently involved in a `zfs send` or
`zfs receive` operation. To force pruning even for such datasets,
use **\-\-force-prune**.

**-q**, **\-\-quiet**, **-Quiet**
: Warn level output logging. Suppress non-error output. WILL WARN BEFORE
SETTING IS APPLIED. Configure logging in Sanoid.nlog.json for normal usage.

**-qq**, **\-\-really-quiet**, **-ReallyQuiet**
: No output logging. Change log level to Off in Sanoid.nlog.json to set for
normal usage. If nlog is configured for Trace logging in Sanoid.nlog.json,
there may be logging output before this argument is parsed.

**\-\-take-snapshots**, **-TakeSnapshots**
: Causes Sanoid.net to take snapshots according to the policy configuration
specified in ZFS and associated templates.

**-vvv**, **\-\-trace**, **-Trace**
: Trace level output logging. Change log level in Sanoid.nlog.json for normal
usage. Extremely verbose and logs many operations that are not relevant outside
of a debugging context. Strongly recommended against use in normal operations.

**-v**, **\-\-verbose**, **-Verbose**
: Info level output logging. Change log level in Sanoid.nlog.json for normal
usage. This is the default setting for console logging in Sanoid.nlog.json.
Specifying this whith default Sanoid.nlog.json settings will have no effect
on output.

**-V**, **\-\-version**, **-Version**
: Outputs Sanoid.net version to configured logging targets and exits,
performing no other operations.
