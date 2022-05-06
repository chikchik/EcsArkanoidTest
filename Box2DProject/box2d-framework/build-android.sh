#!/bin/sh

#"arm64-v8a" "x86_64" "x86" "armeabi-v7a"
rm -fr ./build

NDK=$HOME/Library/Android/sdk/ndk/24.0.8215888/

cmake -S . -B ./build -DCMAKE_TOOLCHAIN_FILE=$NDK/build/cmake/android.toolchain.cmake -DANDROID_ABI=$1 -DANDROID_NATIVE_API_LEVEL=29
cmake --build ./build -j 4

#rm -fr ./build
#
#for ABI in "arm64-v8a" "x86_64" "x86" "armeabi-v7a"; do
#
#    DESTINATION_DIR=./build/opus/$ABI
#    mkdir -p $DESTINATION_DIR
#
#    cmake -S . -B $DESTINATION_DIR
#        -DCMAKE_TOOLCHAIN_FILE=$NDK/build/cmake/android.toolchain.cmake
#        -DANDROID_ABI=$ABI
#        -DANDROID_NATIVE_API_LEVEL=29
#
#    cmake --build $DESTINATION_DIR -j 4
#
#done
