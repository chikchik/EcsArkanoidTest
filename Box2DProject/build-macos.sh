#!/bin/sh

echo "Start building box2d-framework for MacOS"

cd ./box2d-framework/
./build-macos.sh

cd ..
cp -r ./box2d-framework/build/bin/libbox2d.a ./box2d-unity-wrapper/box2d/lib

cd ./box2d-unity-wrapper
./build-macos.sh
