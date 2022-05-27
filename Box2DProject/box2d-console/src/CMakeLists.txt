include_directories(${box2dConsole_SOURCE_DIR}/../box2d-framework/include/box2d/)
include_directories(${box2dConsole_SOURCE_DIR}/../box2d-unity-wrapper/box2dWrapper/inc)
include_directories(${box2dConsole_SOURCE_DIR}/inc)

link_directories(${box2dConsole_SOURCE_DIR}/../box2d-unity-wrapper/build/box2dWrapper/src/Debug)

file(GLOB SRC
     "${box2dConsole_SOURCE_DIR}/src/*.cpp"
)

file(GLOB INC
    "${box2dConsole_SOURCE_DIR}/inc/*.h"
)

add_executable(box2dConsole ${SRC})

target_link_libraries(box2dConsole libbox2d)
