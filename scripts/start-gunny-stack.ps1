# Start Docker SQL (neu chua) + 4 service Gunny.
# Quan trong: moi service chay dung WorkingDirectory (can logconfig.xml + map/).
$ErrorActionPreference = "Continue"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root
New-Item -ItemType Directory -Force -Path (Join-Path $root "logs") | Out-Null

Write-Host "== SQL container ==" -ForegroundColor Cyan
$sql = docker ps -a --filter "name=gunny-sql" --format "{{.Names}} {{.Status}}" 2>$null
if (-not $sql) {
    docker run -d --name gunny-sql `
      -e "ACCEPT_EULA=Y" `
      -e "MSSQL_SA_PASSWORD=Gunny@2026Mac" `
      -p 1433:1433 `
      mcr.microsoft.com/azure-sql-edge:latest
} elseif ($sql -notmatch "Up") {
    docker start gunny-sql | Out-Null
}
Write-Host $sql

function Test-Port([int]$port) {
    return [bool](Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue)
}

function Start-GunnyService([string]$name, [string]$dir, [string]$csproj, [int]$port) {
    if (Test-Port $port) {
        Write-Host "$name already listening on $port" -ForegroundColor Yellow
        return
    }
    $wd = Join-Path $root $dir
    Write-Host "Starting $name (cwd=$dir) ..." -ForegroundColor Cyan
    # Khong redirect stdin — console app se thoat neu EOF (Road/Fighting/Center).
    Start-Process -FilePath "dotnet" `
        -ArgumentList @("run", "--project", $csproj, "-c", "Release") `
        -WorkingDirectory $wd `
        -WindowStyle Minimized
}

# Build nhanh neu thieu binary
foreach ($proj in @(
    "Center.Service/Center.Service.csproj",
    "Fighting.Service/Fighting.Service.csproj",
    "Road.Service/Road.Service.csproj",
    "Tank.Request/Tank.Request.csproj"
)) {
    if (-not (Test-Path (Join-Path $root ($proj -replace '\.csproj$', '/bin/Release/net8.0')))) {
        Write-Host "Building $proj ..." -ForegroundColor DarkCyan
        dotnet build (Join-Path $root $proj) -c Release | Out-Null
    }
}

Start-GunnyService "Center.Service" "Center.Service" "Center.Service.csproj" 9202
Start-Sleep -Seconds 3
Start-GunnyService "Fighting.Service" "Fighting.Service" "Fighting.Service.csproj" 9208
Start-Sleep -Seconds 3
Start-GunnyService "Road.Service" "Road.Service" "Road.Service.csproj" 9500
Start-Sleep -Seconds 3
Start-GunnyService "Tank.Request" "Tank.Request" "Tank.Request.csproj" 828

Write-Host ""
Write-Host "Doi port len..." -ForegroundColor Cyan
for ($i = 1; $i -le 40; $i++) {
    $ports = @(828, 9500, 9202, 9208) | Where-Object { Test-Port $_ }
    Write-Host ("  try {0}: {1}" -f $i, ($ports -join ","))
    if (@($ports).Count -ge 4) { break }
    if ((Test-Port 828) -and $i -ge 15) { break }
    Start-Sleep -Seconds 2
}

$lan = Get-NetIPAddress -AddressFamily IPv4 |
    Where-Object { $_.InterfaceAlias -match 'Wi-Fi|Ethernet' -and $_.IPAddress -notlike '169.*' -and $_.PrefixOrigin -ne 'WellKnown' } |
    Select-Object -First 1 -ExpandProperty IPAddress

Write-Host ""
Write-Host "Local play:  http://127.0.0.1:828/web/play.html" -ForegroundColor Green
if ($lan) { Write-Host "LAN play:    http://${lan}:828/web/play.html" -ForegroundColor Green }
Write-Host "Tunnel:      .\cloudflare\start-quick-tunnel.ps1" -ForegroundColor Cyan
Write-Host "Named:       .\cloudflare\setup-named-tunnel.ps1 -Hostname gunny.YOURDOMAIN.com" -ForegroundColor Cyan
if (-not (Test-Path (Join-Path $root "Road.Service\map"))) {
    Write-Host "CANH BAO: thieu thu muc Road.Service\\map (fore.map/dead.map) — Road/Fighting co the khong start duoc." -ForegroundColor Yellow
}
