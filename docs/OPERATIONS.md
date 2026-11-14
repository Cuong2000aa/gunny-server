# Vận hành: start / stop / build / log / tunnel

## 1. Process và port

| Process | Project | Port | Working directory khi chạy |
|---|---|---|---|
| SQL | Docker `gunny-sql` (azure-sql-edge) | 1433 | — |
| Center | `Center.Service` | 9202 | `Center.Service/bin/Release/net8.0` |
| Fighting | `Fighting.Service` | 9208 | `Fighting.Service/bin/Release/net8.0` |
| Road | `Road.Service` | 9500 | `Road.Service/bin/Release/net8.0` |
| Request | `Tank.Request` | 828 | **`Tank.Request/`** (không phải bin — static file + XML đọc theo content root) |

Thứ tự start: **SQL → Center → Fighting → (đợi ~8s) → Road → Request**.
Road và Fighting cần `logconfig.xml` và thư mục `map/` trong cwd.

## 2. Start nhanh

```powershell
.\scripts\start-gunny-stack.ps1      # Windows: SQL + 4 service (dotnet run -c Release)
```
Mac: `Start Gunny.command` / `Stop Gunny.command` / `RESTART_GUNNY.command`.

Start thủ công từng service (Windows, sau khi đã build Release):

```powershell
$root = "<repo>"
Start-Process dotnet -ArgumentList "Center.Service.dll"   -WorkingDirectory "$root\Center.Service\bin\Release\net8.0"   -WindowStyle Minimized
Start-Process dotnet -ArgumentList "Fighting.Service.dll" -WorkingDirectory "$root\Fighting.Service\bin\Release\net8.0" -WindowStyle Minimized
Start-Sleep 8
Start-Process dotnet -ArgumentList "Road.Service.dll"     -WorkingDirectory "$root\Road.Service\bin\Release\net8.0"     -WindowStyle Minimized
Start-Process dotnet -ArgumentList "run -c Release --no-build" -WorkingDirectory "$root\Tank.Request" -WindowStyle Minimized
```

## 3. Stop

```powershell
foreach ($p in 828,9208,9500) {
  Get-NetTCPConnection -LocalPort $p -State Listen -EA SilentlyContinue |
    % { Stop-Process -Id $_.OwningProcess -Force }
}
```
Thêm `9202` nếu muốn tắt cả Center (thường không cần khi chỉ restart game).

Kiểm tra đang chạy:
```powershell
foreach ($p in 1433,9202,9208,9500,828) { "$p " + [bool](Get-NetTCPConnection -LocalPort $p -State Listen -EA SilentlyContinue) }
```

## 4. Build

```powershell
dotnet build "Road.Service/Road.Service.csproj" -c Release
dotnet build "Fighting.Service/Fighting.Service.csproj" -c Release
dotnet build "Tank.Request/Tank.Request.csproj" -c Release
```
- **Phải stop service trước khi build**, nếu không lỗi khóa file DLL (`MSB3027` / file in use).
- `Game.Logic`, `Game.Server`, `Bussiness`, `SqlDataProvider` là thư viện, build kèm theo các Service.
- Solution: `DDTank 3.0.sln` (server), `Request.sln` (Tank.Request).

## 5. Log

| Log | Vị trí | Có gì |
|---|---|---|
| Road | `Road.Service/bin/Release/net8.0/logs/GameServer.log`, `Error.log`, `FlashError.log` | Packet, login, room, `[MatchTrace]` |
| Fighting | `Fighting.Service/bin/Release/net8.0/logs/` | Trận đấu, `Road→Fight LOAD DROP` |
| Client (browser/mobile) | `Tank.Request/logs/client.log` | Console + Ruffle AVM2 error từ thiết bị (xem `WEB-CLIENT.md` §4) |

Road `logconfig.xml` đã tắt console appender (cửa sổ minimized đầy buffer sẽ **block thread MAIN**) và bật `immediateFlush`.

Tag log hữu ích khi debug vào trận:
- `[MatchTrace] Client→Road LOAD nick=… p1=…` — client gửi tiến độ loading (`GameDataHandler`).
- `[MatchTrace] StartLoading gameId=… players=[…]` — Fighting bắt đầu loading (`PVPGame`).
- `Road→Fight LOAD DROP game not found / player not found` — packet loading bị rơi (`Fighting.Server/ServerClient.cs`).

## 6. Cloudflare tunnel (chơi từ xa / mobile)

```powershell
.\cloudflare\start-quick-tunnel.ps1     # URL https://xxxx.trycloudflare.com, đổi mỗi lần chạy
```
Mở `https://xxxx.trycloudflare.com/web/play.html`. Chỉ expose port 828. Domain cố định: `cloudflare/README.md`.

`Tank.Request` tự rewrite `config.xml` theo public origin (`X-Forwarded-*`), nên không cần sửa URL trong config.

## 7. Cache client

Client Flash cache module theo `<version from="1" to="N">` trong `Source Flash/FlashSV1/config.xml` (URL thêm `?lv=N`).
Sau khi đổi SWF/XML phía client: **tăng N** (hiện tại 31) rồi reload trang.

`play.html`, `ruffle071/*`, `2.png`, `Loading.swf`, `config.xml` được trả header `no-cache`.
