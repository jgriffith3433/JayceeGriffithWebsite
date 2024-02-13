@echo off
start /b /wait build-development-sqlserver.bat
call deploy-development-sqlserver.bat
exit 0
