@echo off
chcp 65001 >nul
echo Dang tat cac dich vu Gunny...
for /f "tokens=2" %%i in ('wmic process where "name='dotnet.exe' and CommandLine like '%%Center.Service%%'" get ProcessId 2^>nul') do taskkill /F /PID %%i >nul 2>&1
for /f "tokens=2" %%i in ('wmic process where "name='dotnet.exe' and CommandLine like '%%Tank.Request%%'" get ProcessId 2^>nul') do taskkill /F /PID %%i >nul 2>&1
for /f "tokens=2" %%i in ('wmic process where "name='dotnet.exe' and CommandLine like '%%Fighting.Service%%'" get ProcessId 2^>nul') do taskkill /F /PID %%i >nul 2>&1
for /f "tokens=2" %%i in ('wmic process where "name='dotnet.exe' and CommandLine like '%%Road.Service%%'" get ProcessId 2^>nul') do taskkill /F /PID %%i >nul 2>&1
echo Da tat toan bo dich vu!
pause
