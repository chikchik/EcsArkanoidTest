#!/bin/sh

rm -fr ./build

NDK=$HOME/Library/Android/sdk/ndk/24.0.8215888/

cmake -S . -B ./build -DCMAKE_TOOLCHAIN_FILE=$NDK/build/cmake/android.toolchain.cmake -DANDROID_ABI=$1 -DANDROID_NATIVE_API_LEVEL=29
cmake --build ./build -j 4

#"arm64-v8a" "x86_64" "x86" "armeabi-v7a"