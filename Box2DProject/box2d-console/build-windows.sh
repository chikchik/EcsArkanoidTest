#!/bin/sh

buildWrapperAndLink()
{
    local LIB_PATH="./libbox2d.dll"
    local GAME_PATH="./build/src/Debug"

    rm -rf ./out && rm -rf ./build && (cmake -S . -B ./build) && cmake --build ./build -j 4 && mv build/src/Debug/box2dConsole.exe ./ && (echo "$(tput setaf 2)BUILD COMPLETE, COPYING...$(tput setaf 3)")
}

buildWrapperAndLink