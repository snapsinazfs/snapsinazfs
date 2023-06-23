 # SnapsInAZfs

 A policy-based snapshot management utility inspired by sanoid

 ## Status

 As of today (2023-06-23), SnapsInAZfs is capable of taking and pruning snapshots, using configuration
 stored in ZFS itself, via user properties, for everything except the timing and naming settings, which are still
 in configuration files, and provides a TUI for making configuration easy.

 I may make an alpha or beta release tag soon and possibly provide a pre-built release, here on github. This comes with what should be the obvious disclaimer that this is an alpha/beta-stage project and you should not trust important systems with it.
 
 ## Project Organization
 
  - SnapsInAZfs: The main SnapsInAZfs executable. This project should only contain code relevant specifically to SnapsInAZfs. Also contains the configuration console TUI.
  - SnapsInAZfs.Interop: Code such as native platform calls, Concurrency management, and other Interop layer code, as
  well as types used to interact with and define structures relevant to ZFS, such as snapshots and datasets.
  - SnapsInAZfs.Settings: Contains formal definitions of settings in code. The .net configuration binder is used to
  populate an instance of this class, in SnapsInAZfs, and command line arguments are used to override individual
  values, as appropriate.
  - Test projects: Each class library project will have a test project defined, for unit tests. I'm not doing this
  by TDD, so these are likely to not be well-defined until later in the project development cycle, when things
  stabilize a bit.

 My intention is to keep the project/solution easily useable from both Visual Studio 2022+ on Windows as well as
 vscode on Linux (I will be using both environments to develop it). As I use ReSharper on Visual Studio in
 Windows, there may end up being some ReSharper-related files and directives in code. However, since ZFS and,
 therefore, SnapsInAZfs currenly are a Linux-targeted toolset, release tags of the project will always be guaranteed
 to compile and run under Linux (at minimum, Ubuntu 22.04 and later and RHEL/CentOS 8.6 and later, as that's what
 I have) with the project's required version of the dotnet runtime installed.

 ## Dependencies

 My intention is for this project to have no external dependencies other than the required version of the dotnet
 runtime that have to be manually installed by the end user. All compile-time
 dependencies of these applications will either be included in published releases or included as project
 references in the dotnet projects, so that they can be automatically restored by the dotnet SDK, from the public
 NuGet repository.\
 Runtime dependencies, such as mbuffer, ssh, and others that are invoked by SnapsInAZfs are, of course, still required
 to be installed and available, if they are used by your configuration, though I intend to implement some of that
 functionality in these applications, themselves, eventually.
 
 That said, here are the nuget package dependencies as of right now (automatically retrieved during build):
 
  - PowerArgs
  - JsonSchema.Net
  - Microsoft.Extensions.Configuration.Binder
  - Microsoft.Extensions.Configuration.Json
  - Microsoft.Extensions.Configuration.Ini
  - Microsoft.Extensions.Configuration.EnvironmentVariables
  - NLog
  - NLog.Extensions.Logging
  - NLog.Targets.Journald
  - JetBrains.Annotations
  - Teminal.Gui

  Additionally, `make` is ideal to be installed, as I've provided a Makefile with several useful build and test targets, to make things easier. Otherwise, you can manually run the commands in the Makefile to build. All build targets in the Makefile are bash-compatible scripts that assume standard coreutils and zfs 2.1 or higher are installed.

  Platform utilities should only be required for installation, and are mostly included in core-utils, so should be available on pretty much every standard linux distro. SnapsInAZfs itself uses native platform calls from libc, for the functionality that would otherwise be provided by those utilities, so the standard shared libraries included in most basic distro installs are all SnapsInAZfs needs to run properly (binaries only - header files are not needed). The goal is for SnapsInAZfs to only require you to have the dotnet7.0 runtime and zfs installed, for pre-built packages, or the dotnet7.0 SDK, in addition, to build from source.

 ## Installing From Source
 
 After cloning this repository, execute the following commands to build and install SnapsInAZfs using make:

     make
     make install

 This will fetch all .net dependencies from NuGet, build SnapsInAZfs in the ./publish/Release-R2R/ folder as a combined "Ready-to-Run" (partially natively pre-compiled) executable file, install SnapsInAZfs to `/usr/local/sbin/SnapsInAZfs`, install all base configuration files to `/usr/local/share/SnapsInAZfs/`, and install a local configuration file at `/etc/SnapsInAZfs/SnapsInAZfs.local.json`, making backups of any replaced configuration files along the way.

 ## Uninstalling From Source

 To uninstall, run `make uninstall`\
 This will delete the executable file from `/usr/local/bin` and delete the base configuration files from 
 `/usr/local/share/SnapsInAZfs`.\
 This will not remove any local configuration files in `/etc/SnapsInAZfs`.\
 This will also not remove the last build artifacts created by `make install`.

 To clean all build artifacts, run `make clean`.\
 To clean specific build target target artifacts, run `make clean-release` or `make clean-debug`.\
 To clean everything, including empty build directories, run `make extraclean`.

 ## Running

 After installing, SnapsInAZfs can be run from any shell, so long as your `$PATH` includes
 the  `/usr/local/sbin` directory and the system has the same version of .net installed as SnapsInAZfs was
 built on. The Makefile will not modify your PATH variable, as it shouldn't be necessary on most distros, since 
 SnapsInAZfs is installed in a common binary path. If `/usr/local/sbin` is not in your PATH, you should add it at
 least for the users intended to run it, or system-wide, if you so desire.
 
 ## Configuration

 **NB: SnapsInAZfs is an early alpha project, and its configuration is not to be considered stable until SnapsInAZfs reaches beta, at which time significant changes to configuration method and schema will be avoided as much as possible.**
 

 ### ZFS Properties

 Settings that apply to zfs and SnapsInAZfs's interaction with it are stored as custom user properties on the datasets themselves, using the `snapsinazfs.com:` namespace.

 When SnapsInAZfs runs, it will get all properties for all pool root datasets and use those values to inform its operation.

 ##### Missing Properties

 If SnapsInAZfs does not find values for all expected settings on each pool root dataset, it will let you know and then exit without making any changes to the system.

 If you want to find out which properties are missing, without making changes to the system, SnapsInAZfs can be run with the --check-zfs-properties command line parameter.\
 This will cause SnapsInAZfs to output a list of missing properties, by pool root dataset, and then exit without doing anything else.

 If properties are not defined, SnapsInAZfs can be invoked with the `--prepare-zfs-properties`
 command line switch.\
 This option will cause SnapsInAZfs to check all pool root datasets for properties that the current version of SnapsInAZfs understands, and define all missing settings, using default values for each, on those pool roots.\
 Default settings are safe, and will result in no snapshots being taken or pruned, unless you have changed
 something yourself.\
 Settings you have already defined will not be overwritten, and **obsolete properties will not be removed**.

 ##### Removing SnapsInAZfs ZFS Properties

 If you no longer wish to use SnapsInAZfs, you can clean up all of its settings from ZFS in either of two ways:

 -  To clean up zfs properties set by SnapsInAZfs, run `make save-snapsinazfs-zfs-properties` and then `make wipe-snapsinazfs-zfs-properties`. The first recipe will save all zfs properties that SnapsInAZfs has set, to enable full restore. The second completely removes all SnapsInAZfs properties from all datasets in all pools and will refuse to run unless the recovery script exists.\
 If you need to restore properties to the state they were in the last time you ran `make save-snapsinazfs-zfs-properties`, run `make restore-wiped-zfs-properties`, and the restore script will run, setting the properties back to how they were when you ran `make save-snapsinazfs-zfs-properties`
 - By manually calling `zfs inherit -r  $setting` on each pool root dataset, where $setting is the setting to remove from ZFS entirely. 
 
 Configuration is applied in the following order, with each element in the list superceding the settings defined at the level above it:
 
 - /usr/local/share/SnapsInAZfs.json (this is the base configuration and should never be modified by the user)
 - /etc/SnapsInAZfs/SnapsInAZfs.local.json
 - Command line arguments


 ## Contributing

 I'm more than happy to have anyone contribute to the project.

 If pull requests are submitted, when I have time, I will happily review and merge those that don't deviate from
 the core mission of the project in a significant way, and that don't completely duplicate something I'm already
 currently working on (that case should be uncommon, though, as I generally commit and push often).

 If you intend to contribute, please adhere to the code style of the project, which generally follows standard
 recommendations for C#, with small deviations if and when I think they improve the readability of the code. Most jarringly, that means native types and native function calls will generally use the names of their native counterparts.
 I will publish ReSharper settings files that reflect the general formatting rules used, once there is a
 significant amount of code in the project and the style has therefore become more stable.
 
 If you happen to _really_ want to contribute to this project in a major way, I'm happy to collaborate. For now,
 though, I would prefer for such collaboration to occur via pull requests from a fork of this repository, branched
 from the dotnet-port-master branch (I will not accept pull requests to master, as that's just intended to be
 a local reference to the original PERL-based sanoid repository, which this repository was forked from).

 ## License

 This project is licensed under the GNU General Public License, version 3.0