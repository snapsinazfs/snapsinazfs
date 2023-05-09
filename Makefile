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

PUBLISHBASECONFIGFILELIST = $(RELEASEPUBLISHDIR)/Sanoid.json $(RELEASEPUBLISHDIR)/Sanoid.nlog.json $(RELEASEPUBLISHDIR)/Sanoid.schema.json $(RELEASEPUBLISHDIR)/Sanoid.template.schema.json
PUBLISHBASECONFIGFILELIST += $(RELEASEPUBLISHDIR)/Sanoid.dataset.schema.json $(RELEASEPUBLISHDIR)/Sanoid.monitoring.schema.json $(RELEASEPUBLISHDIR)/Sanoid.local.schema.json

all:    clean-release   restore build-release

clean:  clean-all

clean-all:      clean-debug     clean-release

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

extraclean:     clean-debug     clean-release
        rm -rfv Sanoid/bin
        rm -rfv Sanoid/obj
        rm -rfv Sanoid.Common/bin
        rm -rfv Sanoid.Common/obj


build:  build-release

build-debug:
        mkdir -p $(DEBUGDIR)
        dotnet build --configuration $(DEBUGCONFIG) -o $(DEBUGDIR)

build-release:
        mkdir -p $(RELEASEDIR)
        dotnet build --configuration $(RELEASECONFIG) -o $(RELEASEDIR) --use-current-runtime --no-self-contained -r linux-x64 Sanoid/Sanoid.csproj

restore:
        dotnet restore -f

install:        install-release

install-release:
        mkdir -p $(RELEASEPUBLISHDIR)
        dotnet publish --configuration $(RELEASECONFIG) --use-current-runtime --no-self-contained -r linux-x64 -o $(RELEASEPUBLISHDIR) Sanoid/Sanoid.csproj
        install --backup=existing -D -C -v -t /usr/local/share/Sanoid.net/ $(PUBLISHBASECONFIGFILELIST)
        [ ! -d /etc/sanoid ] && [ -w /etc ] && mkdir -p /etc/sanoid || true
        install --backup=existing -C -v $(RELEASEPUBLISHDIR)/Sanoid.local.json /etc/sanoid/Sanoid.local.json
        install --backup=existing -C -v $(RELEASEPUBLISHDIR)/Sanoid /usr/local/bin/Sanoid

uninstall:      uninstall-release

uninstall-release:
        rm -rfv /usr/local/share/Sanoid.net
        rm -rfv /etc/sanoid/Sanoid.local.json
        rm -fv /usr/local/bin/Sanoid
        @echo "Not removing configuration files in user home directories."