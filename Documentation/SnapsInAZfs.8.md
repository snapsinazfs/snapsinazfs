% SnapsInAZfs(8) SnapsInAZfs 1.0.0-Beta1
% Brandon Thetford
% June 2023

# Name
SnapsInAZfs - Snapshot manager for ZFS on Linux

# Synopsis
**SnapsInAZfs** \[options\]

# Description
SnapsInAZfs (SIAZ) is a snapshot manager for ZFS on Linux.

# Options

**\-\-check-zfs-properties**, **-CheckZfsProperties**
: Causes SnapsInAZfs to check all pool root filesystems for expected properties
that must be defined, and performs minimal validation on the correctness of
their values, if defined, and then exits without performing any further
actions. Missing or incorrect properties will be reported via nlog. Exit
status will indicate the general result of the check.

**\-\-configconsole**, **\-\-config-console**, **-ConfigConsole**
: Launches SnapsInAZfs's built-in interactive configuration console. Upon exit
from the configuration console, SnapsInAZfs exits. Cannot be specified with
most other command line options.

**\-\-cron**, **-Cron**
: Create snapshots and prune expired snapshots. Equivalent to specifying both
\-\-take-snapshots and \-\-prune-snapshots.

**-vv**, **\-\-debug**, **-Debug**
: Debug level output logging. Change log level in SnapsInAZfs.nlog.json for normal
usage. Very verbose and logs many operations that are not relevant outside of
a troubleshooting or debugging context. Not recommended for use in normal
operation.

**-n**, **\-\-dryrun**, **\-\-dry-run**, **\-\-readonly**, **\-\-read-only**, **-DryRun**
: Skip creation/deletion of snapshots (Simulate). SnapsInAZfs will pretend to
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
: Causes SnapsInAZfs to check all pool root filesystems for expected properties
that must be defined, and performs minimal validation on the correctness of
their values, if defined. Missing properties will be created and set to default
values, and invalid properties will be set to default values. Valid existing
values will not be changed. Exit status indicates the status of the zfs
actions performed.

**\-\-prune-snapshots**, **-PruneSnapshots**
: Causes SnapsInAZfs to prune existing eligible snapshots according to the
policy configuration specified in ZFS and associated templates, except for
file systems or volumes that are currently involved in a `zfs send` or
`zfs receive` operation. To force pruning even for such datasets,
use **\-\-force-prune**.

**-q**, **\-\-quiet**, **-Quiet**
: Warn level output logging. Suppress non-error output. WILL WARN BEFORE
SETTING IS APPLIED. Configure logging in SnapsInAZfs.nlog.json for normal usage.

**-qq**, **\-\-really-quiet**, **-ReallyQuiet**
: No output logging. Change log level to Off in SnapsInAZfs.nlog.json to set for
normal usage. If nlog is configured for Trace logging in SnapsInAZfs.nlog.json,
there may be logging output before this argument is parsed.

**\-\-take-snapshots**, **-TakeSnapshots**
: Causes SnapsInAZfs to take snapshots according to the policy configuration
specified in ZFS and associated templates.

**-vvv**, **\-\-trace**, **-Trace**
: Trace level output logging. Change log level in SnapsInAZfs.nlog.json for normal
usage. Extremely verbose and logs many operations that are not relevant outside
of a debugging context. Strongly recommended against use in normal operations.

**-v**, **\-\-verbose**, **-Verbose**
: Info level output logging. Change log level in SnapsInAZfs.nlog.json for normal
usage. This is the default setting for console logging in SnapsInAZfs.nlog.json.
Specifying this whith default SnapsInAZfs.nlog.json settings will have no effect
on output.

**-V**, **\-\-version**, **-Version**
: Outputs SnapsInAZfs version to configured logging targets and exits,
performing no other operations.

# Examples

These are potential common usage scenarios.\
These scenarios assume your SnapsInAZfs.local.json file has TakeSnapshots and
PruneSnapshots set to true and DryRun set to false, unless otherwise noted.

**Note:** for ***ALL*** invocations of SIAZ, including when `-\-\dry-run` or
`-\-config-console` are specified, SIAZ will ***ALWAYS*** check for existence of
and very basic validity of the ZFS user properties it needs to operate.\
If missing or invalid properties are detected on any pool roots, SIAZ will
terminate with an exit code and with log output indicating what's wrong.

SIAZ does not currently support configurations having only some pools with SIAZ
properties defined.

## General Usage

Normal Use:
```
$ SnapsInAZfs
```
If invoked with no command line options, SIAZ will first check for expected
ZFS property schema on all pool roots, and, if valid, will first take snapshots
and then prune snapshots, as configured. 

Dry Run - Test/Simulation Mode:
```
$ SnapsInAZfs \-\-dry-run
```
Useful for testing configuration changes without making any changes to ZFS.
Has the same effect as setting DryRun to true in the JSON configuration files.
All other arguments behave as normal, with the exception that, no matter which
other arguments are specified, no changes will be made to ZFS (ie no snapshots
will be taken or pruned, and no properties will be altered).

## Backup/Replication

Skip taking new snapshots and only prune expired snapshots:
```
$ SnapsInAZfs \-\-no-take-snapshots
```
Useful on systems that receive snapshots from another system, such as in
a backup/replication setup, and the system does not need to take new snapshots,
but should prune old snapshots. While it is recommended that you do this via
configuration, rather than via command-line arguments, this may provide a
helpful guarantee, even if accidental changes are made or new datasets are
received, no new snapshots will be taken.\
Also useful if you have just made a configuration change that would
result in more snapshots being pruned, and you wish to manually run SIAZ to
observe the results, possibly with a more verbose logging argument or dry run,
as well.

# Exit Status
0 Exit code indicates success\
Non-zero exit codes indicate an error or some other condition that should
result in termination of or allow specific handling in scripts.

 - 0: Normal exit status - Requested operations completed successfully or with
 no fatal errors.
 - 11: EAGAIN - The SIAZ mutex was abandoned by a previous invocation. Try again
 - 16: EBUSY - The SIAZ mutex could not be acquired because another instance is
 already running.
 - 17: EEXIST - The SIAZ mutex could not be acquired
 - 22: EINVAL - An IO exception occurred while trying to acquire the mutex that
 ensures only one instance of SIAZ can run at a time.
 - 37: ENOLCK - The SIAZ mutex was null. Execution is not safe to continue.
 - 127: ECANCELED - Help or Version CLI argument was specified. Used to prevent
 inadvertent inclusion of those arguments in scripts.
 - 1079: EFTYPE - One or more JSON configuration files were missing or invalid
 - 1093: ENOATTR - The ZFS property schema is not valid or an attempt to update
 the ZFS property schema failed.

# Copyright
SnapsInAZfs is created by Brandon Thetford and is inspired by sanoid, created by
Jim Salter.\
This software is licensed for use under the Free Software Foundation's
GPL v3.0 license, or later. See https://www.gnu.org/licenses/gpl-3.0.html
