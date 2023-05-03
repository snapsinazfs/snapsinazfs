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

 ## Project Organization

 I intend to organize the project differently than sanoid/syncoid/findoid.

 Most significantly, I intend to separate the code into one or more common library projects, to help avoid code
 duplication and aid in organization.

 Sanoid.net, Syncoid.net, and Findoid.net will, themselves, remain individual applications that can be invoked with
 identical commands and arguments as their PERL parents.

 **Configuration** for the applications will respect the existing configuration files that sanoid/syncoid expect to
 find, by default. However, there will also be json-formatted configuration files for the .net ports. The default 
 configuration will instruct the .net ports to use the ini-styled configuration files that the PERL applications 
 expect to find in `/etc/sanoid/`, but will allow either pointing to a different path and continuing to use the 
 ini-style files or, as a future enhancement, completely override them in the json configuration. Some 
 configuration options not currently exposed by the PERL applications may be added in the json configuration to
 allow for enahnced flexibility, as I see fit while working on the project.

 My intention is to keep the project/solution easily useable from both Visual Studio 2022+ on Windows as well as
 vscode on Linux (I will be using both environments to develop it). As I use ReSharper on Visual Studio in Windows,
 there may end up being some ReSharper-related files and directives in code. However, since ZFS and, therefore,
 sanoid currenly are a Linux-targeted toolset, the project will always be guaranteed to compile and run under Linux
 (at minimum, Ubuntu 22.04 and later and RHEL/CentOS 8.6 and later, as that's what I have) with the project's
 required version of the dotnet runtime installed.

 ## Dependencies

 My intention is for this project to have no dependencies other than the required version of the dotnet runtime 
 that have to be manually installed by the end user. This includes not being reliant on the PERL versions of the
 applications, either, or any of their dependencies, except for their configuration files. All dependencies of
 these ports will either be included in published releases or included as project references in the dotnet 
 projects, so that they can be automatically restored by the dotnet runtime, from the public NuGet repository.

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