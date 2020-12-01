CXX=g++
CXXFLAGS=-Wall -Werror -pedantic -std=c++17

MAIN=main.cpp
EXEC=$(patsubst %.cpp,%.out,$(MAIN))

HDR=solver.h stargrid.h
SRC=solver.cpp

all: $(EXEC)

release: CXXFLAGS+=-O2
release: all

optim: CXXFLAGS+=-O2 -g
optim: all

debug: CXXFLAGS+=-g
debug: all

profile: CXXFLAGS+=-pg -O2
profile: all

$(EXEC): $(MAIN) $(SRC) $(HDR)
	$(CXX) $(CXXFLAGS) -o $(EXEC) $(MAIN) $(SRC)

clean:
	rm -f $(EXEC)

.PHONY: all debug release clean all optim
