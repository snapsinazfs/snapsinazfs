# These variables generally should not be changed
MAKEFLAGS := $(filter-out -j,$(MAKEFLAGS))
MAKEFLAGS := $(filter-out -jobs,$(MAKEFLAGS))
SNAPSINAZFS_SOLUTION_ROOT ?= .
SIAZ ?= SnapsInAZfs
SIAZLC ?= snapsinazfs
SIAZ_INTEROP ?= $(SIAZ).Interop
SIAZ_SETTINGS ?= $(SIAZ).Settings
SIAZ_PROJECT_DIRECTORY ?= $(SNAPSINAZFS_SOLUTION_ROOT)/$(SIAZ)
SIAZ_PROJECT_FILE_NAME ?= $(SIAZ).csproj
SIAZ_PROJECT_FILE_PATH ?= $(SIAZ_PROJECT_DIRECTORY)/$(SIAZ_PROJECT_FILE_NAME)
SIAZ_INTEROP_PROJECT_DIRECTORY ?= $(SNAPSINAZFS_SOLUTION_ROOT)/$(SIAZ_INTEROP)
SIAZ_INTEROP_PROJECT_FILE_NAME ?= $(SIAZ_INTEROP).csproj
SIAZ_INTEROP_PROJECT_FILE_PATH ?= $(SIAZ_INTEROP_PROJECT_DIRECTORY)/$(SIAZ_INTEROP_PROJECT_FILE_NAME)
SIAZ_SETTINGS_PROJECT_DIRECTORY ?= $(SNAPSINAZFS_SOLUTION_ROOT)/$(SIAZ_SETTINGS)
SIAZ_SETTINGS_PROJECT_FILE_NAME ?= $(SIAZ_SETTINGS).csproj
SIAZ_SETTINGS_PROJECT_FILE_PATH ?= $(SIAZ_SETTINGS_PROJECT_DIRECTORY)/$(SIAZ_SETTINGS_PROJECT_FILE_NAME)
SNAPSINAZFSDOCDIR ?= $(SNAPSINAZFS_SOLUTION_ROOT)/Documentation
ETCDIR ?= /etc
SIAZLOCALCONFIGFILENAME ?= SnapsInAZfs.local.json
SNAPSINAZFSETCDIR ?= $(ETCDIR)/SnapsInAZfs
VERSIONSUFFIXFILE ?= $(SNAPSINAZFS_SOLUTION_ROOT)/VersionSuffix
VERSIONSUFFIX := $(shell cat ${VERSIONSUFFIXFILE})
# Variables above this line generally should not be changed

# For help on common and recommended build procedures,
# run `make help`

# Most variables blow this line can be changed, if you understand the consequences of changing them
# Do not change them in this file.
# Instead, if you wish to override a variable, set it as an environment variable when calling make.
# Variables are assigned using the ?= operator, which only sets them if they are not already defined,
# so variables you set before calling make will have the values you chose.
# For example, the following command would override the build configuration to be "Release" instead of the default "Release-R2R" and set the base destination for
# man pages to /usr/local/man when running `make install`:
# RELEASECONFIG=Release MANDIR=/usr/local/man make install

# These variables are the directories where build and publish artifacts will be placed
# They are used by several recipes, so be sure you understand the effects of changing them
BUILDDIR ?= $(SNAPSINAZFS_SOLUTION_ROOT)/build
PUBLISHROOT ?= $(SNAPSINAZFS_SOLUTION_ROOT)/publish

# These variables are used for *-release recipes
# If you want to use a different dotnet build configuration for release builds, change $RELEASECONFIG to a valid configuration defined in the solution
# If you want to use a different dotnet publish configuration (used for make install), change $RELEASEPUBLISHPROFILE to a valid defined publish profile as well
RELEASECONFIG ?= Release-R2R
RELEASEDIR ?= $(BUILDDIR)/$(RELEASECONFIG)
RELEASEPUBLISHPROFILE ?= Linux-Release-R2R
RELEASEPUBLISHDIR ?= $(PUBLISHROOT)/$(RELEASECONFIG)

# These variables are used for *-debug recipes
DEBUGCONFIG ?= Debug
DEBUGDIR ?= $(BUILDDIR)/$(DEBUGCONFIG)
DEBUGPUBLISHDIR ?= $(PUBLISHROOT)/$(DEBUGCONFIG)

# This variable is used for the test recipes and is the build configuration that will be used for the unit tests and is also the configuration of the associated projects that will be tested
# By default, it is the same as $RELEASECONFIG, so make test tests the code you just compiled.
TESTCONFIG ?= $(RELEASECONFIG)

# These variables should generally not be changed and may result in broken or incomplete installs or uninstalls
PUBLISHBASECONFIGFILELIST = $(RELEASEPUBLISHDIR)/SnapsInAZfs.json $(RELEASEPUBLISHDIR)/SnapsInAZfs.nlog.json $(RELEASEPUBLISHDIR)/SnapsInAZfs.schema.json
PUBLISHBASECONFIGFILELIST += $(RELEASEPUBLISHDIR)/SnapsInAZfs.monitoring.schema.json $(RELEASEPUBLISHDIR)/SnapsInAZfs.local.schema.json

# These variables are for the man pages installed by the install-doc recipe (called implicitly by install).
# If your system uses different directories than these for storing man page sections, set them as appropriate
MANDIR ?= /usr/share/man
MAN3DIR ?= $(MANDIR)/man3
MAN5DIR ?= $(MANDIR)/man5
MAN7DIR ?= $(MANDIR)/man7
MAN8DIR ?= $(MANDIR)/man8

# This is where the executable will be installed, when you run the install recipe
LOCALSBINDIR ?= /usr/local/sbin

# This is the base directory where a sub-directory containing base configuration and schema files will be installed, when you run the install recipe
LOCALSHAREDIR ?= /usr/local/share

# These variables are used for creating the default log file destination folder.
# Be sure to set the same path for log file targets in your local (in /etc/SnapsInAZfs) SnapsInAZfs.nlog.json
LOGROOT ?= /var/log
LOGPATH ?= $(LOGROOT)/$(SIAZ)

all:	build-release

clean:  clean-all

clean-all:      clean-debug	clean-release

clean-debug:
	dotnet clean --configuration $(DEBUGCONFIG) -o $(DEBUGDIR) 2>/dev/null
	[ -d $(DEBUGDIR) ] && rm -rvf $(DEBUGDIR) || true
	rmdir -v $(BUILDDIR) || true
	rm -rfv SnapsInAZfs/bin/$(DEBUGCONFIG) 2>/dev/null
	rm -rfv SnapsInAZfs/obj/$(DEBUGCONFIG) 2>/dev/null
	rmdir -v SnapsInAZfs/bin || true
	rmdir -v SnapsInAZfs/obj || true
	rm -rfv SnapsInAZfs.Interop/bin/$(DEBUGCONFIG) 2>/dev/null
	rm -rfv SnapsInAZfs.Interop/obj/$(DEBUGCONFIG) 2>/dev/null
	rmdir -v SnapsInAZfs.Interop/bin || true
	rmdir -v SnapsInAZfs.Interop/obj || true
	rm -rfv SnapsInAZfs.Settings/bin/$(DEBUGCONFIG) 2>/dev/null
	rm -rfv SnapsInAZfs.Settings/obj/$(DEBUGCONFIG) 2>/dev/null
	rmdir -v SnapsInAZfs.Settings/bin || true
	rmdir -v SnapsInAZfs.Settings/obj || true

clean-release:
	dotnet clean --configuration $(RELEASECONFIG) -o $(RELEASEDIR) 2>/dev/null
	if [ -d $(RELEASEDIR) ] ; then rm -rvf $(RELEASEDIR) ; fi
	[ -d $(RELEASEPUBLISHDIR) ]  && rm -rvf $(RELEASEPUBLISHDIR) || true
	[ -d $(PUBLISHROOT) ]  && rm -rvf $(PUBLISHROOT) || true
	rmdir -v $(PUBLISHROOT) || true
	rmdir -v $(BUILDDIR) || true
	rm -rfv SnapsInAZfs/bin/$(RELEASECONFIG) 2>/dev/null
	rm -rfv SnapsInAZfs/obj/$(RELEASECONFIG) 2>/dev/null
	rmdir -v SnapsInAZfs/bin || true
	rmdir -v SnapsInAZfs/obj || true
	rm -rfv SnapsInAZfs.Interop/bin/$(RELEASECONFIG) 2>/dev/null
	rm -rfv SnapsInAZfs.Interop/obj/$(RELEASECONFIG) 2>/dev/null
	rmdir -v SnapsInAZfs.Interop/bin || true
	rmdir -v SnapsInAZfs.Interop/obj || true
	rm -rfv SnapsInAZfs.Settings/bin/$(RELEASECONFIG) 2>/dev/null
	rm -rfv SnapsInAZfs.Settings/obj/$(RELEASECONFIG) 2>/dev/null
	rmdir -v SnapsInAZfs.Settings/bin || true
	rmdir -v SnapsInAZfs.Settings/obj || true

extraclean:	clean-debug	clean-release
	rm -rfv SnapsInAZfs/bin 2>/dev/null
	rm -rfv SnapsInAZfs/obj 2>/dev/null
	rm -rfv SnapsInAZfs.Interop/bin 2>/dev/null
	rm -rfv SnapsInAZfs.Interop/obj 2>/dev/null
	rm -rfv SnapsInAZfs.Settings/bin 2>/dev/null
	rm -rfv SnapsInAZfs.Settings/obj 2>/dev/null


build:	build-release

build-debug:
	mkdir -p $(DEBUGDIR)
	dotnet build --configuration $(DEBUGCONFIG) -o $(DEBUGDIR) -r linux-x64 -p:VersionSuffix=$(VERSIONSUFFIX) $(SIAZ_PROJECT_FILE_PATH)

build-release:
	mkdir -p $(RELEASEDIR)
	dotnet build --configuration $(RELEASECONFIG) -o $(RELEASEDIR) --use-current-runtime --no-self-contained -r linux-x64 -p:VersionSuffix=$(VERSIONSUFFIX) $(SIAZ_PROJECT_FILE_PATH)

reinstall:	uninstall	clean	install

install:	install-release	|	install-config	install-doc

install-config:	install-config-local	|	install-config-base

install-config-base:
	install --backup=existing -D -C -v -m 664 -t $(LOCALSHAREDIR)/SnapsInAZfs/ $(PUBLISHBASECONFIGFILELIST)

install-config-local:
	[ ! -d $(SNAPSINAZFSETCDIR) ] && [ -w $(ETCDIR) ] && mkdir -p $(SNAPSINAZFSETCDIR) || true
	@test -s $(SNAPSINAZFSETCDIR)/$(SIAZLOCALCONFIGFILENAME) || { install --backup=existing -C -v -m 664 $(RELEASEPUBLISHDIR)/$(SIAZLOCALCONFIGFILENAME) $(SNAPSINAZFSETCDIR)/$(SIAZLOCALCONFIGFILENAME) ; }
	@test -s $(SNAPSINAZFSETCDIR)/SnapsInAZfs.nlog.json || { install --backup=existing -C -v -m 664 $(RELEASEPUBLISHDIR)/SnapsInAZfs.nlog.json $(SNAPSINAZFSETCDIR)/SnapsInAZfs.nlog.json ; }

install-config-local-force:
	[ ! -d $(SNAPSINAZFSETCDIR) ] && [ -w $(ETCDIR) ] && mkdir -p $(SNAPSINAZFSETCDIR) || true
	install --backup=existing -C -v -m 664 $(RELEASEPUBLISHDIR)/$(SIAZLOCALCONFIGFILENAME) $(SNAPSINAZFSETCDIR)/$(SIAZLOCALCONFIGFILENAME)
	install --backup=existing -C -v -m 664 $(RELEASEPUBLISHDIR)/SnapsInAZfs.nlog.json $(SNAPSINAZFSETCDIR)/SnapsInAZfs.nlog.json

install-doc:
	install -C -v -m 644 $(SNAPSINAZFSDOCDIR)/SnapsInAZfs.8 $(MAN8DIR)/$(SIAZ).8
	cp -fl  $(MAN8DIR)/$(SIAZ).8 $(MAN8DIR)/$(SIAZLC).8
	cp -fl  $(MAN8DIR)/$(SIAZ).8 $(MAN8DIR)/siaz.8
	install -C -v -m 644 $(SNAPSINAZFSDOCDIR)/SnapsInAZfs-config-console.8 $(MAN8DIR)/$(SIAZ)-config-console.8
	cp -fl  $(MAN8DIR)/$(SIAZ)-config-console.8 $(MAN8DIR)/$(SIAZLC)-config-console.8
	cp -fl  $(MAN8DIR)/$(SIAZ)-config-console.8 $(MAN8DIR)/siaz-config-console.8
	install -C -v -m 644 $(SNAPSINAZFSDOCDIR)/$(SIAZ)-zfsprops.7 $(MAN7DIR)/$(SIAZ)-zfsprops.7
	cp -fl  $(MAN7DIR)/$(SIAZ)-zfsprops.7 $(MAN7DIR)/$(SIAZLC)-zfsprops.7
	cp -fl  $(MAN7DIR)/$(SIAZ)-zfsprops.7 $(MAN7DIR)/siaz-zfsprops.7
	install -C -v -m 644 $(SNAPSINAZFSDOCDIR)/$(SIAZ)-monitoring.3 $(MAN3DIR)/$(SIAZ)-monitoring.3
	cp -fl  $(MAN3DIR)/$(SIAZ)-monitoring.3 $(MAN3DIR)/$(SIAZLC)-monitoring.3
	cp -fl  $(MAN3DIR)/$(SIAZ)-monitoring.3 $(MAN3DIR)/siaz-monitoring.3
	install -C -v -m 644 $(SNAPSINAZFSDOCDIR)/$(SIAZ).5 $(MAN5DIR)/$(SIAZ).5
	cp -fl  $(MAN5DIR)/$(SIAZ).5 $(MAN5DIR)/$(SIAZLC).5
	cp -fl  $(MAN5DIR)/$(SIAZ).5 $(MAN5DIR)/siaz.5
	cp -fl  $(MAN5DIR)/$(SIAZ).5 $(MAN5DIR)/$(SIAZ).json.5
	cp -fl  $(MAN5DIR)/$(SIAZ).5 $(MAN5DIR)/$(SIAZLC).json.5
	mandb -q

install-release:	publish-release
	install --backup=existing -C -D -v -m 754 $(RELEASEPUBLISHDIR)/SnapsInAZfs $(LOCALSBINDIR)/$(SIAZ)
	cp -fs $(LOCALSBINDIR)/SnapsInAZfs $(LOCALSBINDIR)/$(SIAZLC)
	cp -fs $(LOCALSBINDIR)/SnapsInAZfs $(LOCALSBINDIR)/siaz
	mkdir -p $(LOGPATH)

install-service:
	install --backup=existing -C -v -m 664 $(SNAPSINAZFS_SOLUTION_ROOT)/snapsinazfs.service /usr/lib/systemd/system/snapsinazfs.service
	systemctl daemon-reload
	systemctl enable snapsinazfs.service

publish-release:
	mkdir -p $(RELEASEPUBLISHDIR)
	dotnet publish --configuration $(RELEASECONFIG) --use-current-runtime --no-self-contained -r linux-x64 -p:PublishProfile=$(RELEASEPUBLISHPROFILE) -p:VersionSuffix=$(VERSIONSUFFIX) -o $(RELEASEPUBLISHDIR) $(SIAZ_PROJECT_FILE_PATH)

uninstall:	uninstall-release	uninstall-config-base	uninstall-doc

uninstall-config-base:
	rm -fv $(LOCALSHAREDIR)/SnapsInAZfs/*.json 2>/dev/null

uninstall-config-local:
	rm -fv $(SNAPSINAZFSETCDIR)/$(SIAZLOCALCONFIGFILENAME)* 2>/dev/null
	rm -fv $(SNAPSINAZFSETCDIR)/SnapsInAZfs.nlog.json* 2>/dev/null
	rmdir -v $(SNAPSINAZFSETCDIR) 2>/dev/null

uninstall-doc:
	rm -fv $(MAN8DIR)/$(SIAZ).8 2>/dev/null
	rm -fv $(MAN8DIR)/$(SIAZLC).8 2>/dev/null
	rm -fv $(MAN8DIR)/siaz.8 2>/dev/null
	rm -fv $(MAN8DIR)/$(SIAZ)-config-console.8 2>/dev/null
	rm -fv $(MAN8DIR)/$(SIAZLC)-config-console.8 2>/dev/null
	rm -fv $(MAN8DIR)/siaz-config-console.8 2>/dev/null
	rm -fv $(MAN7DIR)/$(SIAZ)-zfsprops.7 2>/dev/null
	rm -fv $(MAN7DIR)/$(SIAZLC)-zfsprops.7 2>/dev/null
	rm -fv $(MAN7DIR)/siaz-zfsprops.7 2>/dev/null
	rm -fv $(MAN3DIR)/$(SIAZ)-monitoring.3 2>/dev/null
	rm -fv $(MAN3DIR)/$(SIAZLC)-monitoring.3 2>/dev/null
	rm -fv $(MAN3DIR)/siaz-monitoring.3 2>/dev/null
	rm -fv $(MAN5DIR)/$(SIAZ).5 2>/dev/null
	rm -fv $(MAN5DIR)/$(SIAZLC).5 2>/dev/null
	rm -fv $(MAN5DIR)/siaz.5 2>/dev/null
	rm -fv $(MAN5DIR)/$(SIAZ).json.5 2>/dev/null
	rm -fv $(MAN5DIR)/$(SIAZLC).json.5 2>/dev/null
	mandb -q

uninstall-everything:	uninstall-service	uninstall	uninstall-config-local	uninstall-logs

uninstall-logs:
	rm -rfv $(LOGPATH) 2>/dev/null

uninstall-release:
	rm -rfv $(LOCALSHAREDIR)/SnapsInAZfs 2>/dev/null
	rm -fv $(LOCALSBINDIR)/$(SIAZ) 2>/dev/null
	rm -fv $(LOCALSBINDIR)/$(SIAZLC) 2>/dev/null
	rm -fv $(LOCALSBINDIR)/siaz 2>/dev/null

uninstall-service:
	systemctl stop snapsinazfs.service
	systemctl disable snapsinazfs.service
	rm -rf /usr/lib/systemd/system/snapsinazfs.service
	systemctl daemon-reload

test:
	dotnet test --configuration=$(TESTCONFIG) --verbosity=quiet --nologo --filter TestCategory\!=Exhaustive

test-everything:
	dotnet test --configuration=$(TESTCONFIG) --verbosity=quiet --nologo

test-everything-verbose:
	dotnet test --configuration=$(TESTCONFIG) --verbosity=normal --nologo

save-snapsinazfs-zfs-properties:
	@savelog -plnc 20 propWipeUndoScript.sh
	@echo "#!/bin/bash -x" >propWipeUndoScript.sh
	zfs get all -s local -rHo name,property,value | grep "snapsinazfs.com:" | while read obj prop val ; do echo zfs set $${prop}\=\"$${val}\" $${obj} >>propWipeUndoScript.sh ; done
	chmod 774 propWipeUndoScript.sh
	@echo Undo script saved as ./propWipeUndoScript.sh
	@echo Run 'make restore-wiped-zfs-properties' or './propWipeUndoScript.sh' if you need to restore snapsinazfs.com properties

wipe-snapsinazfs-zfs-properties:	save-snapsinazfs-zfs-properties
	zfs get all -s local -rHo name,property | grep "snapsinazfs.com:" | while read obj prop ; do echo Removing $${prop} from $${obj} ; zfs inherit $${prop} $${obj} ; done
	$(info All properties removed)
	$(info Run make restore-wiped-zfs-properties to restore configuration)

restore-wiped-zfs-properties:
	@test -s $(SNAPSINAZFS_SOLUTION_ROOT)/propWipeUndoScript.sh || { echo No restore script. Did you forget to run make save-snapsinazfs-zfs-properties\? ; false ; }
	./propWipeUndoScript.sh
	$(info Properties restored to the state they were in when you last ran make save-snapsinazfs-zfs-properties)
