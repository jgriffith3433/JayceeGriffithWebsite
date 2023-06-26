@echo off
echo ------------------------------------------------------------
echo ----------------------PUSHING-------------------------------
echo ------------------------------------------------------------
echo Pushing api image
docker compose -f docker-compose-development.yaml --verbose push api

echo ------------------------------------------------------------
echo ----------------------STOPPING------------------------------
echo ------------------------------------------------------------
set apiRunning=docker ps -q -f name=api

IF NOT "%apiRunning%"=="" (
	echo Stopping api
    docker stop api
)

echo ------------------------------------------------------------
echo ----------------------REMOVING------------------------------
echo ------------------------------------------------------------
set apiExists=docker ps -a -q -f name=api

IF NOT "%apiExists%"=="" (
	echo Removing api
	docker rm api
)

echo ------------------------------------------------------------
echo ----------------------PULLING-------------------------------
echo ------------------------------------------------------------

echo Pulling api
docker image pull urvaius/wof-api:latest

echo ------------------------------------------------------------
echo ----------------------STARTING------------------------------
echo ------------------------------------------------------------

echo Starting api
docker run --name=api --env=ASPNETCORE_ENVIRONMENT=Development --env-file wof-development-secrets.env --hostname=api.jaycee.margravesenclave.com --env=PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin --env=ASPNETCORE_URLS=http://+:80 --env=DOTNET_RUNNING_IN_CONTAINER=true --env=DOTNET_VERSION=6.0.16 --env=ASPNET_VERSION=6.0.16 --network=wof --workdir=/app -p 5000:5000 --runtime=runc -d urvaius/wof-api:latest


echo Done
exit 0
