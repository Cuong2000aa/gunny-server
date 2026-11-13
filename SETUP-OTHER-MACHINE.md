# Setup máy khác (= máy Mac hiện tại)

Repo: https://github.com/Cuong2000aa/gunny-server

**Kiến trúc / flow chi tiết (cho AI đọc):** [`SOURCE-ARCHITECTURE.md`](./SOURCE-ARCHITECTURE.md)

## 1. Pull
```bat
git clone https://github.com/Cuong2000aa/gunny-server.git
cd gunny-server
git pull
```

## 2. SQL (Docker)
```bat
docker run -d --name gunny-sql -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Gunny@2026Mac" -p 1433:1433 mcr.microsoft.com/azure-sql-edge:latest
```
Đợi port 1433 mở, rồi:
```bat
Restore-Database.bat
```

## 3. Map binary (fore.map / dead.map / bomb)
```bat
Sync-GameAssets.bat
```

## 4. Build + chạy
Cần .NET 8 SDK. Build solution / các project Service, rồi chạy:
- Center.Service
- Fighting.Service
- Road.Service
- Tank.Request

(Mac: double-click `Start Gunny.command`)

## 5. Chơi
http://127.0.0.1:828/web/play.html

## Đã có trong git (mirror máy chính)
| Thứ | Path |
|---|---|
| DB mới | `Database/*.bak` |
| Binary map + bomb | `GameAssets/` |
| Ảnh map / item cache | `Resources/` |
| Flash client | `Source Flash/FlashSV1/` |
| Ruffle | `Tank.Flash/ruffle071/` |
| AI / mission scripts | `GameServerScripts/` |

Password SQL mặc định trong `App.config`: `Gunny@2026Mac`
