# These variables generally should not be changed
MAKEFLAGS := $(filter-out -j,$(MAKEFLAGS))
MAKEFLAGS := $(filter-out -jobs,$(MAKEFLAGS))
SHELL := $(shell which bash)
SNAPSINAZFS_SOLUTION_ROOT ?= .
SIAZ_SOLUTION_FILE ?= SnapsInAZfs.slnx
SIAZ_APPLICATIONS_DIRECTORY ?= $(SNAPSINAZFS_SOLUTION_ROOT)/Applications
SIAZ_LIBRARIES_DIRECTORY ?= $(SNAPSINAZFS_SOLUTION_ROOT)/Libraries
SIAZ ?= SnapsInAZfs
SIAZLC ?= snapsinazfs
SIAZ_INTEROP ?= $(SIAZ).Interop
SIAZ_SETTINGS ?= $(SIAZ).Settings
SIAZ_PROJECT_DIRECTORY ?= $(SIAZ_APPLICATIONS_DIRECTORY)/$(SIAZ)
SIAZ_PROJECT_FILE_NAME ?= $(SIAZ).csproj
SIAZ_PROJECT_FILE_PATH ?= $(SIAZ_PROJECT_DIRECTORY)/$(SIAZ_PROJECT_FILE_NAME)
SIAZ_INTEROP_PROJECT_DIRECTORY ?= $(SIAZ_LIBRARIES_DIRECTORY)/$(SIAZ_INTEROP)
SIAZ_INTEROP_PROJECT_FILE_NAME ?= $(SIAZ_INTEROP).csproj
SIAZ_INTEROP_PROJECT_FILE_PATH ?= $(SIAZ_INTEROP_PROJECT_DIRECTORY)/$(SIAZ_INTEROP_PROJECT_FILE_NAME)
SIAZ_SETTINGS_PROJECT_DIRECTORY ?= $(SIAZ_LIBRARIES_DIRECTORY)/$(SIAZ_SETTINGS)
SIAZ_SETTINGS_PROJECT_FILE_NAME ?= $(SIAZ_SETTINGS).csproj
SIAZ_SETTINGS_PROJECT_FILE_PATH ?= $(SIAZ_SETTINGS_PROJECT_DIRECTORY)/$(SIAZ_SETTINGS_PROJECT_FILE_NAME)
SNAPSINAZFSDOCDIR ?= $(SNAPSINAZFS_SOLUTION_ROOT)/Documentation
ETCDIR ?= /etc
SIAZ_LOCAL_CONFIG_SUFFIX ?= local
SIAZ_LOCAL_CONFIG_FILE_NAME ?= $(SIAZ).$(SIAZ_LOCAL_CONFIG_SUFFIX).json
SIAZ_NLOG_CONFIG_FILE_NAME ?= $(SIAZ).nlog.json
SNAPSINAZFSETCDIR ?= $(ETCDIR)/$(SIAZ)
# Variables above this line generally should not be changed

# For help on common and recommended build procedures,
# run `make help`

# Most variables blow this line can be changed, if you understand the consequences of changing them.
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
PUBLISHBASECONFIGFILELIST = $(RELEASEPUBLISHDIR)/$(SIAZ).json $(RELEASEPUBLISHDIR)/$(SIAZ_NLOG_CONFIG_FILE_NAME) $(RELEASEPUBLISHDIR)/$(SIAZ).schema.json
PUBLISHBASECONFIGFILELIST += $(RELEASEPUBLISHDIR)/$(SIAZ).monitoring.schema.json $(RELEASEPUBLISHDIR)/$(SIAZ).$(SIAZ_LOCAL_CONFIG_SUFFIX).schema.json

# These variables are for the man pages installed by the install-doc recipe (called implicitly by install).
# If your system uses different directories than these for storing man page sections, set them as appropriate.
# However, this should auto-detect a valid man path on most systems, as it takes the first path that man itself reports that it uses.
MANDIR ?= `man -w | cut -d : -f 1`
MAN3DIR ?= $(MANDIR)/man3
MAN5DIR ?= $(MANDIR)/man5
MAN7DIR ?= $(MANDIR)/man7
MAN8DIR ?= $(MANDIR)/man8

# This is where the executable will be installed, when you run the install recipe
USR_LOCAL_SBIN_DIR ?= /usr/local/sbin

# This is the base directory where a sub-directory containing base configuration and schema files will be installed, when you run the install recipe
USR_LOCAL_ETC_DIR ?= /usr/local/etc

# These variables are used for creating the default log file destination folder.
# Be sure to set the same path for log file targets in your local (in /etc/SnapsInAZfs) SnapsInAZfs.nlog.json
LOGROOT ?= /var/log
LOGPATH ?= $(LOGROOT)/$(SIAZ)

.ONESHELL:

all:	build-release

clean:	clean-all

clean-all:	clean-debug	clean-release

clean-debug:
  @echo Cleaning $(DEBUGCONFIG) build artifacts
	dotnet clean $(SIAZ_SOLUTION_FILE) --configuration $(DEBUGCONFIG) -o $(DEBUGDIR) 2>/dev/null
	[ -d $(DEBUGDIR) ] && rm -rvf $(DEBUGDIR) || true
	rmdir -v $(BUILDDIR) || true
	rm -rfv $(SIAZ_PROJECT_DIRECTORY)/bin/$(DEBUGCONFIG) 2>/dev/null
	rm -rfv $(SIAZ_PROJECT_DIRECTORY)/obj/$(DEBUGCONFIG) 2>/dev/null
	rmdir -v $(SIAZ_PROJECT_DIRECTORY)/bin || true
	rmdir -v $(SIAZ_PROJECT_DIRECTORY)/obj || true
	rm -rfv $(SIAZ_INTEROP_PROJECT_DIRECTORY)/bin/$(DEBUGCONFIG) 2>/dev/null
	rm -rfv $(SIAZ_INTEROP_PROJECT_DIRECTORY)/obj/$(DEBUGCONFIG) 2>/dev/null
	rmdir -v $(SIAZ_INTEROP_PROJECT_DIRECTORY)/bin || true
	rmdir -v $(SIAZ_INTpEROP_PROJECT_DIRECTORY)/obj || true
	rm -rfv $(SIAZ_SETTINGS_PROJECT_DIRECTORY)/bin/$(DEBUGCONFIG) 2>/dev/null
	rm -rfv $(SIAZ_SETTINGS_PROJECT_DIRECTORY)/obj/$(DEBUGCONFIG) 2>/dev/null
	rmdir -v $(SIAZ_SETTINGS_PROJECT_DIRECTORY)/bin || true
	rmdir -v $(SIAZ_SETTINGS_PROJECT_DIRECTORY)/obj || true

clean-release:
  @echo Cleaning $(RELEASECONFIG) build artifacts
	dotnet clean $(SIAZ_SOLUTION_FILE) --configuration $(RELEASECONFIG) -o $(RELEASEDIR) 2>/dev/null
	if [ -d $(RELEASEDIR) ] ; then rm -rvf $(RELEASEDIR) ; fi
	[ -d $(RELEASEPUBLISHDIR) ]  && rm -rvf $(RELEASEPUBLISHDIR) || true
	[ -d $(PUBLISHROOT) ]  && rm -rvf $(PUBLISHROOT) || true
	rmdir -v $(PUBLISHROOT) || true
	rmdir -v $(BUILDDIR) || true
	rm -rfv $(SIAZ_PROJECT_DIRECTORY)/bin/$(RELEASECONFIG) 2>/dev/null
	rm -rfv $(SIAZ_PROJECT_DIRECTORY)/obj/$(RELEASECONFIG) 2>/dev/null
	rmdir -v $(SIAZ_PROJECT_DIRECTORY)/bin || true
	rmdir -v $(SIAZ_PROJECT_DIRECTORY)/obj || true
	rm -rfv $(SIAZ_INTEROP_PROJECT_DIRECTORY)/bin/$(RELEASECONFIG) 2>/dev/null
	rm -rfv $(SIAZ_INTEROP_PROJECT_DIRECTORY)/obj/$(RELEASECONFIG) 2>/dev/null
	rmdir -v $(SIAZ_INTEROP_PROJECT_DIRECTORY)/bin || true
	rmdir -v $(SIAZ_INTEROP_PROJECT_DIRECTORY)/obj || true
	rm -rfv $(SIAZ_SETTINGS_PROJECT_DIRECTORY)/bin/$(RELEASECONFIG) 2>/dev/null
	rm -rfv $(SIAZ_SETTINGS_PROJECT_DIRECTORY)/obj/$(RELEASECONFIG) 2>/dev/null
	rmdir -v $(SIAZ_SETTINGS_PROJECT_DIRECTORY)/bin || true
	rmdir -v $(SIAZ_SETTINGS_PROJECT_DIRECTORY)/obj || true

extraclean:	clean-debug	clean-release
	rm -rfv $(SIAZ_PROJECT_DIRECTORY)/bin 2>/dev/null
	rm -rfv $(SIAZ_PROJECT_DIRECTORY)/obj 2>/dev/null
	rm -rfv $(SIAZ_INTEROP_PROJECT_DIRECTORY)/bin 2>/dev/null
	rm -rfv $(SIAZ_INTEROP_PROJECT_DIRECTORY)/obj 2>/dev/null
	rm -rfv $(SIAZ_SETTINGS_PROJECT_DIRECTORY)/bin 2>/dev/null
	rm -rfv $(SIAZ_SETTINGS_PROJECT_DIRECTORY)/obj 2>/dev/null

build:	build-release

build-debug:
	mkdir -p $(DEBUGDIR)
	dotnet build $(SIAZ_SOLUTION_FILE) --configuration $(DEBUGCONFIG) -o $(DEBUGDIR) -r linux-x64 $(SIAZ_PROJECT_FILE_PATH)

build-release:
	mkdir -p $(RELEASEDIR)
	dotnet build $(SIAZ_SOLUTION_FILE) --configuration $(RELEASECONFIG) -o $(RELEASEDIR) --use-current-runtime --no-self-contained -r linux-x64 $(SIAZ_PROJECT_FILE_PATH)

reinstall:	uninstall	clean	install

install:	install-release	|	install-config	install-doc

install-config:	install-config-local	|	install-config-base

install-config-base:
	install --backup=existing -D -C -v -m 664 -t $(USR_LOCAL_ETC_DIR)/$(SIAZ)/ $(PUBLISHBASECONFIGFILELIST)

install-config-local:
	[ ! -d $(SNAPSINAZFSETCDIR) ] && [ -w $(ETCDIR) ] && mkdir -p $(SNAPSINAZFSETCDIR) || true
	@test -s $(SNAPSINAZFSETCDIR)/$(SIAZ_LOCAL_CONFIG_FILE_NAME) || { install --backup=existing -C -v -m 664 $(RELEASEPUBLISHDIR)/$(SIAZ_LOCAL_CONFIG_FILE_NAME) $(SNAPSINAZFSETCDIR)/$(SIAZ_LOCAL_CONFIG_FILE_NAME) ; }
	@test -s $(SNAPSINAZFSETCDIR)/$(SIAZ_NLOG_CONFIG_FILE_NAME) || { install --backup=existing -C -v -m 664 $(RELEASEPUBLISHDIR)/$(SIAZ_NLOG_CONFIG_FILE_NAME) $(SNAPSINAZFSETCDIR)/$(SIAZ_NLOG_CONFIG_FILE_NAME) ; }

install-config-local-force:
	[ ! -d $(SNAPSINAZFSETCDIR) ] && [ -w $(ETCDIR) ] && mkdir -p $(SNAPSINAZFSETCDIR) || true
	install --backup=existing -C -v -m 664 $(RELEASEPUBLISHDIR)/$(SIAZ_LOCAL_CONFIG_FILE_NAME) $(SNAPSINAZFSETCDIR)/$(SIAZ_LOCAL_CONFIG_FILE_NAME)
	install --backup=existing -C -v -m 664 $(RELEASEPUBLISHDIR)/$(SIAZ_NLOG_CONFIG_FILE_NAME) $(SNAPSINAZFSETCDIR)/$(SIAZ_NLOG_CONFIG_FILE_NAME)

install-doc:
	install -C -v -m 644 $(SNAPSINAZFSDOCDIR)/$(SIAZ).8 $(MAN8DIR)/$(SIAZ).8
	cp -fl  $(MAN8DIR)/$(SIAZ).8 $(MAN8DIR)/$(SIAZLC).8
	cp -fl  $(MAN8DIR)/$(SIAZ).8 $(MAN8DIR)/siaz.8
	install -C -v -m 644 $(SNAPSINAZFSDOCDIR)/$(SIAZ)-config-console.8 $(MAN8DIR)/$(SIAZ)-config-console.8
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
	install --backup=existing -C -D -v -m 754 $(RELEASEPUBLISHDIR)/$(SIAZ) $(USR_LOCAL_SBIN_DIR)/$(SIAZ)
	cp -fs $(USR_LOCAL_SBIN_DIR)/$(SIAZ) $(USR_LOCAL_SBIN_DIR)/$(SIAZLC)
	cp -fs $(USR_LOCAL_SBIN_DIR)/$(SIAZ) $(USR_LOCAL_SBIN_DIR)/siaz
	mkdir -p $(LOGPATH)

install-service:
	install --backup=existing -C -v -m 664 $(SNAPSINAZFS_SOLUTION_ROOT)/$(SIAZLC).service /usr/lib/systemd/system/$(SIAZLC).service
	systemctl daemon-reload
	systemctl enable $(SIAZLC).service

publish-release:
	mkdir -p $(RELEASEPUBLISHDIR)
	dotnet publish --configuration $(RELEASECONFIG) --use-current-runtime --no-self-contained -r linux-x64 -p:PublishProfile=$(RELEASEPUBLISHPROFILE) -o $(RELEASEPUBLISHDIR) $(SIAZ_PROJECT_FILE_PATH)

uninstall:	uninstall-release	uninstall-config-base	uninstall-doc

uninstall-config-base:
	rm -fv $(USR_LOCAL_ETC_DIR)/$(SIAZ)/*.json 2>/dev/null

uninstall-config-local:
	rm -fv $(SNAPSINAZFSETCDIR)/$(SIAZ_LOCAL_CONFIG_FILE_NAME)* 2>/dev/null
	rm -fv $(SNAPSINAZFSETCDIR)/$(SIAZ).nlog.json* 2>/dev/null
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
	rm -rfv $(USR_LOCAL_ETC_DIR)/$(SIAZ) 2>/dev/null
	rm -fv $(USR_LOCAL_SBIN_DIR)/$(SIAZ) 2>/dev/null
	rm -fv $(USR_LOCAL_SBIN_DIR)/$(SIAZLC) 2>/dev/null
	rm -fv $(USR_LOCAL_SBIN_DIR)/siaz 2>/dev/null

uninstall-service:
	systemctl stop $(SIAZLC).service
	systemctl disable $(SIAZLC).service
	rm -rf /usr/lib/systemd/system/$(SIAZLC).service
	systemctl daemon-reload

test:
	dotnet test $(SIAZ_SOLUTION_FILE) --configuration=$(TESTCONFIG) --verbosity=quiet --nologo --filter TestCategory\!=Exhaustive

test-everything:
	dotnet test $(SIAZ_SOLUTION_FILE) --configuration=$(TESTCONFIG) --verbosity=quiet --nologo

test-everything-verbose:
	dotnet test $(SIAZ_SOLUTION_FILE) --configuration=$(TESTCONFIG) --verbosity=normal --nologo

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
