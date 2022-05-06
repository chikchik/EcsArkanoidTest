#!/bin/bash
rm -fr ./build

mkdir -p ./build

cmake -S . -B ./build -G Xcode -DCMAKE_TOOLCHAIN_FILE=../ios.toolchain.cmake -DPLATFORM=$1
cmake --build ./build -j 4 --config Release

#"OS64" "MAC"
