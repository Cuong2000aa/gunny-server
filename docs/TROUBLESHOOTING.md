# Troubleshooting

Bước đầu với mọi lỗi client: đọc `Tank.Request/logs/client.log` (phiên mới nhất, lọc `ERROR|Error #|TypeError`) — xem `WEB-CLIENT.md` §4. Sau đó Road `logs/Error.log` + `GameServer.log`.

## 1. Bảng tra nhanh

| Triệu chứng | Nguyên nhân hay gặp | Fix |
|---|---|---|
| Treo 13/13 khi login | Renderer không phải `wgpu-webgl`; thiếu `socketProxy` trên `player.load`; SQL chết; nhiều WS | `SOURCE-ARCHITECTURE.md` §4–5, §9 |
| **Vào game là màn hình vàng** (sau login, trước sảnh) | Túi đồ có TemplateID **không có trong `TemplateAlllist.xml`** → `StoreModel.isProperTo_CanEmbedEquipList` null `Hole` | Xóa item đó (stop Road trước) hoặc thêm template vào XML client |
| **Treo màn loading vàng khi vào trận** | Vũ khí của ai đó trong phòng thiếu `BallConfig` → `LoadBombManager.loadBomb` NPE → client không gửi LOAD | Thêm `BallConfig`, regenerate `bombconfig.xml`, tăng version |
| Đạn bay không có hình | Asset 404 do CDN phân biệt hoa thường | Đã fix: candidate lowercase trong `GetAssetPathCandidates` |
| Trợ thủ bắn không hiện hiệu ứng | Ball trợ thủ không nằm trong SpecialBomb preload | Đã fix: `BaseGame.AddDeputyWeaponBombs()` |
| Bàn phím Android tự bật khi vào phòng | Flash focus ô chat | Đã fix: `setupMobileKeyboardGuard` + nút ⌨ |
| Sửa DB xong mất / không thấy | Road đang chạy ghi đè từ RAM; hoặc chưa logout | Stop Road → sửa → start; nhân vật thoát ra vào lại |
| Build lỗi file locked | Service đang chạy | Stop service rồi build |
| `Map's file not exist` | Chưa sync `GameAssets` | `Sync-GameAssets.bat` |
| Đào ra oval vàng | Thiếu Python + Pillow cho `DigAssetSanitizer` | `pip install pillow` |

## 2. Sự cố đã xử lý (post-mortem ngắn)

### 2.1 Treo loading vào trận trên mobile (11/2026)
- **Triệu chứng:** cả phòng kẹt màn loading vàng, server không nhận LOAD từ client.
- **Điều tra:** thêm trace `[MatchTrace]` ở Road (`GameDataHandler.TraceLoad`) và Fighting (`ServerClient` LOAD DROP) + remote console → client không hề gửi LOAD; Ruffle log NPE trong `RoomLoadingView`.
- **Nguyên nhân gốc:** vũ khí 71424 (Băng Vương Côn) không có `BallConfig` → client không tìm được bomb list.
- **Fix:** thêm `BallConfig` cho 54 vũ khí thiếu, thêm 20 `Ball` thật cho họ 7142x, regenerate `bombconfig.xml`, cập nhật `BallList.xml`. Gỡ bỏ các workaround không cần (strip pet, ForceStuckLoadingAction, bot LOAD100, patch SWF `checkProgress`).
- **Bài học:** tìm đúng tầng (client có gửi không?) trước khi vá server.

### 2.2 Màn hình vàng khi vào game sau khi phát đồ (11/2026)
- **Triệu chứng:** login xong kẹt màn vàng.
- **Log:** `TypeError #1009 at store.data::StoreModel/isProperTo_CanEmbedEquipList() … HallStateView/enter()`.
- **Nguyên nhân:** INSERT thẳng DB tóc 3580 và mắt 6540 — có trong `Shop_Goods` server nhưng không có trong `TemplateAlllist.xml` client → template null → `param1.Hole.split` null.
- **Fix:** xóa 2 item. Script `seed-admin-legend-bag.sql` chỉ chứa TemplateID đã kiểm tra có trong client.

### 2.3 Bàn phím ảo tự bật trên RG557
- Thử 1 (chặn focus trong 400ms sau chạm) thất bại vì Flash focus sau ~6ms và target nằm trong shadow DOM.
- Fix: `inputmode="none"` cho input trong shadowRoot + nút ⌨ mở thủ công.

## 3. Debug vào trận — checklist

1. `client.log`: có `Error #1009` trong `RoomLoadingView` / `LoadBombManager`? → data đạn.
2. Road `GameServer.log`: có `[MatchTrace] Client→Road LOAD … p1=100`? Không có → client chưa gửi.
3. Fighting log: `LOAD DROP game not found` → game đã bị hủy / sai id.
4. `[MatchTrace] StartLoading … players=[…]` liệt kê đủ người chưa.
