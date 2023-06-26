@echo off
start /b /wait build-development-api.bat
start /b /wait build-development-client.bat
start /b /wait build-development-proxy.bat
start cmd /c deploy-development-all.bat
exit 0
