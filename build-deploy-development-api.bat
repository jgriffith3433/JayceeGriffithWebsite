@echo off
start /b /wait build-development-api.bat
call deploy-development-api.bat
exit 0
