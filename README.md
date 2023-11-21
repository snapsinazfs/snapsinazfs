 # SnapsInAZfs (SIAZ)

 A policy-based snapshot management utility inspired by sanoid

 ## Status
 
[![Latest 'build' Tag Status](https://github.com/snapsinazfs/snapsinazfs/actions/workflows/build-and-test-build-tag.yml/badge.svg)](https://github.com/snapsinazfs/snapsinazfs/actions/workflows/build-and-test-build-tag.yml)
[![Latest 'release' Tag Status](https://github.com/snapsinazfs/snapsinazfs/actions/workflows/build-and-test-release-tag.yml/badge.svg)](https://github.com/snapsinazfs/snapsinazfs/actions/workflows/build-and-test-release-tag.yml)
 
 Current stable release version: 1.1.0\
 Current development branch: siaz-2.0.0

 Stable and pre-release versions are available on GitHub in the Releases section. Cloning the repository for local builds is only recommended from release-\* tags.

 Please report any issues in the issue tracker, with as much detail as you can provide. Detailed logs are greatly appreciated and do not reveal secret information beyond the names of your ZFS datasets and snapshots.

 ## Building/Installing From Source
 
 After cloning this repository or extracting a release archive, execute the following commands to build and install SnapsInAZfs using `make`:

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
 
 ## Dependencies

 SIAZ has no external run-time dependencies for core functionality other than the required version of the dotnet runtime
 and ZFS itself that have to be manually installed by the end user.
 
 Compile-time dependencies are included as project references in the dotnet projects, and will be retrieved by the dotnet SDK, from the public NuGet repository, upon build.

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

 Jim also is a very helpful resource for all things ZFS and is a moderator in the [reddit zfs community](https://www.reddit.com/r/zfs/).
