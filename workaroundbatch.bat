@echo off
set arg1=%1
set arg2=%2
psvpfsparser.exe -i %arg1% -o temp -f cma.henkaku.xyz -z %arg2%