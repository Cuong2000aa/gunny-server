# Web client: `play.html`, Ruffle, mobile

File chuẩn: `Tank.Flash/play.html` (serve tại `/web/play.html`). `Tank.Request/play.html` và `Tank.Request/wwwroot/play.html` là **bản cũ hơn** (serve tại `/play.html`), chưa có fix bàn phím/Android/remote log — luôn dùng URL `/web/play.html`.

## 1. Ruffle

- Bản local: `Tank.Flash/ruffle071/` (Ruffle 0.7.1), load `/web/ruffle071/ruffle.js?v=…`.
- `preferredRenderer: "wgpu-webgl"` — **bắt buộc**, đặt ở cả global config và `player.load` (canvas/webgl vỡ `BitmapData.draw` → treo 13/13).
- `socketProxy` — **bắt buộc trên `player.load`**: map TCP Flash (9500/9569/…) → `ws(s)://<origin>/ws?port=…`. Server luôn tunnel về Road 9500.
- `logLevel: "info"` — để remote log thấy được lỗi AVM2.

## 2. Boot

`play.html` → `DDT_Loading.swf?user=…&key=…&config=<origin>/flash/config.xml` → tải XML template (13 bước) → login HTTP → socket Road → hall. Chi tiết: `SOURCE-ARCHITECTURE.md` §4.

## 3. Mobile

### Bàn phím ảo tự bật (Android, RG557)
Flash focus ô chat `EditText` ~6ms sau khi chạm vào phòng → Android bật bàn phím. Ô input ẩn nằm trong **shadow DOM** của `<ruffle-player>`.

`setupMobileKeyboardGuard()` (chỉ chạy trên mobile):
- Tìm input/textarea trong `shadowRoot` của `ruffle-player` / `ruffle-object` / `ruffle-embed`, mỗi 300ms đặt `inputmode="none"` (khóa) hoặc `"text"` (mở).
- `focusin` capture dùng `composedPath()[0]` (target thật trong shadow DOM); nếu chưa mở khóa → `inputmode=none` + blur/focus lại, log `[Keyboard] blocked auto keyboard`.
- Nút **⌨** cố định góc dưới trái (`#btn-keyboard`, `pointerdown` + `preventDefault`): bấm để mở bàn phím khi cần gõ chat, bấm lần nữa để khóa lại. Thứ tự đúng: blur → unlock → focus (blur trước, nếu không listener blur sẽ khóa lại ngay).

### Phím ảo
- Android: ẩn hoàn toàn `#virtual-gamepad` và nút `#btn-toggle-gamepad` (CSS `html.is-android …{display:none!important}` + `toggleGamepad()` return sớm). Tay cầm vật lý (Gamepad API polling) vẫn hoạt động.
- iOS / khác: nút "🎮 Phím Ảo" vẫn bật/tắt được.

## 4. Remote console (`/clientlog`)

Script đầu `play.html` bọc `console.*`, bắt `error`, `unhandledrejection`, `visibilitychange`, `pagehide`; gom và gửi mỗi 1s (fetch hoặc `sendBeacon`) tới `POST /clientlog` với `{sid, dev, lines[]}`.
`Tank.Request` ghi vào `Tank.Request/logs/client.log` dạng `[android i6tnh0] <line>`.

Đọc lỗi phiên mới nhất:
```powershell
$all = Get-Content Tank.Request\logs\client.log
$sid = ($all[-1] -split ' ')[1].TrimEnd(']')
$all | ? { $_ -like "*$sid]*" -and $_ -match 'ERROR|Error #|TypeError' }
```
Lỗi AVM2 có stack trace AS3 (vd `store.data::StoreModel/isProperTo_CanEmbedEquipList()`) — tra code ở `tools/export_2png/scripts` (xem `TOOLS-FLASH.md`).

Lỗi nền vô hại, có cả ở phiên bình thường: `TimesController/loadThumbnail` #1009, `AMF3 deserializer … ObjectId(1)`, `mpa: invalid main_data_begin`, `Encountered stub`.

## 5. PWA

`Tank.Flash/manifest.webmanifest` + `pwa/icon-*.png` + `sw.js` (scope `/web/`). Service worker **không cache** gì (chỉ để Chrome cho "Cài đặt ứng dụng"), tránh kẹt bản SWF cũ.

## 6. Giao diện

Tỉ lệ 10:6 (1000×600), preset 100/125/150%/fullscreen, lưu `localStorage`.
