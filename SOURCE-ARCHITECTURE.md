# Gunny II Private Server — Source Architecture (AI / Dev Reference)

> Doc này mô tả **cách source thực sự hoạt động** trên repo `gunny-server`.  
> Mục tiêu: AI hoặc dev máy khác đọc xong là hiểu flow, port, path, patch quan trọng — **không đoán bừa**.  
> Setup máy mới: xem thêm `SETUP-OTHER-MACHINE.md`.

**Repo:** https://github.com/Cuong2000aa/gunny-server  
**Stack:** .NET 8 + Azure SQL Edge (Docker) + Flash client qua **Ruffle** (WASM) trên browser.

---

## 1. Big picture

```
Browser (Ruffle)
   │  HTTP  :828   →  Tank.Request  (ashx, static SWF/image, play.html)
   │  WS    :828/ws?port=9569 → TCP tunnel → Road :9500
   │
Road.Service :9500          ← game hall / inventory / rooms / PVE enter
   ├─ TCP → Center.Service :9202   (login / zone / account glue)
   └─ TCP → Fighting.Service :9208 (battle simulation)
SQL Server :1433
   ├─ Project_Player34   (accounts / characters)  ← App.config key conString
   ├─ Project_Game34     (templates / maps / missions) ← crosszoneString
   └─ Db_Membership      (membership leftovers)
```

| Process | Project | Port | Vai trò |
|---|---|---|---|
| Request | `Tank.Request` | **828** | Web + HTTP API (.ashx) + static Flash/Resources + **WebSocket proxy** |
| Road | `Road.Service` | **9500** | Game server chính (player online, hall, packets) |
| Center | `Center.Service` | **9202** | Login/center service (Road kết nối vào) |
| Fighting | `Fighting.Service` | **9208** | Fight/battle server |
| SQL | Docker `gunny-sql` | **1433** | DB |

**Thứ tự start chuẩn:** SQL → Center → Fighting → Road → Request.  
Mac: `Start Gunny.command`. Stop: `Stop Gunny.command`.

---

## 2. Directory map (những path AI phải nhớ)

| Path | Là gì | Git? |
|---|---|---|
| `Tank.Request/` | ASP.NET Core host port 828, handlers `.ashx`, asset proxy | Có |
| `Tank.Flash/` | `play.html`, `config.xml`, **Ruffle 0.7.1** (`ruffle071/`) | Có |
| `Source Flash/FlashSV1/` | SWF client thật: `Loading.swf`, `DDT_Loading.swf`, **`2.png`** (core AS3) | Có |
| `Resources/` | Cache ảnh/sound local (`image/`, `sound/`) — server đọc trước CDN | Có (~88MB+) |
| `GameAssets/map`, `GameAssets/bomb` | Binary map (`fore.map`/`dead.map`) + bomb collision | Có (~144MB) |
| `Road.Service/map` → symlink/`Sync` → `GameAssets/map` | Road cwd load `map/<id>/fore.map` | Symlink + sync script |
| `Fighting.Service/map` | Giống Road | Sync cùng script |
| `Game.Server/` | Logic game server (handlers, WorldMgr, login packet…) | Có |
| `Game.Logic/` | PVE/PVP logic, `MapMgr`, AI hooks | Có |
| `GameServerScripts/` | Script AI / mission C# (compile hoặc prebuilt DLL) | Có |
| `Database/*.bak` | Backup SQL để restore máy khác | Có |
| `Bussiness/`, `SqlDataProvider/` | Data access / center interface | Có |
| `bin/`, `obj/` | Build output | **Ignore** — không commit |

### Static URL mapping (Tank.Request)

Served từ máy host:

| URL | Physical |
|---|---|
| `/web/play.html` | `Tank.Flash/play.html` |
| `/web/ruffle071/*` | `Tank.Flash/ruffle071/*` |
| `/flash/*` | `Source Flash/FlashSV1/*` (gồm `2.png`, `Loading.swf`) |
| `/image/*`, `/sound/*` | `Resources/image|sound` rồi fallback CDN |
| `/*.ashx` | Handlers trong `Tank.Request` |

Play URL chuẩn: `http://127.0.0.1:828/web/play.html`

---

## 3. Databases

| Catalog | Config key | Nội dung chính |
|---|---|---|
| `Project_Player34` | `conString` | User, character, bag, quest progress… |
| `Project_Game34` | `crosszoneString` | Item template, map info, mission, NPC, PVE… |
| `Db_Membership` | (membership) | Legacy membership |

**SA password mặc định (local):** `Gunny@2026Mac`  
Connection mẫu: `Data Source=127.0.0.1,1433;...;User ID=sa;Password=Gunny@2026Mac;TrustServerCertificate=True`

Restore máy mới:
- `Restore-Database.bat` / `Restore-Database.sh`
- File: `Database/Game34.bak`, `Player34.bak`, `Db_Membership.bak`

Map metadata (tên, ID) nằm trong **DB**; collision/visual binary nằm trong **GameAssets**; ảnh preview map nằm trong **Resources/image/map**.

---

## 4. Client boot sequence (Flash / Ruffle)

1. Browser load `play.html` → khởi tạo Ruffle với:
   - `preferredRenderer: "wgpu-webgl"` (**bắt buộc** — `canvas`/`webgl` thuần vỡ `BitmapData.draw` → treo 13/13)
   - `socketProxy`: map TCP Flash → `ws://<host>:828/ws?port=...`
2. Load `Loading.swf` → `DDT_Loading.swf` → core **`2.png`** (SWF đổi tên).
3. Loader tải XML/template qua HTTP `:828` (nhiều bước → UI hiện `13/13` / 100%).
4. HTTP login: `Login.ashx` / `LoginSelectList.ashx` (RSA có bypass `RAW:` trên private stack).
5. Client lấy server list → Flash **cộng Port +69** (9500 → **9569**).
6. Socket connect qua Ruffle proxy → Request `/ws` → TCP **Road 9500**.
7. Packet `LOGIN` → `UserLoginHandler` → `SendLoginSuccess` → client `StateType.MAIN` (hall).
8. Hall gọi `StartupResourceLoader.finishLoadingProgress()` → hết màn 13/13.

### Force login (private patch trong `2.png`)

`LoginStateView.forceLoginNow()` có thể bị gọi từ nhiều chỗ (Startup / DDT / enter).  
**Phải chỉ chạy 1 lần** (`forceLoginStarted`). Multi-call → multi WebSocket; Flash `ByteSocket` chỉ listen socket **mới nhất** → nếu server “first-wins” thì treo 13/13.

Server side complementary: `UserLoginHandler` dùng **newest-wins** (kick session cũ).

---

## 5. Socket / WebSocket proxy (cực kỳ quan trọng)

Flash gốc dùng `XMLSocket`/`Socket` TCP. Ruffle trên web **không** TCP thẳng ổn định → `play.html` cấu hình:

```
socketProxy: host 127.0.0.1 (hoặc LAN), port 9500|9569|… → ws://same-host:828/ws?port=…
```

`Tank.Request` `Map("/ws")`:
- Accept WebSocket
- Mở TCP tới `127.0.0.1:9500` (luôn tunnel Road; query `9569` cũng về 9500)
- Bridge binary 2 chiều

**Nếu thiếu `socketProxy` trên `player.load(...)`:** Ruffle cố TCP `:9569` → Road không nhận / bytes=0 → treo login.

---

## 6. Assets: Resources vs CDN vs dig crater

### 6.1 Pipeline `/image/*` và `/sound/*`

Trong `Tank.Request/Program.cs`:

1. Normalize path (đặc biệt casing `craterBrink` → `craterbrink`)
2. Tìm file trong `Resources/`
3. Gender cross `m/` ↔ `f/` nếu thiếu
4. Nếu không có → GET `https://gunny.vcdn.vn/<path>` (timeout ngắn) → **cache xuống Resources**
5. Vẫn fail → pixel trong suốt 1×1 (trừ crater — xem dưới)

CDN **không browse được** folder (403). Chỉ file path trực tiếp mới 200.

Offline / máy khác: càng nhiều file trong `Resources/` càng ít phụ thuộc CDN. Player remote vẫn cần mạng tới **host game**, không phải CDN.

### 6.2 Hiệu ứng đào (dig / crater) — CHI TIẾT (hay lỗi khi sang máy khác)

Dig **không** chỉ là “có file ảnh”. Client (Ruffle) đục địa hình bằng `BitmapData.draw(..., BlendMode.ERASE)` + vẽ viền `craterbrink`. Trên Ruffle cũ, ERASE dễ fail → client **tô nguyên oval vàng/cam** lên map (đó là “hiệu ứng đào cũ” user từng báo).

| Thành phần | Path / code | Việc |
|---|---|---|
| Ảnh lỗ | `Resources/image/bomb/crater/<id>/crater.png` | Mask ERASE (alpha) |
| Ảnh viền | `.../craterbrink.png` | Viền; **phải trong suốt phần giữa** nếu ERASE fail |
| Sanitize runtime | `Tank.Request/DigAssetSanitizer.cs` | Gọi **`python3` + Pillow**: crater → RGB trắng giữ alpha; brink → **full transparent** |
| Proxy / casing | `Tank.Request/Program.cs` | `craterBrink` → `craterbrink`; **cấm** trả pixel 1×1 cho crater; fallback `crater/4/` |
| Collision map | `GameAssets/map/<id>/dead.map` + client `dead.png` | **Không sanitize / không xóa** `dead.png` — đụng collision |
| Bomb server | `GameAssets/bomb/<id>.bomb` | Tile đào server-side (`BallMgr`); thiếu → log `can't find bomb file for ball …` |

**Máy khác bắt buộc có:**

1. `git pull` (đã có ~316 file crater trên git).
2. **Python + Pillow** trên PATH (`pip install pillow`). DigAssetSanitizer thử `python3` rồi `python`.
3. Thiếu Python/Pillow → sanitize fail → lại oval nâu/vàng trên Ruffle.

**Không làm:**
- Đừng “fix dig” bằng cách xóa/thay `map/*/dead.png` hoặc `dead.map`.
- Đừng commit chỉ `.bak` crater (bị `.gitignore` `*.bak`) rồi quên file `.png` đã sanitize.
- Đừng nghĩ CDN tự đủ: brink sai casing trên CDN từng 404 → brink trong suốt 1×1.

### 6.3 Vũ khí phụ / thiên sứ ban phúc (nút R / PHỤ)

Không phải asset “thiên sứ.swf” một file — là **logic đạn phụ**:

| File | Patch |
|---|---|
| `Game.Logic/Phy/Object/Player.cs` → `UseSecondWeapon()` | `Property3 == 31` → khiên/`AddGuardEquipEffect`; **else** → thiên sứ: `SetCurrentWeapon(DeputyWeapon)` + `OnPlayerCure()` |
| Cùng file → set ball khi đổi weapon | Nếu `BallConfigMgr.FindBall(templateId)` **null** → fallback `Template.Property1` làm `ballId` (client cũng vậy) |
| `Game.Logic/Cmd/SecondWeaponCommand.cs` | Packet 84: chỉ khi đang attack + còn `deputyWeaponCount`; gửi lại count về client |

**Phụ thuộc data (DB `Project_Game34`):** bảng Ball / BallConfig + item template Property1/3/4/5.  
→ Máy khác **phải restore `Database/Game34.bak` mới** (đã export cùng repo). Thiếu BallConfig + thiếu fallback cũ = phụ R “không nổ / không trừ lượt”.

**Client UI:** nút PHỤ(R) trên `play.html` / phím `R` — chỉ gửi input; logic thật ở Fighting/Road `SecondWeaponCommand`.

### 6.4 Binary map (Road/Fighting)

| | |
|---|---|
| Path runtime | `{Road|Fighting}.Service/map/<id>/fore.map` (+ `dead.map`) |
| Canonical git | `GameAssets/map`, `GameAssets/bomb` |
| Load code | `Game.Logic/MapMgr.cs` → `Path.Combine("map", id, "fore.map")` relative **cwd service** |
| Sync | `Sync-GameAssets.bat` / `.sh` (Win thường không giữ symlink Mac) |

Thiếu → log: `Map's file ... is not exist` → Road/Fighting không vào trận được.

---

## 7. Server-side game modules

| Module | Namespace / path | Việc |
|---|---|---|
| Packet handlers | `Game.Server/Packets/Client/*` | VD `UserLoginHandler` = ePackageType.LOGIN |
| World / players | `Game.Server/Managers/WorldMgr.cs` | AddPlayer newest-wins |
| Login queue | `Game.Server/Managers/LoginMgr.cs` | Mid-login client map |
| Quests | `Game.Server/Quests/QuestInventory.cs` | Snapshot ToList khi Restart (tránh CollectionModified) |
| Levels | `Game.Server/Managers/LevelMgr.cs` | MaxLevel fallback grade cao |
| PVE scripts | `GameServerScripts/AI/**` | Mission AI; Road load `GameServerScripts.dll` |
| Map collision | `Game.Logic/MapMgr.cs` | `fore.map` / `dead.map` |

Scripts: trên macOS `CodeDom` compile có thể fail → Road load **prebuilt** `GameServerScripts.dll` (copy từ `GameServerScripts/bin/out` nếu có).

---

## 8. Private-server patches — KHÔNG được regress

Khi AI sửa code, **giữ** các hành vi sau:

1. **Ruffle renderer = `wgpu-webgl`** trong `Tank.Flash/play.html` (global config **và** `player.load`).
2. **`socketProxy` trên `player.load`** — không chỉ global config.
3. **WS `/ws` luôn về Road 9500** khi client gửi 9569.
4. **`forceLoginNow` once-only** trong SWF `2.png` (`LoginStateView`).
5. **UserLoginHandler newest-wins** + plaintext RSA fallback (`rsaEncry5` / RAW login).
6. **Dig crater:** casing `craterbrink` + `DigAssetSanitizer` (python3+Pillow) + fallback `crater/4/` — **không** trả 1×1; **không** đụng `dead.png`/`dead.map`.
7. **Thiên sứ / phụ R:** giữ fallback `BallConfig → Property1` và nhánh `Property3==31` vs đạn phụ trong `Player.UseSecondWeapon` / `SecondWeaponCommand`.
8. **Không ffdec-replace `HallStateView` toàn class** nếu không cần — từng gây `VerifyError: Stack depth unbalanced` trên Ruffle.
9. **Alice / network inspector:** N/A repo này (rule ACFC khác workspace).

SWF core: `Source Flash/FlashSV1/2.png` — edit bằng FFDec (`-replace` / `-export script`). Backup trước khi patch.

---

## 9. Common failures → nguyên nhân

| Triệu chứng | Nguyên nhân thường gặp | Fix |
|---|---|---|
| Treo **13/13** 100% | Renderer canvas; dual WS; thiếu socketProxy; SQL chết | wgpu-webgl; once-login; newest-wins; bật Docker SQL |
| `Map's file not exist` | Chưa sync GameAssets | `Sync-GameAssets.bat` |
| LoginSelectList `total="0"` | SQL down / chưa restore | `docker start gunny-sql` + `Restore-Database.bat` |
| Road bytes=0 / không SendLoginSuccess | WS không tới 9500 | Kiểm tra play.html socketProxy + Request `/ws` |
| Skin trống | Thiếu Resources + không CDN | Có mạng lần đầu để cache, hoặc copy `Resources/` |
| VerifyError HallStateView | SWF recompile hỏng | Restore `2.png` từ backup sạch Hall |
| Đào ra **oval vàng/cam** | ERASE Ruffle fail + brink chưa sanitize / thiếu Python | Cài `python3`+Pillow; giữ DigAssetSanitizer; hard-refresh crater |
| Đào “thủng” nhưng nền vàng | Lỗ đục `fore` lộ màu `dead` layer | Đừng xóa dead; đó là layer đá `digable=false` |
| Phụ R / thiên sứ không ăn | BallConfig thiếu hoặc code fallback mất | Restore Game34.bak; giữ patch `Player.cs` / `SecondWeaponCommand` |
| Log `can't find bomb file` | Thiếu `GameAssets/bomb/<id>.bomb` | `Sync-GameAssets` + đủ bak/assets |

Logs hay dùng:
- Mac Request/Road stdout: `/tmp/tankrequest.log`, `/tmp/road.log` hoặc `/tmp/gunny-logs/`
- Road log4net: `Road.Service/bin/Debug/net8.0/logs/`

---

## 10. Build / run (tóm tắt)

```text
.NET 8 SDK
Docker Desktop + container gunny-sql (azure-sql-edge), SA=Gunny@2026Mac
Restore-Database.*
Sync-GameAssets.*
dotnet build các Service + Tank.Request
Start: Center :9202 → Fighting :9208 → Road :9500 → Request :828
Open: http://127.0.0.1:828/web/play.html
```

Solution files: `DDTank 3.0.sln`, `Request.sln`.

---

## 11. Rules for AI agents editing this repo

1. **Trước khi “fix treo login”:** kiểm tra SQL → Request log → Road `SendLoginSuccess` → console Ruffle (`BitmapData.draw` / VerifyError / WS).
2. **Đừng commit** `bin/`, `obj/`, `*.bak` tạm, symlink tuyệt đối kiểu `/Users/it/...`.
3. **Đừng xóa** `GameAssets/` hay `Resources/image/bomb/crater/` khi “dọn repo”.
4. Sửa Flash: patch tối thiểu class; verify bằng export lại script + test Ruffle.
5. Đổi DB trên máy dev → **export lại** `Database/*.bak` và push nếu muốn máy khác giống hệt.
6. Port Flash +69 là hành vi client gốc — proxy/server phải tương thích, **không** “sửa Port list về 9500 rồi quên proxy”.

---

## 12. Quick path cheatsheet

```
play.html          → Tank.Flash/play.html
Ruffle             → Tank.Flash/ruffle071/
Core SWF           → Source Flash/FlashSV1/2.png
HTTP login         → Tank.Request/Login.ashx.cs
WS proxy           → Tank.Request/Program.cs  Map("/ws")
Socket login       → Game.Server/Packets/Client/UserLoginHandler.cs
Hall enter         → hall.HallStateView (trong 2.png)
Map binary         → GameAssets/map/<id>/fore.map
Dig FX             → Resources/image/bomb/crater/<id>/ + DigAssetSanitizer (python+Pillow)
Angel / phụ R      → Player.UseSecondWeapon + SecondWeaponCommand + BallConfig/Property1
CDN                → https://gunny.vcdn.vn/<image|sound path>
```

---

*Last aligned: dig crater sanitizer + thiên sứ deputy-weapon patches documented; DigAssetSanitizer tries python3/python.*
