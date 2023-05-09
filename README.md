 # Sanoid.net

 A .net 7.0+/C#11+ port of sanoid
 
 ## Why?

 I wanted to learn more than the ultra-basic PERL I have gleaned over the years, and porting from one language to
 another is a good way to learn a language.\
 I use sanoid on several systems, and it is not a huge project, so it seemed like a decent project to tackle.

 ## Goals

 Ideally, the goal is to create a .net 7.0+ application having feature parity with the version/commit of sanoid
 that the [dotnet-port-master](../tree/dotnet-port-master) branch is synced with. As of this writing, that is
 sanoid release 2.1.0 plus all sanoid master branch commits up to
 [jimsalterjrs/sanoid@55c5e0ee09df7664cf5ac84a43a167a0f65f1fc0](https://github.com/jimsalterjrs/sanoid/commit/55c5e0ee09df7664cf5ac84a43a167a0f65f1fc0)
 
 ## Compatibility With PERL-based sanoid
 
 Sanoid.net is intended to be at least mostly compatible with PERL-based sanoid. Invocations of Sanoid.net will
 accept the same arguments as PERL sanoid, and will behave the same, except for default timing settings 
 (explained below) and logging output, which is via nlog.
 
 All settings are configurable by the end user and can be made to match PERL sanoid, if desired.
 
 Sanoid.net may also have additional capabilities which are not guaranteed to be available in PERL sanoid.
 
 Thus, sanoid => Sanoid.net portability should be fairly seemless, but Sanoid.net => sanoid portability is only
 guaranteed if no Sanoid.net enhancements are used.

 ## Project Organization

 I intend to organize the project differently than sanoid/syncoid/findoid, partially thanks to the benefit of
 hindsight, but also in an attempt to make it easier to modify and extend.

 Most significantly, I intend to separate the code into one or more common library projects, to help avoid code
 duplication and aid in organization.

 Sanoid.net, Syncoid.net, and Findoid.net will, themselves, remain individual applications that can be invoked
 with identical commands and arguments as their PERL ancestors.

 **Configuration** for the applications will be a hiearchy of json files, with formal schemas included, 
 that mirror and, where the motivation strikes, extend the existing configuration capabilities of PERL 
 sanoid/syncoid.\
 A command line argument will be provided that enables Sanoid.net to parse PERL sanoid's configuration files
 and convert them to Sanoid.net's json configuration format.

 My intention is to keep the project/solution easily useable from both Visual Studio 2022+ on Windows as well
 as vscode on Linux (I will be using both environments to develop it). As I use ReSharper on Visual Studio in
 Windows, there may end up being some ReSharper-related files and directives in code. However, since ZFS and,
 therefore, sanoid currenly are a Linux-targeted toolset, the project will always be guaranteed to compile and
 run under Linux (at minimum, Ubuntu 22.04 and later and RHEL/CentOS 8.6 and later, as that's what I have) with
 the project's required version of the dotnet runtime installed.

 ## Dependencies

 My intention is for this project to have no external dependencies other than the required version of the
 dotnet runtime that have to be manually installed by the end user. This includes not being reliant on the
 PERL versions of the applications, either, or any of their core dependencies. All compile-time dependencies of
 these ports will either be included in published releases or included as project references in the dotnet 
 projects, so that they can be automatically restored by the dotnet runtime, from the public NuGet repository.\
 Runtime dependencies, such as mbuffer, ssh, and others that are invoked by sanoid are, of course, still
 required to be installed and available, if they are used by your configuration.
 
 That said, here are the nuget package dependencies as of right now (automatically retrieved during build):
 
  - PowerArgs
  - JsonSchema.Net
  - Microsoft.Extensions.Configuration.Json
  - Microsoft.Extensions.Configuration.Ini
  - Microsoft.Extensions.Configuration.EnvironmentVariables
  - Microsoft.Extensions.Configuration.CommandLine
  - NLog
  - NLog.Extensions.Logging
  - NLog.Targets.Journald

  Additionally, `make` is ideal to be installed, as I've provided a Makefile with several useful build
  targets, to make things easier. Otherwise, you can manually run the commands in the Makefile to build. All
  build targets in the Makefile are bash-compatible scripts that assume standard coreutils are installed.

 ## Installing
 
 After cloning this repository, execute the following commands to build and install Sanoid.net using make:

     cd sanoid/dotnet
     make install-release

 This will fetch all .net dependencies from NuGet, build Sanoid.net in the ./publish/Release-R2R/ folder as a
 combined "Ready-to-Run" (partially natively pre-compiled) executable file, install Sanoid.net to
 `/usr/local/bin/Sanoid`, install all base configuration files to `/usr/local/share/Sanoid.net/`, and install
 a local configuration file at `/etc/sanoid/Sanoid.local.json`, making backups of any replaced files
 along the way.

 ## Uninstalling

 To uninstall, run `make uninstall`\
 This will delete the executable file from `/usr/local/bin`, delete the base configuration files from 
 `/usr/local/share/Sanoid.net`, and delete the local configuration file at `/etc/sanoid/Sanoid.local.json`.\
 This will not remove any local configuration files in `~/.config/Sanoid.net`.\
 This will also not remove the last build artifacts created by `make install`.

 To clean all build artifacts, run `make clean`\
 To clean specific build target target artifacts, run `make clean-release` or `make clean-debug`

 ## Running

 After runing `make install-release`, Sanoid.net can be run from any shell, so long as your `$PATH` includes
 the  `/usr/local/bin` directory and the system has the same version of .net installed as Sanoid.net was
 built on.


 ## But seriously... WHY???

 I'm not doing this to solve any "problem," fix any "deficiencies" in sanoid, or with the explicit purpose of
 improving it in any way. As I said, it is just a learning project for me, and I'm putting it in a public github
 repository in the hope that someone, somewhere, may either have use for it or learn something from it, as I hope
 to learn from it.

 ## Contributing

 Even though I'm doing this for my own learning, I'm more than happy to have anyone contribute to the project.

 If pull requests are submitted, when I have time, I will happily review and merge those that don't deviate from
 the core mission of the project in a significant way, and that don't completely duplicate something I'm already
 currently working on (that case should be uncommon, though, as I generally commit and push often).

 If you intend to contribute, please adhere to the code style of the project, which generally follows standard
 recommendations for C#, with small deviations if and when I think they improve the readability of the code.
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
