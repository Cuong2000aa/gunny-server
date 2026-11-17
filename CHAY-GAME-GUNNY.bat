@echo off
chcp 65001 >nul
title [PORTGAME.CLOUD] - KHOI DONG GUNNY 3.4 & DOCKER
color 0A

set "SERVER_DIR=c:\Users\NguyenChiCuong(ITPOS\Projects\gunny-server"

echo ====================================================================
echo                   GUNNY 3.4 - PORTGAME.CLOUD
echo             HE THONG KHOI DONG SERVER TU DONG 1-CLICK
echo ====================================================================
echo.

:: 1. Kiem tra va khoi dong Docker Desktop
tasklist /FI "IMAGENAME eq Docker Desktop.exe" 2>NUL | find /I /N "Docker Desktop.exe">NUL
if "%ERRORLEVEL%"=="1" (
    echo [1/6] Dang khoi dong Docker Desktop...
    start "" "C:\Program Files\Docker\Docker\Docker Desktop.exe"
)

echo [1/6] Dang cho Docker khoi dong va bat database gunny-sql...
set /a count=0
:wait_docker
docker start gunny-sql >nul 2>&1
if "%ERRORLEVEL%" NEQ "0" (
    set /a count+=1
    echo       Dang cho Docker san sang... (lan %count%)
    ping 127.0.0.1 -n 4 >nul
    goto wait_docker
)
echo       [OK] Database SQL Server da san sang (Port 1433)!
echo.

:: 2. Don dep tien trinh cu neu co
echo [2/6] Don dep cac port va tien trinh he thong...
taskkill /F /FI "WINDOWTITLE eq Center.Service*" >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq Tank.Request*" >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq Fighting.Service*" >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq Road.Service*" >nul 2>&1
powershell -NoProfile -Command "& { Get-CimInstance Win32_Process | Where-Object { `$_.Name -eq 'dotnet.exe' -and (`$_.CommandLine -like '*Center.Service*' -or `$_.CommandLine -like '*Fighting.Service*' -or `$_.CommandLine -like '*Road.Service*' -or `$_.CommandLine -like '*Tank.Request*') } | ForEach-Object { Stop-Process -Id `$_.ProcessId -Force -ErrorAction SilentlyContinue } }" >nul 2>&1

:: 3. Khoi dong Center.Service
echo [3/6] Dang bat Center.Service (Port 9202)...
start "Center.Service" /min cmd /c "cd /d ""%SERVER_DIR%\Center.Service"" && dotnet bin\Release\net8.0\Center.Service.dll"
ping 127.0.0.1 -n 3 >nul

:: 4. Khoi dong Tank.Request
echo [4/6] Dang bat Tank.Request (Port 828 va WebSocket Proxy)...
start "Tank.Request" /min cmd /c "cd /d ""%SERVER_DIR%\Tank.Request"" && dotnet bin\Release\net8.0\Tank.Request.dll"
ping 127.0.0.1 -n 3 >nul

:: 5. Khoi dong Fighting va Road
echo [5/6] Dang bat Fighting.Service va Road.Service (Port 9500)...
start "Fighting.Service" /min cmd /c "cd /d ""%SERVER_DIR%\Fighting.Service"" && dotnet bin\Release\net8.0\Fighting.Service.dll"
start "Road.Service" /min cmd /c "cd /d ""%SERVER_DIR%\Road.Service"" && dotnet bin\Release\net8.0\Road.Service.dll"
ping 127.0.0.1 -n 4 >nul

:: 6. Khoi dong Cloudflare Tunnel (neu chua chay)
echo [6/6] Dang ket noi Cloudflare Tunnel (portgame.cloud)...
tasklist /FI "IMAGENAME eq cloudflared.exe" 2>NUL | find /I /N "cloudflared.exe">NUL
if "%ERRORLEVEL%"=="1" (
    start "Cloudflare Tunnel" /min cmd /c "cloudflared tunnel run gunny"
    ping 127.0.0.1 -n 4 >nul
) else (
    echo       Cloudflare Tunnel da duoc ket noi san!
)

:: 7. Mo game tren trinh duyet
echo.
echo ====================================================================
echo   [THANH CONG] Toan bo he thong da san sang hoat dong!
echo   Dang mo trinh duyet den: https://gunny.portgame.cloud
echo.
echo   Luu y: Cac cua so server dang thu nho xuong Taskbar.
echo   Khi nao muon tat ca game va Docker, hay chay file "TAT-GAME-GUNNY.bat".
echo ====================================================================
echo.

start https://gunny.portgame.cloud

ping 127.0.0.1 -n 4 >nul
exit
