@echo off
echo ------------------------------------------------------------
echo ----------------------BUILDING------------------------------
echo ------------------------------------------------------------
echo Building sql server

docker compose -f docker-compose-development.yaml --verbose build --force-rm --no-cache sqlserver

echo Done
exit 0