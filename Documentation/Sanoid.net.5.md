% Sanoid.json(5) Sanoid.net 1.0.0-Beta1 Configuration
% Brandon Thetford
% June 2023

# Name
Sanoid.json - Configuration files for Sanoid.net

# Synopsis
**/usr/local/share/Sanoid.net/Sanoid.json** (Sanoid.json)
**/etc/sanoid/Sanoid.local.json** (Sanoid.local.json)

# Description
These files contain general configuration and templates for Sanoid.net.

All configuration files are formatted as plain-text JSON files, in UTF-8
encoding.

Note: Command line arguments which affect settings in any configuration file
have highest precedence and will override their matching settings, at run-time.\
See Sanoid.net(8) for details on command line arguments.

## Configuration Files
**/usr/local/share/Sanoid.net/Sanoid.json** is the base configuration file and
is not intended, recommended, or supported to be changed by the end user.\
This file is installed with read-only permissions, by default, to help avoid
unintended modifications.

**/etc/sanoid/Sanoid.local.json** is the local configuration file.\
All settings specified in this file extend the configuration contained in
Sanoid.json. Any leaf-level settings in Sanoid.local.json that have the same
fully-qualified JSON path as leaf-level settings defined in Sanoid.json will
override the matching settings in Sanoid.json, at run-time.

## Configuration File Schemas
The schemas for Sanoid.json and Sanoid.local.json are identical, except for
required elements. Both can be found in /usr/local/share/Sanoid.net as
Sanoid.schema.json and Sanoid.local.schema.json.\
The schema documents fully describe the layout and legal format for these
configuration files, and are to be considered authoritative, if any
discrepancies exist between this document and those files.

Changes to the schema documents are NOT supported and are likely to cause
run-time errors in Sanoid.net. DO NOT MODIFY THE SCHEMA DOCUMENTS.

# Settings

Settings are described below using their JSON path as identifiers, and a
description including type, default value, and what each settings does.

## General Settings

**/DryRun** (boolean - "true" or "false")
: This is a global setting that controls whether Sanoid.net will perform
a dry run, when invoked. If this setting is "true," Sanoid.net will perform
a dry run and make no changes to ZFS. The command-line option **\-\-dry-run**
overrides this setting.

**/TakeSnapshots** (boolean - "true" or "false")
: This is a global setting that controls whether Sanoid.net will take new
snapshots. If this setting is true, Sanoid.net will take snapshots according
to the properties set on each ZFS dataset. If this setting is false, new
snapshot processing is skipped and no new snapshots will be taken, regardless
of the sanoid.net:takesnapshots property set on each ZFS dataset. The
command-line options **\-\-take-snapshots** and **\-\-no-take-snapshots**
override this setting.

**/PruneSnapshots** (boolean - "true" or "false")
: This is a global setting that controls whether Sanoid.net will prune
expired snapshots. If this setting is true, Sanoid.net will prune eligible
snapshots according to the properties set on each snapshot or inherited from
its parent dataset. If this setting is false, existing snapshots will not be
pruned, regardless of the sanoid.net:prunesnapshots property setting on each
ZFS dataset, the retention properties on each ZFS dataset or snapshot, or the
age of the snapshot. The command-line options **\-\-prune-snapshots** and
**\-\-no-prune-snapshots** override this setting.
