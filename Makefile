SANOID_DOTNET_SOLUTION_ROOT ?= .

PROJECTFILE ?= $(SANOID_DOTNET_SOLUTION_ROOT)Sanoid/Sanoid.csproj

BUILDDIR ?= $(SANOID_DOTNET_SOLUTION_ROOT)/build
PUBLISHROOT ?= $(SANOID_DOTNET_SOLUTION_ROOT)/publish
RELEASECONFIG ?= Release-R2R
RELEASEDIR ?= $(BUILDDIR)/$(RELEASECONFIG)
RELEASEPUBLISHDIR ?= $(PUBLISHROOT)/$(RELEASECONFIG)
DEBUGCONFIG ?= Debug
DEBUGDIR ?= $(BUILDDIR)/$(DEBUGCONFIG)
DEBUGPUBLISHDIR ?= $(PUBLISHROOT)/$(DEBUGCONFIG)

PUBLISHBASECONFIGFILELIST = $(RELEASEPUBLISHDIR)/Sanoid.json $(RELEASEPUBLISHDIR)/Sanoid.nlog.json $(RELEASEPUBLISHDIR)/Sanoid.schema.json
PUBLISHBASECONFIGFILELIST += $(RELEASEPUBLISHDIR)/Sanoid.monitoring.schema.json $(RELEASEPUBLISHDIR)/Sanoid.local.schema.json

SANOIDDOCDIR ?= $(SANOID_DOTNET_SOLUTION_ROOT)/Documentation
MANDIR ?= /usr/local/man

LOCALSBINDIR ?= /usr/local/sbin
LOCALSHAREDIR ?= /usr/local/share
ETCDIR ?= /etc

SANOIDETCDIR ?= $(ETCDIR)/sanoid

clean:  clean-all

clean-all:      clean-debug	clean-release

clean-debug:
	dotnet clean --configuration $(DEBUGCONFIG) -o $(DEBUGDIR)
	[ -d $(DEBUGDIR) ] && rm -rvf $(DEBUGDIR) || true
	rmdir -v $(BUILDDIR) || true
	rm -rfv Sanoid/bin/$(DEBUGCONFIG)
	rm -rfv Sanoid/obj/$(DEBUGCONFIG)
	rmdir -v Sanoid/bin || true
	rmdir -v Sanoid/obj || true
	rm -rfv Sanoid.Common/bin/$(DEBUGCONFIG)
	rm -rfv Sanoid.Common/obj/$(DEBUGCONFIG)
	rmdir -v Sanoid.Common/bin || true
	rmdir -v Sanoid.Common/obj || true
	rm -rfv Sanoid.Interop/bin/$(DEBUGCONFIG)
	rm -rfv Sanoid.Interop/obj/$(DEBUGCONFIG)
	rmdir -v Sanoid.Interop/bin || true
	rmdir -v Sanoid.Interop/obj || true
	rm -rfv Sanoid.Settings/bin/$(DEBUGCONFIG)
	rm -rfv Sanoid.Settings/obj/$(DEBUGCONFIG)
	rmdir -v Sanoid.Settings/bin || true
	rmdir -v Sanoid.Settings/obj || true

clean-release:
	dotnet clean --configuration $(RELEASECONFIG) -o $(RELEASEDIR)
	if [ -d $(RELEASEDIR) ] ; then rm -rvf $(RELEASEDIR) ; fi
	[ -d $(RELEASEPUBLISHDIR) ]  && rm -rvf $(RELEASEPUBLISHDIR) || true
	[ -d $(PUBLISHROOT) ]  && rm -rvf $(PUBLISHROOT) || true
	rmdir -v $(PUBLISHROOT) || true
	rmdir -v $(BUILDDIR) || true
	rm -rfv Sanoid/bin/$(RELEASECONFIG)
	rm -rfv Sanoid/obj/$(RELEASECONFIG)
	rmdir -v Sanoid/bin || true
	rmdir -v Sanoid/obj || true
	rm -rfv Sanoid.Common/bin/$(RELEASECONFIG)
	rm -rfv Sanoid.Common/obj/$(RELEASECONFIG)
	rmdir -v Sanoid.Common/bin || true
	rmdir -v Sanoid.Common/obj || true
	rm -rfv Sanoid.Interop/bin/$(RELEASECONFIG)
	rm -rfv Sanoid.Interop/obj/$(RELEASECONFIG)
	rmdir -v Sanoid.Interop/bin || true
	rmdir -v Sanoid.Interop/obj || true
	rm -rfv Sanoid.Settings/bin/$(RELEASECONFIG)
	rm -rfv Sanoid.Settings/obj/$(RELEASECONFIG)
	rmdir -v Sanoid.Settings/bin || true
	rmdir -v Sanoid.Settings/obj || true

extraclean:	clean-debug	clean-release
	rm -rfv Sanoid/bin
	rm -rfv Sanoid/obj
	rm -rfv Sanoid.Common/bin
	rm -rfv Sanoid.Common/obj
	rm -rfv Sanoid.Interop/bin
	rm -rfv Sanoid.Interop/obj
	rm -rfv Sanoid.Settings/bin
	rm -rfv Sanoid.Settings/obj


build:	build-release

build-debug:
	mkdir -p $(DEBUGDIR)
	dotnet build --configuration $(DEBUGCONFIG) -o $(DEBUGDIR)

build-release:
	mkdir -p $(RELEASEDIR)
	dotnet build --configuration $(RELEASECONFIG) -o $(RELEASEDIR) --use-current-runtime --no-self-contained -r linux-x64 Sanoid/Sanoid.csproj

install-doc:
	install -C -v $(SANOIDDOCDIR)/Sanoid.1 $(MANDIR)/man1/Sanoid.1
	cp -l $(MANDIR)/man1/Sanoid.1 $(MANDIR)/man1/Sanoid.net.1
	mandb

reinstall:	uninstall-release	clean-release	install-release

restore:
	dotnet restore -f

install:	install-release	install-doc

install-release:
	mkdir -p $(RELEASEPUBLISHDIR)
	dotnet publish --configuration $(RELEASECONFIG) --use-current-runtime --no-self-contained -r linux-x64 -o $(RELEASEPUBLISHDIR) Sanoid/Sanoid.csproj
	install --backup=existing -D -C -v -t /usr/local/share/Sanoid.net/ $(PUBLISHBASECONFIGFILELIST)
	[ ! -d /etc/sanoid ] && [ -w /etc ] && mkdir -p /etc/sanoid || true
	install --backup=existing -C -v $(RELEASEPUBLISHDIR)/Sanoid.local.json /etc/sanoid/Sanoid.local.json
	install --backup=existing -C -v $(RELEASEPUBLISHDIR)/Sanoid /usr/local/bin/Sanoid

uninstall:	uninstall-release	uninstall-doc

uninstall-doc:
	rm -fv /usr/local/man/man1/Sanoid.1
	rm -fv /usr/local/man/man1/Sanoid.net.1
	mandb

uninstall-release:
	rm -rfv /usr/local/share/Sanoid.net
	rm -rfv /etc/sanoid/Sanoid.local.json
	rm -fv /usr/local/bin/Sanoid
	@echo "Not removing configuration files in /etc/sanoid or user home directories."

test:	test-everything

test-everything:
	dotnet test --settings=Sanoid.Common.Tests/everything.runsettings

test-everything-dangerous:
	dotnet test --settings=Sanoid.Common.Tests/everything-dangerous.runsettings

test-dangerous:
	dotnet test --settings=Sanoid.Common.Tests/dangerous.runsettings
