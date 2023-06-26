@echo off
start /b /wait build-production-proxy.bat
call deploy-production-proxy.bat
exit 0
