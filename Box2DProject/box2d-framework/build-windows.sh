

#!/bin/sh

rm -fr ./build

cmake -S . -B ./build -DCMAKE_TOOLCHAIN_FILE=../windows.toolchain.cmake -DCMAKE_INSTALL_PREFIX=${USER_ROOT_PATH} ..
cmake --build ./build -j 4