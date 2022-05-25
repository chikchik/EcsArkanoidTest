@echo off
SET TO=headers
SET FROM=..\..\box2d-framework\include\box2d

mklink /j "%TO%" "%FROM%"
EXIT 0
