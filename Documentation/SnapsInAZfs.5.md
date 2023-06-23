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

**/ZfsPath** (string)
: This is the path to the `zfs` executable. The default path should be correct
for most installations. Default is "/usr/local/sbin/zfs". This path must point
to your zfs executable or a hard or symbolic link to it, and must be readable
and executable by the user that will execute sanoid, including for running the
configuration console.

**/ZpoolPath** (string)
: This is the path to the `zpool` executable. The default path should be correct
for most installations. Default is "/usr/local/sbin/zpool". This path must point
to your zpool executable or a hard or symbolic link to it, and must be readable
and executable by the user that will execute sanoid, including for running the
configuration console.

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

### Template Snapshot Timing Section

The **SnapshotTiming** section of a template allows you to fine-tune the times
at which Sanoid.net will take specific types of snapshots, and when existing
snapshots will be considered eligible for pruning.

Default settings correspond to the same behavior that PERL sanoid exhibits.

**/Templates/templateName/SnapshotTiming/UseLocalTime**
: This setting is currently not used by Sanoid.net and is reserved for future
changes. When this setting is implemented, it will control whether Sanoid.net
uses local system time (true) or UTC (false) for snapshot timestamps and all
associated processing, such as calculating eligibility for pruning. The
current behavior is the same as a setting of "true"

**/Templates/templateName/SnapshotTiming/FrequentPeriod**
: This setting is a period, in minutes, for frequent snapshots to be processed.
This value should be a whole-number factor of 60, such as 5, 10, 15, or 20.
While other values may work, they are not recommended nor are they supported.
The value must be an un-quoted integer from 1 to 59.

**/Templates/templateName/SnapshotTiming/HourlyMinute**
: This setting is the minute of the hour on which Sanoid.net will take hourly
snapshots. This value must be an un-quoted whole number from 0 to 59. The
default value is 0, meaning that snapshots will be taken at the top of the
following hour. For example, the hourly snapshot for the hour period from
11:00 to 11:59 will be taken at 12:00.

**/Templates/templateName/SnapshotTiming/DailyTime**
: This setting is a time string, in HH:mm:ss format (0-fill required for all
components), and is the time of day that daily snapshots will be taken. Time
strings can be any valid time of day from 00:00:00 to 23:59:59 and may
optionally include fractional seconds, up to 7 decimal places, though the
accuracy of actual snapshot timing will depend on the precision of your
system's clock and the precision of the mechanism used to invoke Sanoid.net.
For example, a time string of "12:34:56.789" is perfectly legal. This will
be more reliable once Sanoid.net has the ability to run as a daemon.

**/Templates/templateName/SnapshotTiming/WeeklyDay**
: This setting is a number, from 0 to 6, specifying the day of the week on
which weekly snapshots will be taken. Note that Sanoid.net attempts to be
culture-aware, and the meaning of the number may depend on your system
locale's definition of a week and which day is the start of that week.
In the invariant or US culture, 0 is Sunday. The default setting is 1,
which, in the invariant or US culture is Monday, which is the default
behavior of PERL sanoid, as well. The configuraiton colsole will present
this setting to you as the name of the days of the week, in the system
locale's language and calendar.

**/Templates/templateName/SnapshotTiming/WeeklyTime**
: As with DailyTime, this setting is the time of day at which weekly
snapshots will be taken. The rules, restrictions, and default value are the
same as for DailyTime. See DailyTime for details.

**/Templates/templateName/SnapshotTiming/MonthlyDay**
: This setting is the day of the month on which monthly snapshots will be
taken. It is a number from 1 to the system locale's maximum day number for
a month. If the value is set higher than a given month's last day, snapshots
will be taken on the last day of the month (so a setting of 31 will always
take a snapshot on the last day of the month, on the Gregorian calendar).
The default value is 1, meaning the first day of every month.

**/Templates/templateName/SnapshotTiming/MonthlyTime**
: As with DailyTime, this setting is the time of day at which monthly
snapshots will be taken. The rules, restrictions, and default value are the
same as for DailyTime. See DailyTime for details.

**/Templates/templateName/SnapshotTiming/YearlyMonth**
: This setting is the number of the month of the year in which yearly snapshots
will be taken. It is a whole-number value from 1 to the number corresponding to
the last month of the year, in the system locale's calendar (12, for Gregorian).
The configuration console will present this setting to you as the names of the
months of the year, in the system locale's language and calendar.

**/Templates/templateName/SnapshotTiming/YearlyDay**
: As with MonthlyDay, this settings is the day of the month specified in
YearlyMonth on which yearly snapshots will be taken. The rules, restrictions,
and default are the same as for MonthlyDay. See MonthlyDay for details.

**/Templates/templateName/SnapshotTiming/YearlyTime**
: As with DailyTime, this setting is the time of day at which yearly
snapshots will be taken, on the YearlyMonth and YearlyDay specified. The rules,
restrictions, and default value are the same as for DailyTime. See DailyTime
for details.

# Examples

These are example valid configurations.

## Default Base Configuration (/usr/local/share/Sanoid.net/Sanoid.json)

This is the default configuration shipped in Sanoid.json, which is the base
configuration file Sanoid.net builds the rest of its configuration from and is
not intended to be modified by the user. Elements beginning with a \$ symbol
are metadata and do not affect operation of Sanoid.net itself (though they
should not be modified by the user, as that may affect schema validation).

This configuration results in a default invocation of Sanoid.net performing
no actions that change ZFS (snapshots and pruning are disabled). Naming and
timing settings for the default template included in this configuration are
the same as PERL sanoid's defaults.

```
{
  "$schema": "Sanoid.schema.json",
  "$id": "Sanoid.json",
  "$comments": "Default settings for sanoid.net. It is not recommended to modify this file. Customized settings should be specified in /etc/sanoid/Sanoid.local.json",
  "TakeSnapshots": false,
  "PruneSnapshots": false,
  "CacheDirectory": "/var/cache/sanoid",
  "ZfsPath": "/usr/local/sbin/zfs",
  "ZpoolPath": "/usr/local/sbin/zpool",
  "DryRun": false,
  "Monitoring": {
    "Nagios": {
      "$comments": "Nagios-specific monitoring options",
      "MonitorType": "Nagios",
      "Capacity": false,
      "Health": false,
      "Snapshots": false
    }
  },
  "Templates": {
    "default": {
      "Formatting": {
        "ComponentSeparator": "_",
        "Prefix": "autosnap",
        "TimestampFormatString": "yyyy-MM-dd_HH\\:mm\\:ss",
        "FrequentSuffix": "frequently",
        "HourlySuffix": "hourly",
        "DailySuffix": "daily",
        "WeeklySuffix": "weekly",
        "MonthlySuffix": "monthly",
        "YearlySuffix": "yearly"
      },
      "SnapshotTiming": {
        "UseLocalTime": true,
        "FrequentPeriod": 15,
        "HourlyMinute": 0,
        "DailyTime": "00:00:00",
        "WeeklyDay": 1,
        "WeeklyTime": "00:00:00",
        "MonthlyDay": 1,
        "MonthlyTime": "00:00:00",
        "YearlyMonth": 1,
        "YearlyDay": 1,
        "YearlyTime": "00:00:00"
      }
    }
  }
}
```

## Default Local Configuration (/etc/sanoid/Sanoid.local.json)

This is the default local configuration shipped with Sanoid.net. This is the
configuration that is intended to be modified by the user and supercedes the
base configuration as described at the top of this help document. Keys and
values are case-sensitive. Whitespace not contained within quotation marks
is ignored.

This configuration also defaults to NO actions being taken, so that a fresh
install does not result in changes being made to ZFS. See the sections above
for explanations of what each setting does.

This configuration includes a template called "production," which has the
same settings as the default template, on a new install. Additional templates
can be added and removed, and the "production" template can be modified or
removed as desired by the user.

It is strongly recommended that all modifications to this configuration be
made using the configuration console, by invoking Sanoid.net with the
\-\-config-console command-line argument. This will ensure that your settings
are valid and that in-use templates are not accidentally removed.

Note that, at this time, the monitoring functionality is not yet implemented,
so the Monitoring section will be ignored.

```
{
  "$schema": "Sanoid.local.schema.json",
  "$id": "Sanoid.local.json",
  "$comments": "Values specified here supersede and extend the base configuration in Sanoid.json.",
  "CacheDirectory": "var/cache/sanoid",
  "ZfsPath": "/usr/local/sbin/zfs",
  "ZpoolPath": "/usr/local/sbin/zpool",
  "TakeSnapshots": false,
  "PruneSnapshots": false,
  "DryRun": false,
  "Monitoring": {
    "Nagios": {
      "$comments": "Nagios-specific monitoring options",
      "MonitorType": "Nagios",
      "Capacity": false,
      "Health": false,
      "Snapshots": false
    }
  },
  "Templates": {
    "production": {
      "Formatting": {
        "ComponentSeparator": "_",
        "Prefix": "autosnap",
        "TimestampFormatString": "yyyy-MM-dd_HH\\:mm\\:ss",
        "FrequentSuffix": "frequently",
        "HourlySuffix": "hourly",
        "DailySuffix": "daily",
        "WeeklySuffix": "weekly",
        "MonthlySuffix": "monthly",
        "YearlySuffix": "yearly"
      },
      "SnapshotTiming": {
        "UseLocalTime": true,
        "FrequentPeriod": 15,
        "HourlyMinute": 0,
        "DailyTime": "00:00:00",
        "WeeklyDay": 1,
        "WeeklyTime": "00:00:00",
        "MonthlyDay": 1,
        "MonthlyTime": "00:00:00",
        "YearlyMonth": 1,
        "YearlyDay": 1,
        "YearlyTime": "00:00:00"
      }
    }
  }
}
```