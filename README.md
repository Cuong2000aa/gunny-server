# Gunny II (.NET 8 & Web Ruffle Private Server)

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![Ruffle](https://img.shields.io/badge/WebAssembly-Ruffle-FF6B6B?style=flat&logo=webassembly)](https://ruffle.rs/)
[![Maintainer](https://img.shields.io/badge/Maintainer-Cuong2000aa-blue?style=flat)](https://github.com/Cuong2000aa)

> **Lưu ý & Tuyên bố trách nhiệm:** Dự án này là một **Private Server Gunny / DDTank** (dựa trên nền tảng Gunny II / DDTank 3.4/4.1) phục vụ mục đích nghiên cứu, học tập kiến trúc hệ thống game multiplayer cổ điển và bảo tồn di sản game Flash. Bản quyền thương hiệu và tài nguyên đồ họa gốc thuộc về **7Road** và các nhà phát hành chính thức. Dự án không nhận là tác giả game gốc; dự án này do **Cuong2000aa** bảo trì, port và hiện đại hóa trên nền tảng công nghệ mới.

---

## 🚀 Các điểm cải tiến & hiện đại hóa

1. **Backend .NET 8 Đa Nền Tảng (Cross-Platform)**:
   - Toàn bộ các dịch vụ máy chủ (`Center.Service`, `Fighting.Service`, `Road.Service`, `Tank.Request`) đã được nâng cấp lên **.NET 8 C#**, chạy native trực tiếp trên macOS (Apple Silicon / Intel), Linux và Windows mà không cần máy ảo Windows Server hay IIS truyền thống.
   - Kết nối cơ sở dữ liệu SQL Server qua Docker container (`mcr.microsoft.com/azure-sql-edge` hoặc MS SQL).

2. **Chơi Trực Tiếp Trên Trình Duyệt Không Cần Flash Player (Ruffle Emulation)**:
   - Tích hợp trình giả lập WebAssembly **Ruffle**, cho phép mở web chơi ngay trên trình duyệt hiện đại (Chrome, Edge, Safari, Firefox).
   - Tích hợp cầu nối **WebSocket TCP Proxy** tại cổng HTTP 828 (`/ws` $\rightarrow$ cổng game TCP 9500).

3. **Màn Hình Đa Tỉ Lệ & Giao Diện Tùy Biến**:
   - Khóa chuẩn tỉ lệ vàng $10:6$ ($1000 \times 600$), hỗ trợ các preset kích thước: **100% (1000x600)**, **125% (1250x750)**, **150% (1500x900)** và **Toàn Màn Hình (Fullscreen)** có letterbox viền đen chống biến dạng.
   - Ghi nhớ tùy chọn người dùng tự động bằng `localStorage`.
   - Tích hợp sẵn Bàn phím ảo (Virtual Gamepad) cho thiết bị di động (iOS / Android qua mạng LAN).

4. **Vật Lý Hố Đào (Crater Physics) Chuẩn Đồ Họa**:
   - Server đồng bộ hình dạng hố nổ theo `Crater ID` thực tế của từng loại vũ khí thay vì mã đạn thô.
   - Bổ sung xử lý viền hố (`craterbrink.png` / `_brink.bomb`) với thuật toán bitwise `Tile.Add`, loại bỏ lỗi viền đen/trong suốt dị thường.

5. **Asset Proxy Thông Minh & Caching Cục Bộ**:
   - Máy chủ `Tank.Request` tích hợp cơ chế cache tài nguyên cục bộ (`Resources/`) cho vũ khí, trang phục theo mốc cường hóa và crater hố nổ, hạn chế tối đa độ trễ giật lag mạng giữa trận đấu.

---

## 🛠 Cấu trúc thư mục mã nguồn

```
Gunny/
├── Bussiness/            # Logic nghiệp vụ người chơi, item, quest, ngôn ngữ
├── Center.Server/        # Server điều phối trung tâm
├── Center.Service/       # Dịch vụ thực thi Center Server (.NET 8)
├── Fighting.Server/      # Server tính toán phòng đấu và room combat
├── Fighting.Service/     # Dịch vụ thực thi Fighting Server (.NET 8)
├── Game.Base/            # Nền tảng mạng TCP Socket, Packet buffer, Cryptography
├── Game.Logic/           # Động cơ vật lý đạn đạo, map, hố đào (BallMgr, Tile, SimpleBomb)
├── Game.Server/          # Server gameplay thời gian thực (World, Inventory, NPC, Room)
├── Road.Service/         # Dịch vụ thực thi Game Server chính (Port 9500)
├── Tank.Request/         # Máy chủ HTTP / Web API / Asset Proxy / WebSocket Bridge (Port 828)
├── Tank.Flash/           # Bộ client Flash SWF, config.xml, play.html
└── Resources/            # Thư mục chứa asset ảnh đồ họa, hiệu ứng hố nổ, vũ khí
```

---

## 💻 Hướng dẫn chạy nhanh (Local Quickstart)

### 1. Yêu cầu môi trường
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) (để chạy cơ sở dữ liệu SQL Server)

### 2. Khởi động Cơ sở dữ liệu SQL Server
```bash
docker run -d --name gunny-sql \
  -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=Gunny@2026Mac" \
  -p 1433:1433 \
  mcr.microsoft.com/azure-sql-edge:latest
```

### 3. Build & Khởi động Server
Khởi động tuần tự 4 dịch vụ cốt lõi:
```bash
# 1. Center Service (Port 9202)
dotnet run --project Center.Service/Center.Service.csproj

# 2. Fighting Service (Port 9208)
dotnet run --project Fighting.Service/Fighting.Service.csproj

# 3. Road Service (Port 9500)
dotnet run --project Road.Service/Road.Service.csproj

# 4. Web & Asset Proxy (Port 828)
dotnet run --project Tank.Request/Tank.Request.csproj
```

### 4. Vào game
Mở trình duyệt bất kỳ và truy cập:
```
http://127.0.0.1:828/web/play.html
```

---

## 👤 Maintainer

- **Maintainer:** [Cuong2000aa](https://github.com/Cuong2000aa)
- **Email / Contact:** Qua trang cá nhân GitHub chính thức.

# tree-refresh:2026-11-11T02:31:26Z
