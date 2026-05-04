@echo off
echo Iniciando BankMore...
docker context use default >nul 2>&1
docker compose --progress plain up --build
