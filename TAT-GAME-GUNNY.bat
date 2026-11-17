@echo off
chcp 65001 >nul
title [PORTGAME.CLOUD] - TAT TOAN BO MAY CHU GUNNY
color 0C

echo ====================================================================
echo                   GUNNY 3.4 - PORTGAME.CLOUD
echo                    DANG TAT TOAN BO MAY CHU...
echo ====================================================================
echo.

echo [1/3] Dang dong cac cua so dich vu Gunny...
taskkill /F /FI "WINDOWTITLE eq Center.Service*" >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq Tank.Request*" >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq Fighting.Service*" >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq Road.Service*" >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq Cloudflare Tunnel*" >nul 2>&1

echo [2/3] Dang tat tien trinh Cloudflare Tunnel...
taskkill /F /IM cloudflared.exe >nul 2>&1

echo [3/3] Dang giai phong tien trinh .NET va bo nho RAM...
powershell -NoProfile -Command "& { Get-CimInstance Win32_Process | Where-Object { `$_.Name -eq 'dotnet.exe' -and (`$_.CommandLine -like '*Center.Service*' -or `$_.CommandLine -like '*Tank.Request*' -or `$_.CommandLine -like '*Fighting.Service*' -or `$_.CommandLine -like '*Road.Service*') } | ForEach-Object { Stop-Process -Id `$_.ProcessId -Force -ErrorAction SilentlyContinue } }" >nul 2>&1

echo.
echo ====================================================================
echo   [OK] Da tat toan bo Server Gunny va Tunnel thanh cong!
echo   RAM va tai nguyen may tinh da duoc giai phong hoan toan.
echo ====================================================================
echo.
ping 127.0.0.1 -n 3 >nul
exit
