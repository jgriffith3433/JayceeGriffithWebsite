@echo off
echo ------------------------------------------------------------
echo ----------------------PUSHING-------------------------------
echo ------------------------------------------------------------
echo Pushing certbot image
docker compose -f docker-compose-development.yaml --verbose push certbot

echo ------------------------------------------------------------
echo ----------------------STOPPING------------------------------
echo ------------------------------------------------------------
set certbotRunning=docker ps -q -f name=certbot

IF NOT "%certbotRunning%"=="" (
	echo Stopping certbot
    docker stop certbot
)

echo ------------------------------------------------------------
echo ----------------------REMOVING------------------------------
echo ------------------------------------------------------------
set certbotExists=docker ps -a -q -f name=certbot

IF NOT "%certbotExists%"=="" (
	echo Removing certbot
	docker rm certbot
)

echo ------------------------------------------------------------
echo ----------------------PULLING-------------------------------
echo ------------------------------------------------------------

echo Pulling certbot
docker image pull urvaius/jc-certbot:latest

echo ------------------------------------------------------------
echo ----------------------STARTING------------------------------
echo ------------------------------------------------------------

echo Starting certbot
docker run --name=certbot --hostname=certbot.jayceegriffith.com --network=jc --restart=always --runtime=runc -d urvaius/jc-certbot:latest
::docker run --rm certbot renew
::docker-compose run --rm certbot renew

echo Done
exit 0
