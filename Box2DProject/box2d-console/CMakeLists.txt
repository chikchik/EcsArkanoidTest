cmake_minimum_required(VERSION 3.9)
project(box2dConsole VERSION 1.0.1 DESCRIPTION "box2d to unity wrapper test console app" LANGUAGES CXX C)

set(CMAKE_CXX_STANDARD 14)

set(CMAKE_WINDOWS_EXPORT_ALL_SYMBOLS ON)

add_subdirectory(src)