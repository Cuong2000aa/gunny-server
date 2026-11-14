# Vũ khí, đạn, trợ thủ và hiệu ứng

## 1. Luồng dữ liệu

```
Shop_Goods (vũ khí, TemplateID)
   └─ BallConfig(TemplateID) → Common / CommonAddWound / CommonMultiBall / Special  (ID đạn)
         └─ Ball(ID) → BombPartical (hiệu ứng nổ), Crater (hố), physics
```

| Nơi | Đọc từ | Dùng để |
|---|---|---|
| Server (Fighting/Road) | DB `BallConfig`, `Ball` (`BallConfigMgr`, `BallMgr`) | Tính đạn, sát thương, đào |
| Client | `Tank.Request/bombconfig.xml` (vũ khí → ball IDs) qua `WeaponBallManager` | Preload SWF đạn khi vào trận |
| Client | `Tank.Request/BallList.xml` (định nghĩa ball) | Hiệu ứng, crater |

Cả hai XML client là **zlib** (header `0x78`). `bombconfig.xml` sinh lại từ DB bằng `GET http://127.0.0.1:828/bombconfig.ashx`. `BallList.xml` hiện được **sửa tay** (1606 item) — khi thêm ball vào DB phải thêm vào XML.

Giải nén nhanh (PowerShell): đọc bytes, bỏ 2 byte đầu, `DeflateStream`. Nén lại: zlib header `78 9C` + deflate + adler32 big-endian.

> **Bắt buộc:** mọi vũ khí người chơi có thể cầm phải có dòng `BallConfig`. Thiếu → client `LoadBombManager.loadBomb` NPE trong constructor `RoomLoadingView` → **không bao giờ gửi LOAD** → cả phòng treo màn loading vàng.

## 2. Đường dẫn asset (tên bị đảo trong client)

| Thứ | Path | Class trong SWF |
|---|---|---|
| Đạn đang bay | `image/bomb/blastOut/blastOut{ballID}.swf` | `bullet{ID}` |
| Hiệu ứng nổ | `image/bomb/bullet/bullet{BombPartical}.swf` | `blastOutMovie{N}` |
| Hố đào | `image/bomb/crater/{Crater}/crater.png` (+ `craterbrink.png`) | — |
| Đạn trợ thủ / đặc biệt | `bombs/{id}.swf` | `tank.resource.bombs.Bomb{id}` |

CDN `https://gunny.vcdn.vn` **phân biệt hoa thường** và chỉ có bản lowercase (`blastout/blastout{id}.swf`). `Tank.Request/Program.cs` → `GetAssetPathCandidates` thêm candidate lowercase cho path `image/bomb/`. Crater đã được `DigAssetSanitizer` xử lý cố ý — **không** ghi đè bằng bản CDN.

## 3. Trợ thủ (CategoryID 17, phím R)

- Client: ballId = `Property1` của template.
- Server: `BallConfig(TemplateID)`, fallback `Property1`. `Property3 == 31` → **khiên** (không bắn).
- Client luôn preload SpecialBomb `[1, 3, 4, 64, 59, 97, 98]`. Ball khác phải gửi trong loading files.
  → `BaseGame.AddDeputyWeaponBombs()` (gọi trong `PVPGame.StartLoading`) thêm `AddLoadingFile(1, "bombs/{id}.swf", "tank.resource.bombs.Bomb{id}")` cho ball của trợ thủ mọi người trong trận.
- Logic bắn: `Player.UseSecondWeapon()` + `Game.Logic/Cmd/SecondWeaponCommand.cs` (xem `SOURCE-ARCHITECTURE.md` §6.3).

Lệch đã biết (hình hiển thị theo ball server, đều có asset): 17002 VIP Thiên sứ (client 64 / server 59), 17018/17019 Hoa Hồng Hắc Ám (298 / 11409).

## 4. Trạng thái data đạn (11/2026)

- 54 vũ khí từng thiếu `BallConfig` đã được thêm:
  - Họ Băng Vương Côn 7142x → đạn thật 11225–11234 / 112251–112292 (physics clone từ họ 7140, BombPartical 1039/1040, Crater 830/831).
  - Vũ khí có `Property1/2` là ball hợp lệ → dùng ball đó.
  - Còn lại → mặc định `20/20/20/21`.
- CDN: 2033/2053 asset có. Thiếu: blastout 0, 46–49, 545, 555, 5451, 5452, 11259–11262, 11415, 11427, 119034; bullet 298, 1055, 1056, 119034. Chỉ ảnh hưởng vũ khí 10082 Khỉ Trộm Đào và 10083 Vật tổ (không ai sở hữu).
- Vũ khí đang mượn đạn mặc định vì CDN không có art: Xích Chùy, Spuppet, Bia Hơi, Vua Ếch, Pháo Hoa…

## 5. Thêm / tự tạo vũ khí mới

1. Chọn art có sẵn: đạn (`blastOut{id}`), nổ (`bullet{n}`), crater — kiểm tra tồn tại qua `http://127.0.0.1:828/image/bomb/...`.
2. Thêm `Ball` (clone dòng gần giống, đổi `ID`, `BombPartical`, `Crater`, physics) vào DB **và** `BallList.xml`.
3. Thêm `Shop_Goods` (CategoryID 7, `Pic` = thư mục hình vũ khí trong `image/arm/...`) và `BallConfig`.
4. Template mới phải có trong `TemplateAlllist.xml` (regenerate qua handler tương ứng hoặc sửa XML) — nếu không client crash.
5. Regenerate `bombconfig.xml` (`/bombconfig.ashx`), tăng `version` trong `config.xml`, restart Road/Fighting.
6. Test: cầm vũ khí vào trận PVP, xem `client.log` không có `Error #1009` và `[MatchTrace] Client→Road LOAD` lên 100.

## 6. Kiểm tra asset đạn cho một danh sách vũ khí

```powershell
$w = "72244,71424"   # TemplateID
$rows = docker exec gunny-sql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "Gunny@2026Mac" -W -s "," -h -1 -Q `
  "SET NOCOUNT ON; SELECT c.TemplateID,b.ID,b.BombPartical,b.Crater FROM Project_Game34.dbo.BallConfig c JOIN Project_Game34.dbo.Ball b ON b.ID IN (c.Common,c.CommonAddWound,c.CommonMultiBall,c.Special) WHERE c.TemplateID IN ($w)"
# với mỗi dòng: GET /image/bomb/blastOut/blastOut{ID}.swf, /image/bomb/bullet/bullet{BombPartical}.swf, /image/bomb/crater/{Crater}/crater.png
```
