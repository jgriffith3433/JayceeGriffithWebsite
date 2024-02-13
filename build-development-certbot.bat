@echo off
echo ------------------------------------------------------------
echo ----------------------BUILDING------------------------------
echo ------------------------------------------------------------
echo Building client certbot
::docker compose -f docker-compose-development.yaml --verbose build --force-rm --no-cache certbot certonly --webroot-path /var/www/certbot/ --dry-run -d jayceegriffith.com
docker compose -f docker-compose-development.yaml --verbose build --force-rm --no-cache certbot
echo Done
exit 0