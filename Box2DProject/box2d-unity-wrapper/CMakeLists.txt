cmake_minimum_required(VERSION 3.9)
project(box2d VERSION 1.0.1 DESCRIPTION "box2d to unity wrapper lib")

set(CMAKE_CXX_STANDARD 14)

include_directories(box2d/headers)
link_directories(box2d/lib)

add_library(libbox2d SHARED Box2DToUnityWrapper.cpp)

target_link_libraries(libbox2d LINK_PUBLIC Box2d)

if(DEFINED FRAMEWORK)
    set_target_properties(libbox2d PROPERTIES
    FRAMEWORK TRUE
    FRAMEWORK_VERSION CXX)
endif()