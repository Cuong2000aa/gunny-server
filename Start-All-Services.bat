@echo off
chcp 65001 >nul
echo [1/5] Kiem tra Docker DB...
docker start gunny-sql >nul 2>&1

echo [2/5] Khoi dong Center.Service...
start "Center.Service" /min cmd /c "cd /d %~dp0Center.Service && dotnet bin\Release\net8.0\Center.Service.dll"
timeout /t 2 >nul

echo [3/5] Khoi dong Tank.Request...
start "Tank.Request" /min cmd /c "cd /d %~dp0Tank.Request && dotnet bin\Release\net8.0\Tank.Request.dll"
timeout /t 2 >nul

echo [4/5] Khoi dong Fighting.Service & Road.Service...
start "Fighting.Service" /min cmd /c "cd /d %~dp0Fighting.Service && dotnet bin\Release\net8.0\Fighting.Service.dll"
start "Road.Service" /min cmd /c "cd /d %~dp0Road.Service && dotnet bin\Release\net8.0\Road.Service.dll"
timeout /t 2 >nul

echo [5/5] Khoi dong Cloudflare Tunnel (portgame.cloud)...
start "Cloudflare Tunnel" /min cmd /c "cloudflared tunnel run gunny"

echo ========================================================
echo   GUNNY 3.4 SERVER DA KHOI DONG THANH CONG!
echo   Link choi: https://gunny.portgame.cloud/web/play.html
echo   Link goc:  https://portgame.cloud/web/play.html
echo ========================================================
pause
