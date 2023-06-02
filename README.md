 # Sanoid.net

 A .net 7.0+/C#11+ re-implementation (not a port) of sanoid

 ## Status

 As of today (2023-06-02), Sanoid.net is capable of taking (but not pruning) snapshots, using configuration
 stored in ZFS itself, via user properties. Snapshot pruning is the next work item.

 Once taking and pruning snapshots work as intended, I may make an alpha release tag and possibly provide a
 pre-built release, here on github. This comes with what should be the obvious disclaimer that this is an
 alpha-stage project and you should not trust important systems with it.

 I estimate taking all snapshot levels and pruning them as well should be finished within the next few days, if
 I'm able to put enough time into it. With the configuration in ZFS, just about every operation is now so much
 easier to handle, both on the dev side and the user side.

 
 ## Goals

 Ideally, the goal is to create a .net 7.0+ application having at least feature parity with the version/commit
 of sanoid that the [dotnet-port-master](../tree/dotnet-port-master) branch is synced with. As of this writing,
 that is sanoid release 2.1.0 plus all sanoid master branch commits up to
 [jimsalterjrs/sanoid@55c5e0ee09df7664cf5ac84a43a167a0f65f1fc0](https://github.com/jimsalterjrs/sanoid/commit/55c5e0ee09df7664cf5ac84a43a167a0f65f1fc0)

 I have been making various changes and enhancements along the way, so far, especially in the area of configuration, which is now going to depend on native facilities provided by ZFS, via user properties.\
 This will keep all of the zfs-related configuration in ZFS itself, and will make the settings and how they are going to apply to each dataset much easier both for the user and also for me, in developing Sanoid.net.

 ### Long-Term Goals

 I intend to, at some point, implement some functionality that PERL sanoid/syncoid currently depends on external utilities for, such as sending snapshots to remote machines, as native calls within the application, using sockets, openssl, etc, as appropriate. Same goes for compression of send streams.

 I also want to build Sanoid.net to eventually support running as a daemon, which could have various advantages, such as more consistent and reliable timestamps (since it will not depend on systemd timers to run) and anything else that having a long-running resident process may provide.

 #### Long-Term Lofty Goals

 I would love to, once Sanoid.net is stable, attempt to implement interaction with zfs via native calls through
 libzfs. This may or may not be easily achievable without *significant* additional work, to marshall all the
 various C++ constructs in zfs (some of which are quite cumbersome) into the .net world.

 I've already written a few basic P/Invoke methods to make calls directly to libc functions, to avoid spawning
 processes for equivalent commands, but zfs is a completely different beast.
 
 ## Compatibility With PERL-based sanoid
 
 Sanoid.net is intended to have *similar* behavior to PERL-based sanoid. Invocations of Sanoid.net will accept
 most of the same arguments as PERL sanoid, and will behave largely the same, except that it ***requires***
 configuration to be set in zfs. Sanoid.net can both check and update the required property schema for you.
 
 Sanoid.net will also have additional capabilities which are not guaranteed to be available in PERL sanoid.

 Sanoid.net is also planned to incorporate some functionality that PERL sanoid currently delegates to other
 utilities, where that functionality is either included in .net or where I get the motivation to do so.

 ## Project Organization

 I intend to organize the project differently than sanoid/syncoid/findoid, partially thanks to the benefit of hindsight, but also in an attempt to make it easier to maintain, modify, and extend.

 Most significantly, I intend to separate the code into one or more common library projects, to help avoid code
 duplication and aid in organization.
 
 Currently, the solution is laid out as follows:
 
  - Sanoid: The main Sanoid.net executable. This project should only contain code relevant specifically to Sanoid.net
  - Sanoid.Interop: Code such as native platform calls, Concurrency management, and other Interop layer code, as
  well as types used to interact with and define structures relevant to ZFS, such as snapshots and datasets.
  - Sanoid.Settings: Contains formal definitions of settings in code. The .net configuration binder is used to
  populate an instance of this class, in Sanoid.net, and command line arguments are used to override individual
  values, as appropriate.
  - Sanoid.Common: Intended to be a common library that Sanoid.net, Syncoid.net, and Findoid.net use, with types
  relevant to all, but which don't belong in Sanoid.Interop
  - Test projects: Each class library project will have a test project defined, for unit tests. I'm not doing this
  by TDD, so these are likely to not be well-defined until later in the project development cycle, when things
  stabilize a bit.
  - Syncoid: Doesn't exist yet, but will be the Syncoid.net utility
  - Findoid: Doesn't exist yet, but will be the Findoid.net utility

 Sanoid.net, Syncoid.net, and Findoid.net will, themselves, remain individual applications that can be invoked
 with the same or very similar commands and arguments as their PERL ancestors, plus anything new Sanoid.net provides.

 My intention is to keep the project/solution easily useable from both Visual Studio 2022+ on Windows as well as
 vscode on Linux (I will be using both environments to develop it). As I use ReSharper on Visual Studio in
 Windows, there may end up being some ReSharper-related files and directives in code. However, since ZFS and,
 therefore, sanoid currenly are a Linux-targeted toolset, release tags of the project will always be guaranteed
 to compile and run under Linux (at minimum, Ubuntu 22.04 and later and RHEL/CentOS 8.6 and later, as that's what
 I have) with the project's required version of the dotnet runtime installed.

 ## Dependencies

 My intention is for this project to have no external dependencies other than the required version of the dotnet
 runtime that have to be manually installed by the end user. This includes not being reliant on the PERL versions
 of the applications, either, or any of their configuration files or required perl packages. All compile-time
 dependencies of these applications will either be included in published releases or included as project
 references in the dotnet projects, so that they can be automatically restored by the dotnet SDK, from the public
 NuGet repository.\
 Runtime dependencies, such as mbuffer, ssh, and others that are invoked by sanoid are, of course, still required
 to be installed and available, if they are used by your configuration, though I intend to implement some of that
 functionality in these applications, themselves, eventually.
 
 That said, here are the nuget package dependencies as of right now (automatically retrieved during build):
 
  - PowerArgs
  - JsonSchema.Net
  - Microsoft.Extensions.Configuration.Binder
  - Microsoft.Extensions.Configuration.Json
  - Microsoft.Extensions.Configuration.Ini
  - Microsoft.Extensions.Configuration.EnvironmentVariables
  - Microsoft.Extensions.Configuration.CommandLine
  - NLog
  - NLog.Extensions.Logging
  - NLog.Targets.Journald
  - JetBrains.Annotations

  Additionally, `make` is ideal to be installed, as I've provided a Makefile with several useful build and test targets, to make things easier. Otherwise, you can manually run the commands in the Makefile to build. All build targets in the Makefile are bash-compatible scripts that assume standard coreutils are installed. I may also use autoconf, at a later time, though it hasn't been strictly necessary, so far.

  Platform utilities should only be required for installation, and are mostly included in core-utils, so should be available on pretty much every standard linux distro. Sanoid.net itself uses native platform calls from libc, for the functionality that would otherwise be provided by those utilities, so the standard shared libraries included in most basic distro installs are all Sanoid.net needs to run properly (binaries only - header files are not needed). The goal is for Sanoid.net to only require you to have the dotnet7.0 runtime and zfs installed, for pre-built packages, or the dotnet7.0 SDK, in addition, to build from source.

 ## Installing From Source
 
 After cloning this repository, execute the following commands to build and install Sanoid.net using make:

     cd sanoid/dotnet
     make install-release

 This will fetch all .net dependencies from NuGet, build Sanoid.net in the ./publish/Release-R2R/ folder as a combined "Ready-to-Run" (partially natively pre-compiled) executable file, install Sanoid.net to usr/local/bin/Sanoid` (also hard-linked to `/usr/local/bin/Sanoid.net`), install all base configuration files to `/usr/local/share/Sanoid.net/`, and install a local configuration file at `/etc/sanoid/Sanoid.local.json`, making backups of any replaced configuration files along the way.

 ## Uninstalling From Source

 To uninstall, run `make uninstall`\
 This will delete the executable file from `/usr/local/bin`, delete the base configuration files from 
 `/usr/local/share/Sanoid.net`, and delete the local configuration file at `/etc/sanoid/Sanoid.local.json`.\
 This will not remove any local configuration files in `~/.config/Sanoid.net`.\
 This will also not remove the last build artifacts created by `make install`.

 To clean all build artifacts, run `make clean`\
 To clean specific build target target artifacts, run `make clean-release` or `make clean-debug`

 ## Installing From Pre-Built Packages

 Once Sanoid.net is in a functional beta stage, I intend to provide .deb and .rpm packages that will install the
 pre-built application, as a framework-dependent (requires dotnet runtime). When Sanoid.net is ready for release,
 I intend to provide packages with both framework-dependent and framework-independent builds. Packages will 
 install to the same folders and have identical functionality and behavior as those built from source.

 ## Running

 After installing, Sanoid.net can be run from any shell, so long as your `$PATH` includes
 the  `/usr/local/bin` directory and the system has the same version of .net installed as Sanoid.net was
 built on. The Makefile will not modify your PATH variable, as it shouldn't be necessary on most distros, since 
 Sanoid.net is installed in a common binary path. If `/usr/local/bin` is not in your PATH, you should add it at
 least for the users intended to run it, or system-wide, if you so desire.

 You can also invoke it directly from the project folder itself by running `dotnet run`.
 
 ## Configuration

 **NB: Sanoid.net is an early alpha project, and its configuration is not to be considered stable until Sanoid.net reaches beta, at which time significant changes to configuration method and schema will be avoided as much as possible.**

 Configuration for Sanoid.net will be quite different from PERL sanoid.\
 Most importantly, many/most settings that PERL sanoid currently keeps in text files will instead be stored directly in ZFS, using custom user properties.\
 Settings not relevant to ZFS or Sanoid.net's interaction with it will be stored in JSON configuration files.\
 Templates and timing/retention settings will also stay in JSON, to avoid setting a ton of user properties in zfs and to make quick changes easier for the user.
 
 Continue reading below for specifics on where and how settings are stored.

 ### Global Sanoid.net Settings

 Various global settings will still be in text files, formatted as JSON, with a schema distributed along with Sanoid.net.

 #### Settings currently defined as global:

 This table lists all settings currently accepted in the global configuration and what they do.\
 These settings are defined at the root level of the Sanoid.local.json file and must follow the schema in the
 Sanoid.local.schema.json document, using standard JSON rules for data types as shown.

 | Setting Key | Type      | Default | Description |
 | :---------- | :-------- | :------ | :---------- |
 | DryRun | boolean | false | If true, Sanoid.net will not make any changes to ZFS, and will attempt to output what it would have done if this setting were false[^dryrunnote1] |
 | TakeSnapshots | boolean | false | If true, Sanoid.net will run its snapshot functionality, for taking new snapshots. |
 | PruneSnapshots | boolean | false | If true, Sanoid.net will run its snapshot pruning functionality. |
 | Templates | Template[^templatesnote1][^templatesnote3] | [^templatesnote2] | A dictionary of string names to template definitions, as defined in Sanoid.template.schema.json] |
 | CacheDirectory | string[^cachedir1] | /var/cache/sanoid | The directory Sanoid.net will use to store cache data, as needed.[^cachedir2][^cachedir3] |
 | Formatting | Formatting[^formatting1] | | Definitions for the various components of snapshot names. |
 | ZfsPath | string[^zfspath1] | /usr/local/sbin/zfs | The path to the zfs utility |

 [^dryrunnote1]: Setting TakeSnapshots or PruneSnapshots to true will still control Sanoid.net's output, but no changes will be made to ZFS.

 [^templatesnote1]: A JSON object. The defintion of a Template is in Sanoid.template.schema.json, and will also be published online.

 [^templatesnote2]: A default template is defined in the core Sanoid.json file, which MUST exist. It is strongly recommended not to modify the default template. If you want to use other settings, define your own templates.

 [^templatesnote3]: Must be valid as a user property identifier in ZFS. See the User Properties section of the zfsprops(7) man page for details. Default is `sanoid.net` and is not recommended to be changed

 [^cachedir1]: Must be a valid path string.

 [^cachedir2]: If the directory does not exist, it will be created.

 [^cachedir3]: Any user that executes Sanoid.net must have read and write access to this directory.

 [^formatting1]: The schema for this section can be found in Sanoid.schema.json.

 [^zfspath1]: Must be a valid path pointing to the zfs utility itself or a resolveable link to it, that the user running Sanoid.net has execute access to.

 #### ZFS Properties

 Settings that apply to zfs and Sanoid's interaction with it will be stored as custom user properties on the datasets themselves.

 When Sanoid.net runs, it will get all properties for all datasets and use those values to inform its operation.

 ##### Missing Properties

 If Sanoid.net does not find values for all expected settings on each pool root dataset, it will let you know and then exit without making any changes to the system.

 If you want to find out which properties are missing, without making changes to the system, Sanoid.net can be run with the --check-zfs-properties command line parameter.\
 This will cause Sanoid.net to output a list of missing properties, by pool root dataset, and then exit without doing anything else.

 If properties are not defined, Sanoid.net can be invoked with the `--prepare-zfs-properties`
 command line switch.\
 This option will cause Sanoid.net to check all pool root datasets for properties that the current version of Sanoid.net understands, and define all missing settings, using default values for each, on those pool roots.\
 Default settings are safe, and will result in no snapshots being taken or pruned.\
 Settings you have already defined will not be overwritten, and obsolete properties will not be removed.

 ##### Removing Sanoid.net ZFS Properties

 If you no longer wish to use Sanoid.net, you can clean up all of its settings from ZFS in either of two ways:

 - By running Sanoid.net with the `--remove-zfs-properties` command line switch, which will clear all Sanoid.net settings on all datasets via a call to `zfs inherit $setting -r` on every pool root dataset. Once a user property is no longer explicitly set to a value other than inherit, in ZFS, ZFS removes the property completely.
 - By manually calling `zfs inherit -r  $setting` on each pool root dataset, where $setting is the setting to remove from ZFS entirely. 
 
 Configuration is applied in the following order, with each element in the list superceding the settings defined at the level above it:
 
 - /usr/local/share/Sanoid.json (this is the base configuration and should never be modified by the user)
 - /etc/sanoid/Sanoid.local.json
 - $HOME/.config/Sanoid.net/Sanoid.user.json
 - Sanoid.local.json  (In the same path as the executable. Facilitates administrator overrides for user local configurations. This file is not installed by default. Also used when executing Sanoid.net directly from the build directory.)
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

 Jim Salter's sanoid project is licensed under the GNU General Public License, version 3.0, as retrieved from
 http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17, as stated at the top of
 [sanoid](https://github.com/jimsalterjrs/sanoid/blob/master/sanoid) itself. Thus, the majority of this project is
 required to and happily uses the same license. If any component both can have a different license and I _want_ to
 give that component a different license, a GPL-compatible license such as MIT will be used, but that will be
 conspicuously mentioned if it happens.
