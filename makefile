CONFIG?=Release
PREFIX?=prefix
PREFIX:=$(abspath $(PREFIX))
VERSION=2.0.20407
PROJECT=Toaster
SCRATCH=scratch
tools=Toast Butter Crumb
otherfiles=makefile template.sh
markdown=README LICENSE
lc=$(shell echo $(1) | tr A-Z a-z)
zipfile=$(PROJECT)-$(VERSION).tar.gz

define copyrule
$(1): $(2)
	cp $$< $$@

endef

define mkscriptrule
$$(PREFIX)/bin/$(call lc,$(1)): $$(PREFIX)/lib/$(1) $$(PREFIX)/lib/$(1)/$(1).exe
	sed -e 's,_TOOL_,$(1),g' -e 's,_PREFIX_,$$(PREFIX),g' template.sh > $$@
	chmod u+x $$@

$$(PREFIX)/lib/$(1):	
	mkdir -p $$@

$$(PREFIX)/lib/$(1)/$(1).exe: $(1).exe
	cp $$< $$@

endef

.PHONY: default
default:
	$(error Specify clean, dist or install)

.PHONY: dist
dist: $(SCRATCH) $(zipfile)
$(SCRATCH): 
	mkdir -p $(SCRATCH)	
$(zipfile): $(foreach X,$(tools),$(SCRATCH)/$(X).exe) \
			$(foreach X,$(otherfiles),$(SCRATCH)/$(X)) \
			$(foreach X,$(markdown),$(SCRATCH)/$(X))
	tar -cvz -C $(SCRATCH) -f $(zipfile) $(foreach file,$^,$(notdir $(file)))
	openssl sha1 $(zipfile)
#	aws s3 cp $(zipfile) s3://jlyonsmith/ --profile jamoki --acl public-read
$(foreach X,$(tools),$(eval $(call copyrule,$(SCRATCH)/$(X).exe,$(X)/bin/$(CONFIG)/$(X).exe)))
$(foreach X,$(otherfiles),$(eval $(call copyrule,$(SCRATCH)/$(X),$(X))))
$(foreach X,$(markdown),$(eval $(call copyrule,$(SCRATCH)/$(X),$(X).md)))

.PHONY: install
install: $(PREFIX)/bin $(PREFIX)/lib $(foreach X,$(tools),$(PREFIX)/bin/$(call lc, $(X)))
ifdef HOMEBREW
install: $(foreach X,$(markdown),$(PREFIX)/$(X))
endif
$(PREFIX)/bin $(PREFIX)/lib:
	mkdir -p $@
$(foreach X,$(tools),$(eval $(call mkscriptrule,$(X))))
ifdef HOMEBREW
$(foreach X,$(markdown),$(eval $(call copyrule,$(PREFIX)/$(X),$(X))))
endif

.PHONY: clean
clean:
	-@rm *.gz
	-@rm -rf $(SCRATCH)
