@echo off
start /b /wait build-production-client.bat
call deploy-production-client.bat
exit 0
