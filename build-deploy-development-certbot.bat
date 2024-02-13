@echo off
start /b /wait build-development-certbot.bat
call deploy-development-certbot.bat
exit 0
