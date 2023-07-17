SNAPSINAZFS_SOLUTION_ROOT ?= .

PROJECTFILE ?= $(SNAPSINAZFS_SOLUTION_ROOT)SnapsInAZfs/SnapsInAZfs.csproj

BUILDDIR ?= $(SNAPSINAZFS_SOLUTION_ROOT)/build
PUBLISHROOT ?= $(SNAPSINAZFS_SOLUTION_ROOT)/publish
RELEASECONFIG ?= Release-R2R
RELEASEDIR ?= $(BUILDDIR)/$(RELEASECONFIG)
RELEASEPUBLISHDIR ?= $(PUBLISHROOT)/$(RELEASECONFIG)
DEBUGCONFIG ?= Debug
DEBUGDIR ?= $(BUILDDIR)/$(DEBUGCONFIG)
DEBUGPUBLISHDIR ?= $(PUBLISHROOT)/$(DEBUGCONFIG)
SIAZ ?= SnapsInAZfs
SIAZLC ?= snapsinazfs

PUBLISHBASECONFIGFILELIST = $(RELEASEPUBLISHDIR)/SnapsInAZfs.json $(RELEASEPUBLISHDIR)/SnapsInAZfs.nlog.json $(RELEASEPUBLISHDIR)/SnapsInAZfs.schema.json
PUBLISHBASECONFIGFILELIST += $(RELEASEPUBLISHDIR)/SnapsInAZfs.monitoring.schema.json $(RELEASEPUBLISHDIR)/SnapsInAZfs.local.schema.json

SNAPSINAZFSDOCDIR ?= $(SNAPSINAZFS_SOLUTION_ROOT)/Documentation
MANDIR ?= /usr/local/man

LOCALSBINDIR ?= /usr/local/sbin
LOCALSHAREDIR ?= /usr/local/share
ETCDIR ?= /etc

SNAPSINAZFSETCDIR ?= $(ETCDIR)/SnapsInAZfs

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
	dotnet build --configuration $(DEBUGCONFIG) -o $(DEBUGDIR)

build-release:
	mkdir -p $(RELEASEDIR)
	dotnet build --configuration $(RELEASECONFIG) -o $(RELEASEDIR) --use-current-runtime --no-self-contained -r linux-x64 SnapsInAZfs/SnapsInAZfs.csproj

reinstall:	uninstall	clean	install

install:	install-release	|	install-config	install-doc

install-config:	install-config-local	|	install-config-base

install-config-base:
	install --backup=existing -D -C -v -m 664 -t $(LOCALSHAREDIR)/SnapsInAZfs/ $(PUBLISHBASECONFIGFILELIST)

install-config-local:
	[ ! -d $(SNAPSINAZFSETCDIR) ] && [ -w $(ETCDIR) ] && mkdir -p $(SNAPSINAZFSETCDIR) || true
	@test -s $(SNAPSINAZFSETCDIR)/SnapsInAZfs.local.json || { install --backup=existing -C -v -m 664 $(RELEASEPUBLISHDIR)/SnapsInAZfs.local.json $(SNAPSINAZFSETCDIR)/SnapsInAZfs.local.json ; }
	@test -s $(SNAPSINAZFSETCDIR)/SnapsInAZfs.nlog.json || { install --backup=existing -C -v -m 664 $(RELEASEPUBLISHDIR)/SnapsInAZfs.nlog.json $(SNAPSINAZFSETCDIR)/SnapsInAZfs.nlog.json ; }

install-config-local-force:
	[ ! -d $(SNAPSINAZFSETCDIR) ] && [ -w $(ETCDIR) ] && mkdir -p $(SNAPSINAZFSETCDIR) || true
	install --backup=existing -C -v -m 664 $(RELEASEPUBLISHDIR)/SnapsInAZfs.local.json $(SNAPSINAZFSETCDIR)/SnapsInAZfs.local.json
	install --backup=existing -C -v -m 664 $(RELEASEPUBLISHDIR)/SnapsInAZfs.nlog.json $(SNAPSINAZFSETCDIR)/SnapsInAZfs.nlog.json

install-doc:
	install -C -v -m 644 $(SNAPSINAZFSDOCDIR)/SnapsInAZfs.8 $(MANDIR)/man8/$(SIAZ).8
	cp -fl  $(MANDIR)/man8/$(SIAZ).8 $(MANDIR)/man8/$(SIAZLC).8
	cp -fl  $(MANDIR)/man8/$(SIAZ).8 $(MANDIR)/man8/siaz.8
	install -C -v -m 644 $(SNAPSINAZFSDOCDIR)/SnapsInAZfs.5 $(MANDIR)/man5/SnapsInAZfs.5
	cp -fl $(MANDIR)/man5/SnapsInAZfs.5 $(MANDIR)/man5/SnapsInAZfs.json.5
	mandb -q

install-release:	publish-release
	install --backup=existing -C -v -m 754 $(RELEASEPUBLISHDIR)/SnapsInAZfs $(LOCALSBINDIR)/SnapsInAZfs

install-service:
	install --backup=existing -C -v -m 664 $(SNAPSINAZFS_SOLUTION_ROOT)/snapsinazfs.service /usr/lib/systemd/system/snapsinazfs.service
	systemctl daemon-reload
	systemctl enable snapsinazfs.service

publish-release:
	mkdir -p $(RELEASEPUBLISHDIR)
	dotnet publish --configuration $(RELEASECONFIG) --use-current-runtime --no-self-contained -r linux-x64 -p:PublishProfile=Linux-Release-R2R -o $(RELEASEPUBLISHDIR) SnapsInAZfs/SnapsInAZfs.csproj

uninstall:	uninstall-release	uninstall-config-base	uninstall-doc

uninstall-config-base:
	rm -fv $(LOCALSHAREDIR)/SnapsInAZfs/*.json 2>/dev/null

uninstall-config-local:
	rm -fv $(SNAPSINAZFSETCDIR)/SnapsInAZfs.local.json* 2>/dev/null
	rm -fv $(SNAPSINAZFSETCDIR)/SnapsInAZfs.nlog.json* 2>/dev/null
	rmdir -v $(SNAPSINAZFSETCDIR) 2>/dev/null

uninstall-doc:
	rm -fv $(MANDIR)/man8/$(SIAZ).8 2>/dev/null
	rm -fv $(MANDIR)/man8/$(SIAZLC).8 2>/dev/null
	rm -fv $(MANDIR)/man8/siaz.8 2>/dev/null
	rm -fv $(MANDIR)/man5/SnapsInAZfs.5 2>/dev/null
	mandb -q

uninstall-everything:	uninstall-service	uninstall	uninstall-config-local

uninstall-release:
	rm -rfv $(LOCALSHAREDIR)/SnapsInAZfs 2>/dev/null
	rm -fv $(LOCALSBINDIR)/SnapsInAZfs 2>/dev/null

uninstall-service:
	systemctl stop snapsinazfs.service
	systemctl disable snapsinazfs.service
	rm -rf /usr/lib/systemd/system/snapsinazfs.service
	systemctl daemon-reload

test:	test-everything

test-everything:
	dotnet test --configuration=Release-R2R --verbosity=minimal --nologo

test-everything-verbose:
	dotnet test --configuration=Release-R2R --verbosity=normal --nologo

save-snapsinazfs-zfs-properties:
	@test ! -s $(SNAPSINAZFS_SOLUTION_ROOT)/propWipeUndoScript.sh || { echo Properties already saved. Will not overwrite. ; false ; }
	@echo "#!/bin/bash -x" >propWipeUndoScript.sh
	zfs get all -s local -rHo name,property,value | grep snapsinazfs.com | while read obj prop val ; do echo zfs set $${prop}\=\"$${val}\" $${obj} >>propWipeUndoScript.sh ; done
	chmod 774 propWipeUndoScript.sh
	@echo Undo script saved as ./propWipeUndoScript
	@echo Run 'make restore-wiped-zfs-properties' or './propWipeUndoScript.sh' if you need to restore snapsinazfs.com properties

wipe-snapsinazfs-zfs-properties:
	@test -s $(SNAPSINAZFS_SOLUTION_ROOT)/propWipeUndoScript.sh || { echo Must save properties before wiping ; false ; }
	zfs get all -s local -rHo name,property | grep snapsinazfs.com | while read obj prop ; do echo Removing $${prop} from $${obj} ; zfs inherit $${prop} $${obj} ; done
	$(info All properties removed)
	$(info Run make restore-wiped-zfs-properties to restore configuration)

restore-wiped-zfs-properties:
	@test -s $(SNAPSINAZFS_SOLUTION_ROOT)/propWipeUndoScript.sh || { echo No restore script. Did you forget to run make save-snapsinazfs-zfs-properties\? ; false ; }
	./propWipeUndoScript.sh
	$(info Properties restored to the state they were in when you last ran make save-snapsinazfs-zfs-properties)
