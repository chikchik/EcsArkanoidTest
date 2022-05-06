#!/bin/bash
rm -fr ./build

mkdir -p ./build

cmake -S . -B ./build
cmake --build ./build -j
