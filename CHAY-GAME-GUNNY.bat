@echo off
chcp 65001 >nul
title [PORTGAME.CLOUD] - KHOI DONG GUNNY 3.4 & DOCKER
color 0A

echo ====================================================================
echo                   GUNNY 3.4 - PORTGAME.CLOUD
echo             HE THONG KHOI DONG SERVER TU DONG 1-CLICK
echo ====================================================================
echo.

:: 1. Kiem tra va khoi dong Docker Desktop neu chua chay
tasklist /FI "IMAGENAME eq Docker Desktop.exe" 2>NUL | find /I /N "Docker Desktop.exe">NUL
if "%ERRORLEVEL%"=="0" goto docker_running
echo [1/6] Dang khoi dong Docker Desktop...
start "" "C:\Program Files\Docker\Docker\Docker Desktop.exe"

:docker_running
echo [1/6] Dang cho Docker san sang va bat database gunny-sql...
set /a count=0
:wait_docker
docker start gunny-sql >nul 2>&1
if "%ERRORLEVEL%"=="0" goto docker_ready
set /a count+=1
echo       Dang cho Docker khoi dong engine - thu lan %count%...
ping 127.0.0.1 -n 4 >nul
goto wait_docker

:docker_ready
echo       [OK] Database SQL Server da san sang (Port 1433)!
echo.

:: 2. Don dep cac port va tien trinh cu
echo [2/6] Don dep cac port va tien trinh he thong...
taskkill /F /FI "WINDOWTITLE eq Center.Service*" >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq Tank.Request*" >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq Fighting.Service*" >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq Road.Service*" >nul 2>&1
taskkill /F /IM dotnet.exe >nul 2>&1

:: 3. Khoi dong Center.Service
echo [3/6] Dang bat Center.Service (Port 9202)...
start "Center.Service" /min cmd /c "cd /d c:\Users\NGUYEN~1\Projects\GUNNY-~1\Center.Service && dotnet bin\Release\net8.0\Center.Service.dll"
ping 127.0.0.1 -n 3 >nul

:: 4. Khoi dong Tank.Request
echo [4/6] Dang bat Tank.Request (Port 828 va WebSocket Proxy)...
start "Tank.Request" /min cmd /c "cd /d c:\Users\NGUYEN~1\Projects\GUNNY-~1\Tank.Request && dotnet bin\Release\net8.0\Tank.Request.dll"
ping 127.0.0.1 -n 3 >nul

:: 5. Khoi dong Fighting va Road
echo [5/6] Dang bat Fighting.Service va Road.Service (Port 9500)...
start "Fighting.Service" /min cmd /c "cd /d c:\Users\NGUYEN~1\Projects\GUNNY-~1\Fighting.Service && dotnet bin\Release\net8.0\Fighting.Service.dll"
start "Road.Service" /min cmd /c "cd /d c:\Users\NGUYEN~1\Projects\GUNNY-~1\Road.Service && dotnet bin\Release\net8.0\Road.Service.dll"
ping 127.0.0.1 -n 4 >nul

:: 6. Khoi dong Cloudflare Tunnel (neu chua chay)
echo [6/6] Dang ket noi Cloudflare Tunnel (portgame.cloud)...
tasklist /FI "IMAGENAME eq cloudflared.exe" 2>NUL | find /I /N "cloudflared.exe">NUL
if "%ERRORLEVEL%"=="0" goto tunnel_running
start "Cloudflare Tunnel" /min cmd /c "cloudflared tunnel run gunny"
ping 127.0.0.1 -n 4 >nul
goto tunnel_done
:tunnel_running
echo       Cloudflare Tunnel da duoc ket noi san!
:tunnel_done

:: 7. Mo game tren trinh duyet
echo.
echo ====================================================================
echo   [THANH CONG] Toan bo he thong da bat thanh cong!
echo   Dang mo trinh duyet den: https://gunny.portgame.cloud
echo.
echo   Luu y: Cua so nay giu cho may chu hoat dong on dinh.
echo   Ban co the thu nho cua so nay xuong Taskbar.
echo.
echo   Khi nao muon TAT GAME VA TAT LUON DOCKER:
echo   Hay nhan phim bat ky vao cua so nay (hoac mo TAT-GAME-GUNNY.bat)
echo ====================================================================
echo.

start https://gunny.portgame.cloud

pause
call "%~dp0TAT-GAME-GUNNY.bat"
