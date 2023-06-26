@echo off
start cmd /c deploy-production-api.bat
start cmd /c deploy-production-client.bat
start cmd /c deploy-production-proxy.bat
exit 0
