#!/bin/sh

echo "Start building box2d-framework for iOS"

cd ./box2d-framework/
./build-ios.sh OS64

cd ..
cp -r ./box2d-framework/build/bin/Release/libbox2d.a ./box2d-unity-wrapper/box2d/lib

cd ./box2d-unity-wrapper
./build-ios.sh OS64
#xcodebuild -configuration Release -project box2dlib.xcodeproj -scheme iOS build
