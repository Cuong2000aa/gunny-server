# Expose Tank.Request (:828) ra internet qua Cloudflare Quick Tunnel (URL tạm *.trycloudflare.com)
# Yêu cầu: Docker SQL + 4 service đang chạy local.
$ErrorActionPreference = "Stop"

$cloudflared = Get-Command cloudflared -ErrorAction SilentlyContinue
if (-not $cloudflared) {
    Write-Error "cloudflared chưa có trong PATH. Cài: winget install Cloudflare.cloudflared"
}

Write-Host "Starting Cloudflare Quick Tunnel -> http://127.0.0.1:828" -ForegroundColor Cyan
Write-Host "Giữ cửa sổ này mở. URL https://....trycloudflare.com sẽ hiện bên dưới." -ForegroundColor Yellow
Write-Host "Play: https://<URL>/web/play.html" -ForegroundColor Yellow
Write-Host ""

& cloudflared tunnel --url http://127.0.0.1:828 --protocol http2
