@echo off
start /b /wait build-development-proxy.bat
call deploy-development-proxy.bat
exit 0
