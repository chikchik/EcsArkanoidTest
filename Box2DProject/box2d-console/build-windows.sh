#!/bin/sh

rm -rf ./build && rm -rf ./box2dConsole.exe && (cmake -S . -B ./build) && cmake --build ./build -j 4 && mv build/src/Debug/box2dConsole.exe ./

