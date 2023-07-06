 # SnapsInAZfs (SIAZ)

 A policy-based snapshot management utility inspired by sanoid

 ## Status
 
[![Latest 'build' Tag Status](https://github.com/snapsinazfs/snapsinazfs/actions/workflows/build-and-test-build-tag.yml/badge.svg)](https://github.com/snapsinazfs/snapsinazfs/actions/workflows/build-and-test-build-tag.yml)
[![Latest 'release' Tag Status](https://github.com/snapsinazfs/snapsinazfs/actions/workflows/build-and-test-release-tag.yml/badge.svg)](https://github.com/snapsinazfs/snapsinazfs/actions/workflows/build-and-test-release-tag.yml)
 
 As of today (2023-07-05), SnapsInAZfs is capable of taking and pruning snapshots, using configuration
 stored in ZFS itself, via user properties, for everything except the timing and naming settings, which are still
 in configuration files, and provides a TUI for making configuration easy.
 
 SnapsInAZfs is also capable of running as a systemd service. A unit file and `make` recipe for installing and
 uninstalling the service are included.

 Version 1.0.0-Beta-3 has been tagged as a release on github, for testing. This comes with what should be the
 obvious disclaimer that this is a beta-stage project and you should not trust important systems with it.\
 That said, I'm running it on my home lab as well as a non-critical production system at work, and it's
 behaving as expected for my use cases, so far. 
 
 ## Project Organization
 
  - SnapsInAZfs: The main SnapsInAZfs executable. This project should only contain code relevant specifically to SnapsInAZfs. Also contains the configuration console TUI.
  - SnapsInAZfs.Interop: Code such as native platform calls, Concurrency management, and other Interop layer code, as
  well as types used to interact with and define structures relevant to ZFS, such as snapshots and datasets.
  - SnapsInAZfs.Settings: Contains formal definitions of settings in code. The .net configuration binder is used to
  populate an instance of this class, in SnapsInAZfs, and command line arguments are used to override individual
  values, as appropriate.
  - Test projects: Each class library project will have a test project defined, for unit tests. I didn't initially
  write this using TDD, so these are not yet well-defined. I am working on adding tests right now. I won't tag a
  non-beta release until I've gotten significant coverage from the unit tests and fixed anything they reveal.

 My intention is to keep the project/solution easily useable from both Visual Studio 2022+ on Windows as well as
 vscode on Linux (I will be using both environments to develop it). As I use ReSharper on Visual Studio in
 Windows, there may end up being some ReSharper-related files and directives in code. However, since ZFS and,
 therefore, SnapsInAZfs currenly are a Linux-targeted toolset, release tags of the project will always be guaranteed
 to compile and run under Linux (at minimum, Ubuntu 22.04 and later and RHEL/CentOS 8.6 and later, as that's what
 I have) with the project's required version of the dotnet runtime installed.

 ## Dependencies

 SIAZ has no external run-time dependencies for core functionality other than the required version of the dotnet runtime
 and ZFS itself that have to be manually installed by the end user.
 
 Compile-time dependencies will either be included in pre-packaged releases or included as project
 references in the dotnet projects, so that they can be automatically restored by the dotnet SDK, from the public
 NuGet repository.\
 
 Other runtime dependencies, such as mbuffer, ssh, and others that may be invoked by SnapsInAZfs are, of course, still required
 to be installed and available, if they are used by your configuration, though I intend to implement some of that
 functionality natively, eventually.
 
 Additionally, `make` is ideal to be installed, as I've provided a Makefile with several useful build and test targets, to make things easier. Otherwise, you can manually run the commands in the Makefile to 
 build. All build targets in the Makefile are bash-compatible scripts that assume standard coreutils and zfs 2.1 or higher are installed.

 Platform utilities should only be required for installation, and are mostly included in core-utils, so should be available on pretty much every standard linux distro. SnapsInAZfs itself uses native platform calls from libc, for the functionality that would otherwise be provided by those utilities, so the standard shared libraries included in most basic distro installs are all SnapsInAZfs needs to run properly (binaries only - header files are not needed). The goal is for SnapsInAZfs to only require you to have the dotnet7.0 runtime and zfs installed, for pre-built packages, or the dotnet7.0 SDK, in addition, to build from source.

 ## Building/Installing From Source
 
 After cloning this repository, execute the following commands to build and install SnapsInAZfs using make:

     make
     make install

 This will fetch all .net dependencies from NuGet, build SnapsInAZfs in the ./publish/Release-R2R/ folder as a
 combined "Ready-to-Run" (partially natively pre-compiled) executable file, install SnapsInAZfs to
 `/usr/local/sbin/SnapsInAZfs`, install all base configuration files to `/usr/local/share/SnapsInAZfs/`,
 and install a local configuration file at `/etc/SnapsInAZfs/SnapsInAZfs.local.json`, making backups of any
 replaced configuration files along the way.

 To clean build artifacts, run `make clean`, which removes the release build files and intermediates,
 or `make extraclean`, which removes build artifacts and intermediates for all defined project
 configurations and removes the `build` and `publish` folders, as well. The `clean` and `extraclean` recipes do
 not touch anything outside the repository folder, such as installed binaries, documentation, or configuration.
 
 To install the systemd service unit for SIAZ, run `make install-service`, which ONLY installs the service unit
 to /usr/lib/systemd/system/snapsinazfs.service, enables it, and runs `systemctl daemon-reload`.\
 The `install-service` recipe will not build SIAZ or perform any other actions.

 ## Uninstalling

 To uninstall, run `make uninstall` from the repository root directory.\
 This will delete the executable file from `/usr/local/sbin`, delete the base configuration files from 
 `/usr/local/share/SnapsInAZfs`, delete installed man pages, and update the mandb accordingly.\
 This will not remove any local configuration files in `/etc/SnapsInAZfs`.\
 This will also not remove the last build artifacts created by `make install`, nor will it uninstall the service,
 if it has been installed. To do so, either run `make uninstall-service` or `make uninstall-everything`.
 
 To uninstall the systemd service unit for SIAZ, run `make uninstall-service`, which stops the service unit,
 disables it (to remove alias symlinks), removes the service unit file from /usr/lib/systemd/system/snapsinazfs.service,
 and runs `systemctl daemon-reload`.

 To clean all build artifacts, run `make clean`.\
 To clean specific build target target artifacts, run `make clean-release` or `make clean-debug`.\
 To clean everything, including build directories, run `make extraclean`.

 ## Running

 After installing, SnapsInAZfs can be run from any shell, so long as your `$PATH` includes
 the  `/usr/local/sbin` directory and the system has the same version of .net installed as SnapsInAZfs was
 built on. The Makefile will not modify your PATH variable, as it shouldn't be necessary on most distros, since 
 SnapsInAZfs is installed in a common binary path. If `/usr/local/sbin` is not in your PATH, you should add it at
 least for the users intended to run it, or system-wide, if you so desire.
 
 ## Configuration

 Configuration for SIAZ is in JSON-formatted files, for global program options and template definitions,
 and in ZFS user properties, for options that apply to how SIAZ interacts with your datasets.

 ### ZFS Properties

 Settings that apply to zfs and SnapsInAZfs's interaction with it are stored as custom user properties on the datasets themselves, using the `snapsinazfs.com:` namespace.

 When SnapsInAZfs runs, it will get all properties for all pool root datasets and use those values to inform its operation.

 #### Missing Properties

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

 ## Getting Help

 SIAZ comes with man pages that are installed when you run `make install`. Just type `man SnapsInAZfs` at the
 command line for general program help or `man SnapsInAZfs.5` for configuration help. The source documents for
 the man pages are markdown files which can also be viewed in any markdown-compatible viewer, such as github.com
 itself. The files are located in the `/Documentation` folder, in the repository. `.md` files are the markdown
 source files, and `.#` files are files formatted for man, translated from markdown by `pandoc`.

 The code itself is also heavily documented and quite verbose, so you may be able to find what you're looking for
 in the code. I have intentionally left some constructs which could be simplified in code as separate or more
 verbose operations, specifically to make it easier to read and understand. The compiler can easily optimize those
 situations away, so there is no performance concern. Even debug builds of SIAZ run quite fast with thousands of
 ZFS objects on modest hardware.

 If you find a bug or odd behavior in SIAZ, issues can be filed here. Note, however, that this is not a technical
 support board and SIAZ is not a commercial product, and there are no warranties, implied or otherwise. This
 includes support. While I will do my best to help out, if an issue is filed and is more than just asking for
 configuration help or similar, there is no guarantee your issue will ever be fixed, addressed, or even seen.\
 Unless, of course, you want to sponsor me with a fat check. Then I might consider giving you priority. ;)

 Constructive criticism, suggestions, and feature requests are always welcome, though they are at my or other
 contributors' discretion to address, implement, etc.

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
 from the master branch or a branch specifically created for a reported issue.

 ## License

 This project is licensed under the GNU General Public License, version 3.0

 ## Acknowledgements

 While this project is a "clean room"/from-scratch implementation, it was inspired by and follows many of the core
 principles of "Sanoid," created by Jim Salter. Sanoid is also licensed under the GPL version 3.0, though the name
 belongs to Jim. He graciously offered to allow this project to use a name that had "sanoid" in it, so long as
 it was unique enough not to be easily confused with sanoid (very understandable!), but I decided to just go
 ahead and pick a new name.

 My personal thanks go to Jim Salter and anyone who contributed to Sanoid, over the years, as it has been an
 indespensible tool in my professional life and my nerdy home tech life.

 The original Sanoid project, written in PERL, is located at https://github.com/jimsalterjrs/sanoid/

 Jim also is a very helpful resource for all things ZFS and is a moderator in the [reddit zfs community](https://www.reddit.com/r/zfs/).
