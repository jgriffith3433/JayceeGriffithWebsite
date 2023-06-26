@echo off
echo ------------------------------------------------------------
echo ----------------------BUILDING------------------------------
echo ------------------------------------------------------------
echo Building client proxy
docker compose -f docker-compose-development.yaml --verbose build --force-rm --no-cache proxy

echo Done
exit 0