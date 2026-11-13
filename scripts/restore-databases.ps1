# Restore Database/*.bak into Docker gunny-sql, then apply admin full patch.
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$saPassword = "Gunny@2026Mac"
$container = "gunny-sql"
$bakDir = "/var/opt/mssql/backup"
$dataDir = "/var/opt/mssql/data"

Write-Host "== Ensure SQL container ==" -ForegroundColor Cyan
$sql = docker ps -a --filter "name=$container" --format "{{.Names}} {{.Status}}" 2>$null
if (-not $sql) {
    docker run -d --name $container `
      -e "ACCEPT_EULA=Y" `
      -e "MSSQL_SA_PASSWORD=$saPassword" `
      -p 1433:1433 `
      mcr.microsoft.com/azure-sql-edge:latest | Out-Null
} elseif ($sql -notmatch "Up") {
    docker start $container | Out-Null
}

Write-Host "Waiting for SQL..." -ForegroundColor DarkCyan
for ($i = 1; $i -le 60; $i++) {
    docker exec $container /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $saPassword -Q "SELECT 1" -h -1 2>$null | Out-Null
    if ($LASTEXITCODE -eq 0) { break }
    if ($i -eq 60) { throw "SQL not ready" }
    Start-Sleep -Seconds 2
}

docker exec $container mkdir -p $bakDir 2>$null | Out-Null
docker cp "$root\Database\Db_Membership.bak" "${container}:${bakDir}/Db_Membership.bak"
docker cp "$root\Database\Game34.bak" "${container}:${bakDir}/Game34.bak"
docker cp "$root\Database\Player34.bak" "${container}:${bakDir}/Player34.bak"

function Invoke-Sql([string]$query) {
    docker exec $container /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $saPassword -Q $query
    if ($LASTEXITCODE -ne 0) { throw "SQL failed" }
}

function Restore-Db([string]$dbName, [string]$bakFile, [string]$logicalData, [string]$logicalLog) {
    Write-Host "Restoring $dbName ..." -ForegroundColor Cyan
    Invoke-Sql @"
IF DB_ID(N'$dbName') IS NOT NULL
BEGIN
  ALTER DATABASE [$dbName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
  DROP DATABASE [$dbName];
END
RESTORE DATABASE [$dbName]
FROM DISK = N'$bakDir/$bakFile'
WITH MOVE N'$logicalData' TO N'$dataDir/${dbName}.mdf',
     MOVE N'$logicalLog' TO N'$dataDir/${dbName}_log.ldf',
     REPLACE;
"@
    Write-Host "[OK] $dbName" -ForegroundColor Green
}

Restore-Db "Db_Membership" "Db_Membership.bak" "Db_Membership_Data" "Db_Membership_Log"
Restore-Db "Project_Game34" "Game34.bak" "Db_Tank" "Db_Tank_log"
Restore-Db "Project_Player34" "Player34.bak" "DDTMixTankAc" "DDTMixTankAc_log"

Write-Host "Apply admin/max40 seed (gear + bags + cards + pets)..." -ForegroundColor Cyan
Get-Content "$root\scripts\seed-max-users.sql" -Raw | docker exec -i $container /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $saPassword
Get-Content "$root\scripts\seed-bags-cards-pets.sql" -Raw | docker exec -i $container /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $saPassword
Get-Content "$root\scripts\fix-stats-and-safe-gear.sql" -Raw | docker exec -i $container /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $saPassword
Write-Host ""
Write-Host "Done." -ForegroundColor Green
Write-Host "Accounts: admin (Lv60 max bags), max40 (Lv40 limited) — any password" -ForegroundColor Yellow
Write-Host "After seed: logout/login lại nhân vật để túi hiện đồ." -ForegroundColor Yellow
Write-Host "Next: .\Sync-GameAssets.bat" -ForegroundColor Cyan
Write-Host "Then:  .\scripts\start-gunny-stack.ps1" -ForegroundColor Cyan
