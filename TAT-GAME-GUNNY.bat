@echo off
chcp 65001 >nul
title [PORTGAME.CLOUD] - TAT TOAN BO MAY CHU & DOCKER
color 0C

echo ====================================================================
echo                   GUNNY 3.4 - PORTGAME.CLOUD
echo            DANG TAT TOAN BO MAY CHU & DOCKER DESKTOP...
echo ====================================================================
echo.

:: 1. Tat cac cua so server Gunny
echo [1/4] Dang dong cac cua so dich vu Gunny...
taskkill /F /FI "WINDOWTITLE eq Center.Service*" >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq Tank.Request*" >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq Fighting.Service*" >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq Road.Service*" >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq Cloudflare Tunnel*" >nul 2>&1

:: 2. Tat tien trinh Cloudflare Tunnel
echo [2/4] Dang tat tien trinh Cloudflare Tunnel...
taskkill /F /IM cloudflared.exe >nul 2>&1

:: 3. Giai phong tien trinh .NET
echo [3/4] Dang tat cac tien trinh .NET server...
powershell -NoProfile -Command "& { Get-CimInstance Win32_Process | Where-Object { `$_.Name -eq 'dotnet.exe' -and (`$_.CommandLine -like '*Center.Service*' -or `$_.CommandLine -like '*Tank.Request*' -or `$_.CommandLine -like '*Fighting.Service*' -or `$_.CommandLine -like '*Road.Service*') } | ForEach-Object { Stop-Process -Id `$_.ProcessId -Force -ErrorAction SilentlyContinue } }" >nul 2>&1

:: 4. Tat Docker container va dong hoan toan Docker Desktop de giai phong RAM
echo [4/4] Dang dung container gunny-sql va tat Docker Desktop...
docker stop gunny-sql >nul 2>&1
taskkill /F /IM "Docker Desktop.exe" >nul 2>&1
taskkill /F /IM "com.docker.backend.exe" >nul 2>&1
taskkill /F /IM "com.docker.proxy.exe" >nul 2>&1
taskkill /F /IM "com.docker.build.exe" >nul 2>&1
wsl --shutdown >nul 2>&1

echo.
echo ====================================================================
echo   [OK] Da tat toan bo Server Gunny, Cloudflare Tunnel va Docker!
echo   May ao WSL2 da shutdown, giai phong 3GB RAM cho may tinh.
echo ====================================================================
echo.
ping 127.0.0.1 -n 3 >nul
exit
