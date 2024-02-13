@echo off
echo ------------------------------------------------------------
echo --------------------GETTING CERT----------------------------
echo ------------------------------------------------------------

::docker run --name=certbot --hostname=certbot.jayceegriffith.com --network=jc --restart=always --runtime=runc -d urvaius/jc-certbot:latest
::docker run --rm certbot renew
::docker-compose run --rm certbot renew
docker compose -f docker-compose-development.yaml run --rm certbot certonly --webroot --webroot-path /var/www/certbot/ --dry-run -d jayceegriffith.com 
echo Done
exit 0
