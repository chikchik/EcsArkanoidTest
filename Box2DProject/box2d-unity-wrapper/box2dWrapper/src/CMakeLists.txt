include_directories(${box2d_SOURCE_DIR}/box2d/headers)
include_directories(${box2d_SOURCE_DIR}/box2dWrapper/inc)
link_directories(${box2d_SOURCE_DIR}/box2d/lib)



file(GLOB SRC
     "${box2d_SOURCE_DIR}/box2dWrapper/src/*.cpp"
)

file(GLOB INC
    "${box2d_SOURCE_DIR}/box2dWrapper/inc/*.h"
)

add_library(libbox2d SHARED ${SRC} ${INC})

target_link_libraries(libbox2d LINK_PUBLIC Box2d)

if(DEFINED FRAMEWORK)
    set_target_properties(libbox2d PROPERTIES
    FRAMEWORK TRUE
    FRAMEWORK_VERSION CXX)
endif()
