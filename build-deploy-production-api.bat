@echo off
start /b /wait build-production-api.bat
call deploy-production-api.bat
exit 0
