@echo off
echo ------------------------------------------------------------
echo ----------------------PUSHING-------------------------------
echo ------------------------------------------------------------
echo Pushing proxy image
docker compose -f docker-compose-production.yaml --verbose push proxy

echo ------------------------------------------------------------
echo ----------------------STOPPING------------------------------
echo ------------------------------------------------------------
set proxyRunning=docker ps -q -f name=proxy

IF NOT "%proxyRunning%"=="" (
	echo Stopping proxy
    docker stop proxy
)

echo ------------------------------------------------------------
echo ----------------------REMOVING------------------------------
echo ------------------------------------------------------------
set proxyExists=docker ps -a -q -f name=proxy

IF NOT "%proxyExists%"=="" (
	echo Removing proxy
	docker rm proxy
)

echo ------------------------------------------------------------
echo ----------------------PULLING-------------------------------
echo ------------------------------------------------------------

echo Pulling proxy
docker image pull urvaius/wof-proxy:latest

echo ------------------------------------------------------------
echo ----------------------STARTING------------------------------
echo ------------------------------------------------------------

echo Starting proxy
docker run --name=proxy --hostname=proxy.jaycee.margravesenclave.com --env=PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin --env=NGINX_VERSION=1.23.3 --env=NJS_VERSION=0.7.9 --env=PKG_RELEASE=1~bullseye --network=wof -p 80:80 --restart=always --runtime=runc -d urvaius/wof-proxy:latest


echo Done
exit 0
