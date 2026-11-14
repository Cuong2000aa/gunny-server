# Tài liệu Gunny II Private Server

Bộ tài liệu chuẩn cho repo `gunny-server`. Đọc theo thứ tự nếu mới vào dự án.

| # | Tài liệu | Đọc khi |
|---|---|---|
| 1 | [`../SOURCE-ARCHITECTURE.md`](../SOURCE-ARCHITECTURE.md) | Hiểu kiến trúc: process, port, boot sequence, patch không được regress |
| 2 | [`../SETUP-OTHER-MACHINE.md`](../SETUP-OTHER-MACHINE.md) | Dựng máy mới từ `git clone` |
| 3 | [`OPERATIONS.md`](OPERATIONS.md) | Start / stop / restart, log, Cloudflare tunnel, build |
| 4 | [`DATABASE.md`](DATABASE.md) | Bảng quan trọng, item category, ô túi đồ, phát đồ an toàn, export `.bak` |
| 5 | [`WEAPONS-AND-BALLS.md`](WEAPONS-AND-BALLS.md) | Vũ khí, đạn, trợ thủ, asset hiệu ứng, thêm vũ khí mới |
| 6 | [`WEB-CLIENT.md`](WEB-CLIENT.md) | `play.html`, Ruffle, mobile/Android, bàn phím, PWA, remote log |
| 7 | [`TOOLS-FLASH.md`](TOOLS-FLASH.md) | FFDec, export script AS3, patch SWF `2.png` |
| 8 | [`TROUBLESHOOTING.md`](TROUBLESHOOTING.md) | Triệu chứng → nguyên nhân → cách fix, kèm các sự cố đã gặp |
| 9 | [`SECURITY.md`](SECURITY.md) | Rủi ro bảo mật đã biết khi public qua tunnel |

## Tóm tắt 30 giây

```
Browser (Ruffle WASM) ──HTTP/WS :828──► Tank.Request ──TCP──► Road :9500 ──► Center :9202
                                                                  └──────► Fighting :9208
                                         SQL Server (Docker gunny-sql) :1433
```

- Chơi: `http://127.0.0.1:828/web/play.html` (hoặc URL Cloudflare tunnel + `/web/play.html`).
- Tài khoản: `admin` (UserID 1, Lv60, đồ full) và `max40`. Mật khẩu bất kỳ.
- SQL: `sa` / `Gunny@2026Mac`, container `gunny-sql`.
- Client thật là SWF đổi tên `Source Flash/FlashSV1/2.png`.

## Quy tắc vàng

1. Đổi data DB mà client cũng đọc (item, đạn) → kiểm tra cả file XML client tải (`TemplateAlllist.xml`, `bombconfig.xml`, `BallList.xml`). Lệch giữa server và client là nguyên nhân số 1 gây **màn hình vàng**.
2. Sửa DB người chơi → **tắt Road trước**, nếu không dữ liệu trong RAM sẽ ghi đè.
3. Sửa SWF / XML client → tăng `<version to="N">` trong `Source Flash/FlashSV1/config.xml` để client bỏ cache.
4. Đổi DB muốn máy khác giống hệt → export lại `Database/*.bak` (xem `DATABASE.md`).
