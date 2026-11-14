# Database

## 1. Kết nối

- Container: `gunny-sql` (azure-sql-edge), port 1433, `sa` / `Gunny@2026Mac`.
- sqlcmd trong container: **`/opt/mssql-tools/bin/sqlcmd`** (không phải `mssql-tools18`).
- Compat level cũ: **không có** `STRING_SPLIT` / `STRING_AGG` → dùng `VALUES (...)` hoặc ghép chuỗi ở script ngoài.

```powershell
docker exec gunny-sql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "Gunny@2026Mac" -W -s "|" -Q "SELECT 1"
```

| Database | Nội dung |
|---|---|
| `Project_Game34` | Template: `Shop_Goods` (item), `Ball`, `BallConfig`, map, mission, NPC, drop… |
| `Project_Player34` | Người chơi: `Sys_Users_Detail`, `Sys_Users_Goods` (túi đồ), `Sys_Users_Pet`, `Sys_Users_Card`… |
| `Db_Membership` | Membership legacy |

## 2. Bảng hay dùng

| Bảng | Khóa / cột chính | Ghi chú |
|---|---|---|
| `Project_Game34.dbo.Shop_Goods` | `TemplateID`, `CategoryID`, `Name`, `Quality`, `NeedSex`, `NeedLevel`, `Attack/Defence/Agility/Luck`, `Property1..8`, `Hole` | Template item. `NeedSex`: 0 = chung, 1 = nam, 2 = nữ |
| `Project_Game34.dbo.Ball` | `ID`, `BombPartical`, `Crater`, physics… | Định nghĩa đạn |
| `Project_Game34.dbo.BallConfig` | `TemplateID`, `Common`, `CommonAddWound`, `CommonMultiBall`, `Special` | Vũ khí → 4 loại đạn |
| `Project_Player34.dbo.Sys_Users_Detail` | `UserID`, `NickName`, `Sex` (1 = nam), `Grade`, `Money`, `Gold`… | Admin = UserID 1 |
| `Project_Player34.dbo.Sys_Users_Goods` | `ItemID` (identity), `UserID`, `BagType`, `Place`, `TemplateID`… | Túi đồ |

## 3. Item category (`Shop_Goods.CategoryID`)

| ID | Loại | ID | Loại |
|---|---|---|---|
| 1 | Nón | 13 | Bộ đồ (suit) |
| 2 | Kính | 14 | Dây chuyền |
| 3 | Tóc | 15 | Cánh |
| 4 | Mặt | 16 | Bong bóng chat |
| 5 | Áo | 17 | Trợ thủ (offhand, phím R) |
| 6 | Mắt | 27 | Item đặc biệt (không khảm) |
| 7 | Vũ khí | 28 | Vòng tay đặc biệt (+5/+7) |
| 8 | Vòng tay | 29 | Nhẫn đặc biệt (+5/+7) |
| 9 | Nhẫn | 31 | Thiên sứ ban phúc (biến thể trợ thủ) |
| 50/51/52 | **Trang bị thú cưng** (vũ khí/nón/giáp pet) — không mặc cho nhân vật | | |

## 4. Túi trang bị (`BagType = 0`)

Server: `PlayerEquipInventory(…, capacity 127, beginSlot 31)`. Client: `BagEquipListView(…, 31, 127)`, trang 1 = ô 31–79, trang 2 = ô 80–126.

| Place | Ô đang mặc | Place | Ô đang mặc |
|---|---|---|---|
| 0 | Nón | 9, 10 | Nhẫn |
| 1 | Kính | 11 | Bộ đồ |
| 2 | Tóc | 12 | Dây chuyền |
| 4 | Áo | 13 | Cánh |
| 5 | Mắt | 15 | Trợ thủ |
| 6 | Vũ khí | 31–126 | **Ô túi** |
| 7, 8 | Vòng tay | | |

### Quy ước “full” (giống đồ Admin)

`StrengthenLevel=15`, `AttackCompose=DefendCompose=AgilityCompose=LuckCompose=50`, `Hole1..4=0`, `Hole5=Hole6=-1`, `Hole5Level=Hole6Level=5`, `IsBinds=1`, `IsUsed=1`, `ValidDate=0` (vĩnh viễn), `IsJudge=1`, `IsExist=1`, `Count=1`, `Blood=1000`, `latentEnergyCurStr/NewStr='0,0,0,0'`.

## 5. Phát đồ cho người chơi — checklist an toàn

1. **TemplateID phải có trong client** `Tank.Request/TemplateAlllist.xml` (zlib). Thiếu → client null `Hole` khi vào sảnh → **màn hình vàng** (đã xảy ra với tóc 3580, mắt 6540).
2. Vũ khí (cat 7) phải có dòng `BallConfig` và đạn có asset (xem `WEAPONS-AND-BALLS.md`). Thiếu → treo màn loading vào trận.
3. Đúng giới tính (`NeedSex` 0 hoặc trùng `Sex`), `NeedLevel` ≤ cấp.
4. **Stop Road** trước khi INSERT/UPDATE/DELETE `Sys_Users_*`, start lại sau.
5. Chỉ dùng `Place` trống trong 31–126; kiểm tra không trùng place.
6. Backup bảng trước (`SELECT * INTO Sys_Users_Goods_bak_YYYYMMDD FROM Sys_Users_Goods`).

Kiểm tra TemplateID có trong client (PowerShell):
```powershell
$b=[IO.File]::ReadAllBytes("Tank.Request\TemplateAlllist.xml")
$ms=New-Object IO.MemoryStream(,$b[2..($b.Length-1)])
$x=(New-Object IO.StreamReader((New-Object IO.Compression.DeflateStream($ms,[IO.Compression.CompressionMode]::Decompress)))).ReadToEnd()
$x -match 'TemplateID="72244"'
```

## 6. Seed tài khoản

`scripts/restore-databases.ps1` restore `.bak` rồi chạy theo thứ tự:

1. `seed-max-users.sql` — admin Lv60 / max40, đồ mặc (**xóa sạch goods UserID 1**).
2. `seed-bags-cards-pets.sql` — túi trang bị, đạo cụ, thẻ, pet (xóa rồi tạo lại).
3. `fix-stats-and-safe-gear.sql` — đồ mặc an toàn, stat hiển thị.
4. `seed-admin-legend-bag.sql` — thêm 48 món LEGEND/full cho Admin (idempotent, chỉ lấp ô trống).

`Restore-Database.bat` / `.sh` chỉ restore `.bak`, **không chạy seed** — dữ liệu y như lúc export.

## 7. Export `.bak` để máy khác giống hệt

```powershell
docker exec -u root gunny-sql mkdir -p /var/opt/mssql/export
docker exec -u root gunny-sql chown mssql /var/opt/mssql/export
foreach ($pair in @(@("Project_Game34","Game34.bak"),@("Project_Player34","Player34.bak"),@("Db_Membership","Db_Membership.bak"))) {
  docker exec gunny-sql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "Gunny@2026Mac" `
    -Q "BACKUP DATABASE [$($pair[0])] TO DISK=N'/var/opt/mssql/export/$($pair[1])' WITH COPY_ONLY, INIT, FORMAT, COMPRESSION"
  docker cp "gunny-sql:/var/opt/mssql/export/$($pair[1])" "Database\$($pair[1])"
}
git add -f Database/*.bak     # *.bak bị .gitignore, phải -f
```
Logical file name (script restore cần): Game34 = `Db_Tank`/`Db_Tank_log`, Player34 = `DDTMixTankAc`/`DDTMixTankAc_log`, Membership = `Db_Membership_Data`/`Db_Membership_Log`.

## 8. Bảng backup đang có trong DB

| Bảng | Tạo khi |
|---|---|
| `Project_Game34.dbo.BallConfig_bak_20261114` | Trước khi thêm 54 dòng BallConfig cho vũ khí thiếu |
| `Project_Game34.dbo.Ball_bak_20261114` | Trước khi thêm 20 đạn Băng Vương Côn |
| `Project_Player34.dbo.Sys_Users_Goods_bak_20261114b` | Trước khi thêm đồ LEGEND cho Admin |
