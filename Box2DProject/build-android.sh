#!/bin/sh

echo "Start building box2d-framework for Android"

cd ./box2d-framework/
./build-android.sh $1

cd ..
cp -r ./box2d-framework/build/bin/libbox2d.a ./box2d-unity-wrapper/box2d/lib

cd ./box2d-unity-wrapper
./build-android.sh $1
