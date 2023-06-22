% Sanoid.json(5) Sanoid.net 1.0.0-Beta1 Configuration
% Brandon Thetford
% June 2023

# Name
Sanoid.json - Configuration files for Sanoid.net

# Synopsis
**/usr/local/share/Sanoid.net/Sanoid.json** (Sanoid.json)\
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

These are general settings that affect the behavior of Sanoid.net.\
They can be overridden by command-line arguments, as noted.

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

## Templates

Templates, which provide common timing and formatting settings for snapshots,
are defined under the **/Templates** node, which is a dcitionary of key:value
pairs, where the key is the template's name, as a quoted string, and the
value is the template itself, as a JSON object, with a schema defined in
Sanoid.template.schema.json.

For a template to be valid for use, it must be fully defined, with all options
explicitly set. As with other configuration elements, it is possible to
configure templates manually, in the json files, but it is **strongly**
recommended that you use the configuration console to do so, instead, as it
will ensure your templates are fully and properly defined, and will prevent
you from deleting templates that are in use by any datasets in ZFS.

A template is a JSON object consisting of two sections: "Formatting" and
"SnapshotTiming"

### Template Formatting Section

The **Formatting** section of a template defines elements used for naming of
new snapshots that Sanoid.net creates. The default Formatting options
provided in the "default" template, on a new install, will result in snapshot
names that follow the same format that PERL sanoid creates. For example, a
daily snapshot taken on 2023-06-22 at 14:00:00 (local time) will be named
"path/to/dataset@**autosnap\_2023-06-22\_14:00:00_daily**".

These settings do not affect operation of Sanoid.net in any way _other_ than
in how it will name snapshots created on datasets with that template applied.

Pruning of existing snapshots is in no way affected by these settings, as
Sanoid.net keeps metadata defining what a snapshot _is_ in ZFS properties,
which are not affected by these settings.

If you use a heterogeneous setup combining both PERL sanoid and Sanoid.net,
you MUST use Formatting settings that correspond identically to PERL sanoid's
configuration, or else PERL sanoid will misinterpret or skip them entirely,
as PERL sanoid decides what a snapshot is and what to do with it based solely
off of its name.

Note that all settings must conform to ZFS identifier rules, as they apply
to naming of snapshots. Thus, only 7-bit ASCII characters in restricted ranges
are supported. The configuration console helps enforce these restrictions.
See zfs(8) for specifics.

**/Templates/templateName/Formatting/ComponentSeparator**
: This is a single character used to separate each component of a snapshot's
name. By default, it is the underscore (_) character.

**/Templates/templateName/Formatting/Prefix**
: The prefix is the first component of a snapshot name, appearing immediately
after the @ symbol. It should be a 1-12 character alphanumeric string and is
case-sensitive. By default, it is the string "autosnap"

**/Templates/templateName/Formatting/TimestampFormatString**
: The Timestamp Format String is a special format string that determines how
the timestamp is formatted in the name of a snapshot. Sanoid.net uses the .net
DateTimeOffset type, internally, for representation of time, which contains
the full date, time, and timezone offset (in local time, by default) that
the operation was executed at. This string must be escaped, following both JSON
and C# rules (which are mostly the same).
The default string is "yyyy-MM-dd_HH\\:mm\\:ss", which results in output as
shown in the example given above. Other format strings are allowed, but they
must adhere to ZFS snapshot identifier requirements and must be valid .net
DateTimeOffset format specifier strings. Documentation of valid format strings
can be found at
https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings

**/Templates/templateName/Formatting/FrequentSuffix**
: The Frequent Suffix is the string used as the final portion of a snapshot name,
when that snapshot's period is "frequent." It should be a 1-12 character
alphanumeric string and is case-sensitive. By default, it is the string
"frequently"

**/Templates/templateName/Formatting/HourlySuffix**
: The Hourly Suffix is the string used as the final portion of a snapshot name,
when that snapshot's period is "hourly." It should be a 1-12 character
alphanumeric string and is case-sensitive. By default, it is the string
"hourly"

**/Templates/templateName/Formatting/DailySuffix**
: The Daily Suffix is the string used as the final portion of a snapshot name,
when that snapshot's period is "daily." It should be a 1-12 character
alphanumeric string and is case-sensitive. By default, it is the string
"daily"

**/Templates/templateName/Formatting/WeeklySuffix**
: The Weekly Suffix is the string used as the final portion of a snapshot name,
when that snapshot's period is "weekly." It should be a 1-12 character
alphanumeric string and is case-sensitive. By default, it is the string
"weekly"

**/Templates/templateName/Formatting/MonthlySuffix**
: The Monthly Suffix is the string used as the final portion of a snapshot name,
when that snapshot's period is "monthly." It should be a 1-12 character
alphanumeric string and is case-sensitive. By default, it is the string
"monthly"

**/Templates/templateName/Formatting/YearlySuffix**
: The Yearly Suffix is the string used as the final portion of a snapshot name,
when that snapshot's period is "yearly." It should be a 1-12 character
alphanumeric string and is case-sensitive. By default, it is the string
"yearly"
