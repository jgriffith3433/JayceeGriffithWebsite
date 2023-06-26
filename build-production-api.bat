@echo off
echo ------------------------------------------------------------
echo ----------------------BUILDING------------------------------
echo ------------------------------------------------------------
echo Building api image
docker compose -f docker-compose-production.yaml --verbose build --force-rm --no-cache api

echo Done
exit 0