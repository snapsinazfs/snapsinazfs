 # SnapsInAZfs (SIAZ)

 A policy-based snapshot management utility and daemon for ZFS on Linux.

 ## Status
 
[![Latest 'build' Tag Status](https://github.com/snapsinazfs/snapsinazfs/actions/workflows/build-and-test-build-tag.yml/badge.svg)](https://github.com/snapsinazfs/snapsinazfs/actions/workflows/build-and-test-build-tag.yml)
[![Latest 'release' Tag Status](https://github.com/snapsinazfs/snapsinazfs/actions/workflows/build-and-test-release-tag.yml/badge.svg)](https://github.com/snapsinazfs/snapsinazfs/actions/workflows/build-and-test-release-tag.yml)
 
 Current stable release version: 2.0.1\
 Current development branch: siaz-2.0

 Stable and pre-release versions are available on GitHub in the Releases section. Cloning the repository for local builds is only recommended from release-\* tags.

 Please report any issues in the issue tracker, with as much detail as you can provide. Detailed logs are greatly appreciated and do not reveal secret information beyond the names of your ZFS datasets and snapshots.

 ## Requirements

 SIAZ is built in c\# on .net 8, and strives to require no other external dependencies to be manually installed other than the .net 8 runtim or .net 8 SDK (to build from source), and a supported version of ZFS (currently 2.0 and up).\
 Building SIAZ also requires standard command-line utilities that typically exist on any supported Linux distribution, such as `install`, `cp`, and the like.

 SIAZ will likely build and run on any modern Linux system that supports the required .NET version and ZFS on Linux version.\
 However, the following list of requirements are what the project is built and tested on by me, so is all I can say with reasonable certainty should work.\

 - An x64 CPU
 - An x64 build of one of the following Linux distributions:
   - Ubuntu 22.04 or higher
     - This includes equivalent KUbuntu releases
     - This also includes KDE Neon releases with equivalent Ubuntu versions as their upstream
   - RHEL 8.6 or higher
   - CentOS Stream 8.6 or higher
 - Microsoft .net 8.0 or higher
   - x64 versions of .net are the only versions SIAZ is supported on and it will refuse to build on other platforms
   - The releases made available in your package manager are preferred, but any properly installed .net runtime _should_ work
   - If you are using a pre-built version of SIAZ (not yet provided here), the .net 8.0 runtime is sufficient
   - If you are building from source, the .net 8.0 SDK is required. `make` is an optional dependency for a simplified build workflow familiar to Linux users.
   - If you are using a pre-built AOT-compiled version of SIAZ or a pre-build framework-independent version of SIAZ (not yet provided here), you do not need ANY .net components installed, but it is your responsibility to ensure binary compatibility with the target system.
 - ZFS version 2.1 or higher
   - Some features may require higher ZFS versions. That is and will be documented when relevant. It is your responsibility to ensure ZFS compatibility, in the version of the kernel module, the userspace utilities, and in the necessary feature flags on pools/datasets, where relevant.
   - Live testing against ZFS is performed against version 2.2 and 2.3, at present, so I suggest 2.2 or higher.
 - For use as a daemon, systemd is supported, with a minimum version of that which is distributed via your distribution's official package repositories
   - While it may be possible to use SIAZ as a daemon in other systems, the startup code in SIAZ will consider itself to be running as a console application, so you're on your own and will have to force daemon mode (read the manpages)
 - A command line terminal, if directly invoking SIAZ manually, or to use the SIAZ Configuration Console, which is a TUI similar to the Network Manager nmtui utility.
   - If you want to use the mouse capabilities of the configuration console (which even works over SSH), you of course will need a mouse and proper ssh/sshd/server-side configurations to allow that (has worked out of the box on all supported versions of linux I've tried it on)
   - I've tested it in the Gnome Terminal app, a linux local console, KDE Konsole, PuTTY, and Windows 11 under Windows Terminal and the legacy console host, and all work with equivalent features.
   - Advanced functionality of the config console may or may not work on "minimal" installs of supported distros, as that is not a configuration I have tested. Please report if you run into such issues.
 - Documentation is provided as GROFF-formatted text files intended to be consumed by `man`. Thus, the `mandb` command must be available and executable by the installing user to install or uninstall the manpages.
 - Write permissions to the destination deployment paths, during installation or uninstallation. These include various directories under /usr, /etc, and /var/log, by default (see installation documentation or the Makefile)

 ### File System Permissions
 - Execute permissions for the `zfs` and `zpool` utilities for the user or service account executing SIAZ.
 - Write permissions for the user or service account executing SIAZ, to the configured log directory (/var/log/SnapsInAZfs by default)
 - Write permissions to the location specified by the user when saving a configuration JSON file via the configuration console.
 - Read permissions to any directories created by the SIAZ installation as well as to any configuration files given to SIAZ at runtime, for the user or service account executing SIAZ at that time.

 ### MAC (SELinux or AppArmor, if in use) Permissions
 No configuration is currently provided here for configuring these systems.\
 If you are using them (and you really should be), ensure that the following are granted to the user or account installing, executing, or uninstalling SIAZ, as appropriate.\
 root or sudo equivalent of course works, but I encourage you to lock things down appropriately for your environment.
 - Relevant permissions to do all of the above.
 - Relevant permissions to open Unix sockets or other network sockets for listening or as outgoing connections, as defined in your configuration, if monitoring or other network-enabled functionality is in use.
 - Relevant permissions to allow SIAZ to launch the `zfs` and `zpool` utilities as child processes by SIAZ.
 - Relevant permissions for SIAZ to write to any logging targets defined in your SIAZ nlog configuration (file and stdout are the default targets on a new install)

 ### ZFS Permissions
 SIAZ directly launches the `zfs` and `zpool` utilities to carry out its ZFS-related operations.\
 Thus, if you are using ZFS permissions (set via the `zfs allow` or `zfs deny` commands), you need all of the above as well as the following:
 - ZFS permissions allowing get, set, and list operations on all pools SIAZ is configured to use, for the account executing SIAZ.
 - ZFS permissions allowing get, set, inherit, list, snapshot, destroy, rollback, send, and receive operations on all datasets (explicitly set or inherited) for the account executing SIAZ, including the configuration console.

 ### Network Configuration/Permissions
 If any network-enabled functionality of SIAZ is used, your system must have a properly-configured/working network configuration for those features.\
 While other protocols/transports may be possible to configure and may work, only UDP or TCP over IPv4 or IPv6, as well as Unix Sockets, are officially supported by SIAZ itself. NLog or other components may support additional protocols/transports, but support for that is out of scope for this project and you should refer to those components' official documentation for such configurations.\
 Beyond that, the following is required, where relevant, for use of those features:
 - Firewall configuration
   - Allow inbound traffic on configured interfaces/ports, as specified in your SIAZ configuration (such as the HTTP monitoring service or the replication service, when that is implemented)
   - Allow outbound traffic on configured interfaces/ports, as specified in your SIAZ configuration (such as remote logging targets or replication, when that is implemented)
   - SIAZ and the provided Makefile do not configure firewalls for you, so this is your responsibility
   - If you enable the provided sample monitoring service endpoint (disabled by default), this means you need an inbound allow rule for TCP port 60763 to the monitoring service. If enabled but no port is specified, .net Kestrel uses a default port of TCP/5000
 - If you use Unix Sockets for the monitoring service, the account executing SIAZ needs read and write permission to the socket, as does the account executing whatever application is consuming the service via that socket.
 - The monitoring service is an HTTP service, using HTTP/2 by default, but capable of other HTTP versions as specified in the Kestrel configuration.
   - Some common options are described in SIAZ man pages. For those that aren't, or for more information on the ones that are, see Microsoft documentation for available options. Look for Kestrel Configuration at Microsoft Learn.
   - For systems/networks with application-aware firewalls, you can use standard HTTP/HTTPS DPI capabilities for the monitoring service connections. SIAZ HTTP communication is RFC-compliant to whatever degree Kestrel is.
   - Monitoring service connections can be considered to be short-lived, as they are just simple JSON responses (or appropriate HTTP error codes, in error conditions). Actual size of response body varies by the requested monitoring metric and may also vary based on your configuration or, for certain error metrics, the number of errors SIAZ has available to report.

 ### Misc Dependency Commentary
 While it is of course possible (and likely) that various other configurations, such as other distros, other ZFS versions, etc may work, even out of the box, they are not officially supported.\
 If you encounter a problem in an unsupported configuration that I cannot duplicate on a supported configuration, you're on your own, though pull requests that address such issues are welcome.
 
 Hardware requirements beyond CPU aren't listed because there aren't really any beyond that. SIAZ itself does not require specific CPU features/SIMD instruction sets (at this time), and memory requirements are entirely dependent on your configuration (though, just to throw out a number that is in no way a lower or upper bound, release builds with minimal configuration running as a systemd daemon often clock in at (sometimes significantly) less than 64MB while executing snapshot operations (not including memory used by ZFS utilities).

 Compile-time dependencies are included as project references in the dotnet projects, and will be retrieved by the dotnet SDK, from the public NuGet repository, upon build. Thus, an internet connection is necessary during the build process, any time NuGet package references are changed in the SIAZ project files or if they do not exist in the build directories, where the dotnet SDK expects to see them.

 ## Building/Installing From Source
 
 After cloning this repository or extracting a release archive on a system that meets the requirements for the version of SIAZ you are installing, execute the following commands to build and install SnapsInAZfs using `make`:

     make
     make install
     #optionally, run the following to run the unit/integration tests:
     make test
 
 To install the systemd service unit for SIAZ, run `make install-service`, which ONLY installs the service unit
 to `/usr/lib/systemd/system/snapsinazfs.service`, enables it, and runs `systemctl daemon-reload`.\
 The `install-service` recipe will not build SIAZ or perform any other actions.

 For detailed installation documentation,
 including descriptions of all Makefile recipes, build configurations, and other relevant details,
 run `make help` from the repository root directory.

 The man page displayed by that make recipe is the authoritative installation guide and supercedes anything that may appear in this document, if discrepancies exist.

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

 Invoke SnapsInAZfs with `SnapsInAZfs`, `snapsinazfs`, or `siaz` (all are hard-linked to the same executable).

 ## Configuration

 Base application settings, including global program options, template definitions, and logging settings for SIAZ are in JSON-formatted files. See `snapsinazfs(5)` after install for detailed documentation.\
 All other configuration is specified in ZFS user properties, for options that apply to how SIAZ interacts with your datasets. See `snapsinazfs-zfsprops(7)` after install for detailed documentation.\

 ## Getting Help

 SIAZ comes with man pages that are installed when you run `make install`.\
 These man pages are:
 - snapsinazfs(8)
 - snapsinazfs(5)
 - snapsinazfs-zfsprops(7)
 - snapsinazfs-config-console(8)
 - snapsinazfs-monitoring(3)
 
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
 recommendations for C#, with small deviations if and when I think they improve the readability of the code.\
 I have included ReSharper settings files that reflect the general formatting rules used.
 
 If you happen to _really_ want to contribute to this project in a major way, I'm happy to collaborate. For now,
 though, I would prefer for such collaboration to occur via pull requests from a fork of this repository, branched
 from the master branch or a branch specifically created for a reported issue.

 ## License

 This project is licensed under the MIT License

 Copyright 2023 Brandon Thetford

 Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

 The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

 ## Acknowledgements

 While this project is a "clean room"/from-scratch implementation, I want to give a shout-out to "Sanoid", as it was inspired by and follows some of the core
 principles of "Sanoid," created by Jim Salter, for ease of migration for existing Sanoid users.\
 Sanoid is licensed under the GPL version 3.0, though the name belongs to Jim.

 My personal thanks go to Jim Salter and anyone who contributed to Sanoid, over the years, as it has been an
 indespensible tool in my professional life and my nerdy home tech life.

 The original Sanoid project, written in PERL, is located at https://github.com/jimsalterjrs/sanoid/
 
