#!/bin/sh

echo "Start building box2d-framework for Windows"

cd ./box2d-framework/
./build-windows.sh

cd ..
cp -r ./box2d-framework/build/bin/Debug/box2d.lib ./box2d-unity-wrapper/box2d/lib

cd ./box2d-unity-wrapper
./build-windows.sh && (echo "$(tput setaf 2)BUILD COMPLETE, COPYING...$(tput setaf 3)" && cp -v ./build/Debug/libbox2d.dll ../../ServerApp/ && cp -v ./build/Debug/libbox2d.dll ../../Project/Assets/Plugins/LibBox2D/Windows && echo "$(tput setaf 2)DONE!" || echo "$(tput setaf 1)ERROR!") || echo "$(tput setaf 1)BUILD ERROR!"

