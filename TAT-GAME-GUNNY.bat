@echo off
chcp 65001 >nul
title [PORTGAME.CLOUD] - TAT TOAN BO MAY CHU & DOCKER
color 0C

echo ====================================================================
echo                   GUNNY 3.4 - PORTGAME.CLOUD
echo            DANG TAT TOAN BO MAY CHU VA DOCKER...
echo ====================================================================
echo.

echo [1/4] Dang tat cac cua so dich vu Gunny...
taskkill /F /FI "WINDOWTITLE eq Center.Service*" >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq Tank.Request*" >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq Fighting.Service*" >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq Road.Service*" >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq Cloudflare Tunnel*" >nul 2>&1
taskkill /F /IM cloudflared.exe >nul 2>&1

echo [2/4] Dang tat cac tien trinh .NET...
taskkill /F /IM dotnet.exe >nul 2>&1

echo [3/4] Dang dung container SQL Server...
docker stop gunny-sql >nul 2>&1

echo [4/4] Dang tat Docker Desktop va giai phong RAM...
taskkill /F /IM "Docker Desktop.exe" >nul 2>&1
taskkill /F /IM "com.docker.backend.exe" >nul 2>&1
taskkill /F /IM "com.docker.proxy.exe" >nul 2>&1
taskkill /F /IM "com.docker.build.exe" >nul 2>&1
wsl --shutdown >nul 2>&1

echo.
echo ====================================================================
echo   [OK] Da tat toan bo Server Gunny, Cloudflare va Docker!
echo   May ao WSL2 da shutdown, giai phong 3GB RAM cho may tinh.
echo ====================================================================
echo.
ping 127.0.0.1 -n 3 >nul
exit
