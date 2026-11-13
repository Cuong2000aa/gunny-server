@echo off
REM Run after git pull on Windows if Road/Fighting report missing fore.map
set ROOT=%~dp0
cd /d "%ROOT%"

if not exist "GameAssets\map\1001\fore.map" (
  echo [X] Missing GameAssets\map — pull latest from git first.
  exit /b 1
)

echo Syncing map/bomb into Road.Service and Fighting.Service ...
if exist "Road.Service\map" rmdir /s /q "Road.Service\map" 2>nul
if exist "Road.Service\bomb" rmdir /s /q "Road.Service\bomb" 2>nul
if exist "Fighting.Service\map" rmdir /s /q "Fighting.Service\map" 2>nul
if exist "Fighting.Service\bomb" rmdir /s /q "Fighting.Service\bomb" 2>nul
REM remove leftover symlink stubs
del /f /q "Road.Service\map" 2>nul
del /f /q "Road.Service\bomb" 2>nul
del /f /q "Fighting.Service\map" 2>nul
del /f /q "Fighting.Service\bomb" 2>nul

xcopy /E /I /Y "GameAssets\map" "Road.Service\map\" >nul
xcopy /E /I /Y "GameAssets\bomb" "Road.Service\bomb\" >nul
xcopy /E /I /Y "GameAssets\map" "Fighting.Service\map\" >nul
xcopy /E /I /Y "GameAssets\bomb" "Fighting.Service\bomb\" >nul

if exist "Road.Service\map\1001\fore.map" (
  echo [OK] Road.Service\map ready
) else (
  echo [X] Sync failed
  exit /b 1
)
echo Done. Restart Road + Fighting.
