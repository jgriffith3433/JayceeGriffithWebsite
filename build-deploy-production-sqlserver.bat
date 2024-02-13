@echo off
start /b /wait build-production-sqlserver.bat
call deploy-production-sqlserver.bat
exit 0
