#!/bin/bash
rm -fr ./build

mkdir -p ./build

cmake -S . -B ./build -DFRAMEWORK=True -G Xcode -DCMAKE_TOOLCHAIN_FILE=../ios.toolchain.cmake -DPLATFORM=$1 -DCMAKE_XCODE_ATTRIBUTE_DEVELOPMENT_TEAM=89S6WN62A4
cmake --build ./build -j 4 --config Release

#"OS64" 89S6WN62A4
