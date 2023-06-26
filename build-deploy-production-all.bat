@echo off
start /b /wait build-production-api.bat
start /b /wait build-production-client.bat
start /b /wait build-production-proxy.bat
start cmd /c deploy-production-all.bat
exit 0
