@echo off
echo ------------------------------------------------------------
echo ----------------------PUSHING-------------------------------
echo ------------------------------------------------------------
echo Pushing client image
docker compose -f docker-compose-production.yaml --verbose push client

echo ------------------------------------------------------------
echo ----------------------STOPPING------------------------------
echo ------------------------------------------------------------
set clientRunning=docker ps -q -f name=client

IF NOT "%clientRunning%"=="" (
	echo Stopping client
    docker stop client
)

echo ------------------------------------------------------------
echo ----------------------REMOVING------------------------------
echo ------------------------------------------------------------
set clientExists=docker ps -a -q -f name=client

IF NOT "%clientExists%"=="" (
	echo Removing client
	docker rm client
)

echo ------------------------------------------------------------
echo ----------------------PULLING-------------------------------
echo ------------------------------------------------------------

echo Pulling client
docker image pull urvaius/wof-client:latest

echo ------------------------------------------------------------
echo ----------------------STARTING------------------------------
echo ------------------------------------------------------------

echo Starting client
docker run --name=client --hostname=client.jaycee.margravesenclave.com --env=PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin --env=NGINX_VERSION=1.23.3 --env=NJS_VERSION=0.7.9 --env=PKG_RELEASE=1~bullseye --network=wof -p 9000:9000 --runtime=runc -d urvaius/wof-client:latest


echo Done
exit 0
