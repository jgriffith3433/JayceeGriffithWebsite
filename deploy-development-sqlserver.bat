@echo off
echo ------------------------------------------------------------
echo ----------------------PUSHING-------------------------------
echo ------------------------------------------------------------
echo Pushing sqlserver image
docker compose -f docker-compose-development.yaml --verbose push sqlserver

echo ------------------------------------------------------------
echo ----------------------STOPPING------------------------------
echo ------------------------------------------------------------
set sqlserverRunning=docker ps -q -f name=sqlserver

IF NOT "%sqlserverRunning%"=="" (
	echo Stopping sqlserver
    docker stop sqlserver
)

echo ------------------------------------------------------------
echo ----------------------REMOVING------------------------------
echo ------------------------------------------------------------
set sqlserverExists=docker ps -a -q -f name=sqlserver

IF NOT "%sqlserverExists%"=="" (
	echo Removing sqlserver
	docker rm sqlserver
)

echo ------------------------------------------------------------
echo ----------------------PULLING-------------------------------
echo ------------------------------------------------------------

echo Pulling sqlserver
docker image pull urvaius/jc-sqlserver:latest

echo ------------------------------------------------------------
echo ----------------------STARTING------------------------------
echo ------------------------------------------------------------

echo Starting sqlserver
docker run --name=sqlserver --hostname=sqlserver.jayceegriffith.com --env=PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin --network=jc -p 1433:1433 --restart=always --runtime=runc -d urvaius/jc-sqlserver:latest


echo Done
exit 0
