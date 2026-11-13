# Setup Cloudflare Named Tunnel (domain riêng, URL cố định)
# Cần: tài khoản Cloudflare + domain đã add vào Cloudflare DNS
param(
    [Parameter(Mandatory = $true)]
    [string]$Hostname,   # vd: gunny.yourdomain.com

    [string]$TunnelName = "gunny"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$configPath = Join-Path $PSScriptRoot "config.yml"

Write-Host "== Cloudflare login (mở browser) ==" -ForegroundColor Cyan
& cloudflared tunnel login

Write-Host "== Create tunnel '$TunnelName' (bỏ qua nếu đã có) ==" -ForegroundColor Cyan
$existing = & cloudflared tunnel list 2>$null | Select-String -Pattern $TunnelName
if (-not $existing) {
    & cloudflared tunnel create $TunnelName
} else {
    Write-Host "Tunnel '$TunnelName' đã tồn tại." -ForegroundColor Yellow
}

Write-Host "== Route DNS $Hostname -> tunnel ==" -ForegroundColor Cyan
& cloudflared tunnel route dns $TunnelName $Hostname

$tunnelList = & cloudflared tunnel list --output json | ConvertFrom-Json
$tunnel = $tunnelList | Where-Object { $_.name -eq $TunnelName } | Select-Object -First 1
if (-not $tunnel) {
    Write-Error "Không tìm thấy tunnel '$TunnelName' sau khi tạo."
}

$cred = Join-Path $env:USERPROFILE ".cloudflared\$($tunnel.id).json"
@"
tunnel: $($tunnel.id)
credentials-file: $cred

ingress:
  - hostname: $Hostname
    service: http://127.0.0.1:828
    originRequest:
      noTLSVerify: true
      connectTimeout: 30s
      keepAliveTimeout: 90s
  - service: http_status:404
"@ | Set-Content -Path $configPath -Encoding UTF8

Write-Host ""
Write-Host "Đã ghi $configPath" -ForegroundColor Green
Write-Host "Chạy tunnel:  cloudflared tunnel --config `"$configPath`" run $TunnelName" -ForegroundColor Green
Write-Host "Play URL:     https://$Hostname/web/play.html" -ForegroundColor Green
Write-Host "Repo root:    $root" -ForegroundColor DarkGray
