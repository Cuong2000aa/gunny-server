# Chạy named tunnel đã setup (cloudflare/config.yml)
$ErrorActionPreference = "Stop"
$configPath = Join-Path $PSScriptRoot "config.yml"
if (-not (Test-Path $configPath)) {
    Write-Error "Chưa có config.yml. Chạy trước: .\setup-named-tunnel.ps1 -Hostname gunny.yourdomain.com"
}

Write-Host "Starting named Cloudflare Tunnel..." -ForegroundColor Cyan
& cloudflared tunnel --config $configPath run
