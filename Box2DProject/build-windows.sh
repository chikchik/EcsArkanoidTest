#!/bin/sh

echo "Start building box2d-framework for Windows"

cd ./box2d-framework/
./build-windows.sh

cd ..
cp -r ./box2d-framework/build/bin/Debug/box2d.lib ./box2d-unity-wrapper/box2d/lib

cd ./box2d-unity-wrapper

buildWrapperAndLink()
{
    local LIB_PATH="./build/box2dWrapper/src/Debug/libbox2d.dll"
    local LIB_DBG_PATH="./build/box2dWrapper/src/Debug/libbox2d.pdb"
    local SERVER_PATH="../../ServerApp/"
    local GAME_PATH="../../Project/Assets/Plugins/LibBox2D/Windows"
    local TEST_PATH="../box2d-console"

    ./build-windows.sh && (echo "$(tput setaf 2)BUILD COMPLETE, COPYING...$(tput setaf 3)" && cp -v $LIB_PATH $SERVER_PATH && cp -v $LIB_PATH $GAME_PATH && cp -v $LIB_PATH $TEST_PATH && cp -v $LIB_DBG_PATH $SERVER_PATH && cp -v $LIB_DBG_PATH $GAME_PATH && cp -v $LIB_DBG_PATH $TEST_PATH && echo "$(tput setaf 2)DONE!" || echo "$(tput setaf 1)ERROR!") || echo "$(tput setaf 1)BUILD ERROR!"
}

buildWrapperAndLink
