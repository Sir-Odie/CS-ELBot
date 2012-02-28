SHELL = /bin/bash

srcdir = .
top_srcdir = .

DESTDIR = ./bin

all:
	mkdir -p $(DESTDIR)/Release
	gmcs -out:$(DESTDIR)/Release/cs-elbot.exe `find . -name '*.cs' -print` -r:./MySql.Data.dll

debug:
	mkdir -p $(DESTDIR)/Debug
	gmcs -debug -out:$(DESTDIR)/Debug/cs-elbot.exe `find . -name '*.cs' -print` -r:./MySql.Data.dll

clean:
	rm -rf $(DESTDIR)
        
install:
	echo not implemented
