# Rủi ro bảo mật đã biết

Server được thiết kế cho chơi local/LAN. Khi public qua Cloudflare tunnel, các điểm sau **mở cho bất kỳ ai có URL**:

| Điểm | Vị trí | Rủi ro | Hướng xử lý |
|---|---|---|---|
| `ValidAdminIP` luôn `true` | `Tank.Request/csFunction.cs` | Handler `.ashx` kiểm tra admin IP (ví dụ tạo lại XML, nạp/thưởng) gọi được từ Internet | Chỉ cho loopback/LAN; tách whitelist cho XML template public |
| `AdminIP` rỗng | `Tank.Request/App.config`, `Web.config` | Như trên | Đặt lại IP hoặc dùng cơ chế token |
| `/api/admin/toggle-resource`, `/api/admin/resource-status` | `Tank.Request/Program.cs` | Chỉ kiểm tra query `?user=admin` → ai cũng đổi được cấp/xu của Admin | Kiểm tra session/đăng nhập thật hoặc chỉ cho loopback |
| Login mật khẩu bất kỳ | Login handler (RAW/bypass) | Ai biết tên `admin` là vào được tài khoản Admin | Bật lại kiểm tra mật khẩu trước khi public |
| `/clientlog` không giới hạn | `Tank.Request/Program.cs` | Ghi file không giới hạn kích thước | Giới hạn body + xoay vòng file |
| SA password + RSA private key trong config | `App.config`, `Program.cs` | Lộ nếu repo public | Dùng biến môi trường / user-secrets |

**Không bao giờ** expose port 1433, 9202, 9208, 9500 ra ngoài — chỉ 828 qua tunnel.
