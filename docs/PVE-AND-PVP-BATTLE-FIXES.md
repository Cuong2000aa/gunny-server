# Sửa lỗi toàn diện các trận chiến PVE & PVP (Hang Rồng, Vòng Xoáy Thời Gian, Đấu Trường Dũng Sĩ, Băng Vương Côn 91%)

Tài liệu này tổng hợp nguyên nhân gốc rễ và giải pháp chi tiết đã áp dụng để khắc phục các lỗi crash, lỗi rơi vực, kẹt lượt 60 giây và treo màn hình 91% khi vào trận.

---

## 1. Lỗi kẹt loading 91% khi vào trận (PVP & PVE)

### 1.1 Triệu chứng
- Người chơi (Admin hoặc tài khoản cấp cao) vào trận (Đấu trường, Tự do, Khiêu chiến, hoặc Phó bản) bị treo vĩnh viễn ở `loading. 91%`.
- Đối thủ bot (ví dụ: `China (MAX)`) hoặc đồng đội đã `OK` (100%), đồng hồ đếm ngược chạy về 0 nhưng không bao giờ vào được trận đấu.

### 1.2 Nguyên nhân gốc rễ
- Admin đang trang bị vũ khí **Thần Khí - Băng Vương Côn** (`TemplateID = 7142`, Pic = `Snunchakus`).
- Trong file `Tank.Request/bombconfig.xml`, vũ khí 7142 được cấu hình dùng các loại đạn:
  - `Common = 11225`
  - `CommonMultiBall = 112251`
  - `CommonAddWound = 112252`
  - `Special = 11230`
  (và các cấp cường hóa 71421, 71422, 71423, 71424 dùng các ball 11226..11229 và 11231..11234).
- Các file asset trên đĩa (`Resources/image/bomb/blastout/blastout11225.swf`, `bullet1039.swf`, `bullet1040.swf`, `GameAssets/bomb/11225.bomb`, v.v.) đều đầy đủ, nhưng **cả 20 mã ball này chưa từng được INSERT vào bảng `Ball` và bảng `BallConfig` trong SQL Server `Project_Game34`**.
- Do đó, `BallList.xml` khi sinh ra không có các ball này.
- Khi vào trận, Flash client gọi:
  ```actionscript
  // LoadBombManager.as
  public function getLoadBombComplete(param1:WeaponInfo) : Boolean {
      for each (var b in param1.bombs) {
          if (!BallManager.findBall(b).isComplete()) return false;
      }
      return true;
  }
  ```
- Do `BallManager.findBall(11225)` trả về `null`, lệnh gọi `.isComplete()` trên `null` gây `TypeError #1009 (NullReferenceException)` trong ActionScript.
- Hàm tính tiến trình `checkProgress()` trong `RoomLoadingView.as` bị chặn lại khi đạt 10/11 hoặc 11/12 tài nguyên hoàn tất $\rightarrow$ tiến trình dừng đúng ở $11 / 12 \times 100\% = \mathbf{91\%}$.

### 1.3 Cách khắc phục
1. **Bổ sung bảng `Ball` trong DB `Project_Game34`**:
   - Thêm 15 đạn thường (11225..11229 và các biến thể 1, 2) với `BombPartical = '1039'`, `Crater = '830'`.
   - Thêm 5 đạn Pow/Special (11230..11234) với `BombPartical = '1040'`, `Crater = '831'`.
2. **Bổ sung bảng `BallConfig` trong DB `Project_Game34`**:
   - Thêm cấu hình cho 5 vũ khí 7142, 71421, 71422, 71423, 71424.
3. **Tái tạo XML Client**:
   - Chạy `curl http://127.0.0.1:828/createallxml.ashx` để build lại `BallList.xml`.
   - Kiểm tra xác thực tự động toàn bộ 835 vũ khí trong `bombconfig.xml`: **0 vũ khí thiếu ball**.

---

## 2. Phó bản Đấu Trường Dũng Sĩ (PVE 13)

### 2.1 Ải 4 (Map 13004 - Boss Mathias)
- **Kẹt chết ngay khi vừa vào ải**: Boss Mathias bị rơi xuyên vực chết ngay lập tức do spawn tại tọa độ $X=900$, nơi không có mặt đất collision.
- **Lỗi collision Map 13004**: Map 13004 trước đó thiếu file `dead.map` nhị phân, kích thước DB `Game_Map` bị sai.
- **Khắc phục**:
  - Trích xuất và biên dịch file nhị phân `dead.map` ($1132 \times 1055$) từ `Resources/image/map/13004/dead.png`, đồng bộ vào `GameAssets/map/13004/`, `Road.Service/map/13004/`, `Fighting.Service/map/13004/`.
  - Cập nhật thông số chuẩn trong DB `Game_Map` và cập nhật điểm rơi của người chơi ($Y=940$).
  - Sửa `WAS13004.cs`: Đưa Boss Mathias về ngai vàng tại tọa độ $X=260, Y=200$ (tiếp đất trên bục cao $Y=710$), hướng mặt nhìn sang phải (`direction = 1`) về phía người chơi ($X \in [500..650]$).

### 2.2 Ải 1 (WAS13001) - Quái kẹt ngoài map
- Tọa độ quái spawn cũ là $X=1330$, vượt ra khỏi chiều rộng map ($X_{max}=1130$), quái không thể di chuyển hoặc tiếp đất.
- **Khắc phục**: Viết lại thuật toán spawn theo từng đợt (wave), mỗi đợt duy trì tối đa 6 quái trên sàn, spawn ngẫu nhiên trong khoảng an toàn $X \in [700, 1100]$. Bổ sung logic tiếp viện qua `OnNewTurnStarted` cho đến khi tiêu diệt đủ `TotalCount`.

---

## 3. Phó bản Vòng Xoáy Thời Gian (PVE 12)

### 3.1 Ải 2 (Map 12002 - Boss Cá Sấu & Thuyền Trưởng)
- **Boss rơi xuyên sàn thuyền**: Lệnh `FallFrom(900, 500)` bắt đầu quét mặt đất từ $Y=500$, bỏ qua sàn tàu ở $Y=281$ và rơi xuống đáy biển ở $Y=570$.
- **Khắc phục**: Sửa `FallFrom(900, 0, "", ...)` quét top-down từ đỉnh map để bắt chính xác sàn tàu tại $Y=281$.

### 3.2 Ải 1 (TVS12001)
- Áp dụng cơ chế wave spawn thông minh, giới hạn số quái sống đồng thời để tránh tràn bộ nhớ và chống quái kẹt ngoài biên bản đồ.

---

## 4. Phó bản Hang Rồng (PVE 5)

### 4.1 Boss quét mặt đất (DCR5101, 5102, 5103, 5104)
- Sửa toàn bộ điểm bắt đầu `FallFrom` từ $Y=500$ thành $Y=0$, đảm bảo mọi Boss rồng và NPC đều tiếp đất chính xác trên các mỏm đá cao mà không bị lọt vực.

### 4.2 Chuẩn hóa Animation Frame Label
- Trong SWF của các Boss rồng và quái sói/đấu trường, các action đánh cận chiến được đặt nhãn là `beatA` thay vì `beat`.
- Đã đồng bộ tất cả các lệnh gọi `PlayMovie("beat", ...)` thành `PlayMovie("beatA", ...)` trên 12 file script AI NPC để tránh tình trạng animation bị khựng hoặc bỏ qua lượt.

---

## 5. Lỗi SimpleNpc kẹt lượt 60 giây

### 5.1 Triệu chứng
- Khi quái thường (NPC) kết thúc hành động di chuyển hoặc tấn công, lượt đấu không chuyển ngay cho người chơi mà bị treo chờ hết 60 giây timeout.

### 5.2 Nguyên nhân & Khắc phục
- Trong `Game.Logic/Phy/Object/SimpleNpc.cs`, phương thức `StartAttacking()` không gọi `StopAttacking()` khi hoàn tất chuỗi hành động (khác với `SimpleBoss.cs`).
- Thêm kiểm tra:
  ```csharp
  if (base.IsAttacking)
  {
      StopAttacking();
  }
  ```
- Nhờ đó, cờ `IsAttacking` được hạ ngay lập tức, giải phóng `WaitLivingAttackingAction` và chuyển lượt tức thì cho người chơi.

---

## 6. Bổ sung ảnh chuyển ải (show1.jpg, show2.jpg)
- Tạo đầy đủ `show1.jpg` và `show2.jpg` từ `back.jpg` cho tất cả các map phó bản:
  - Map Hang Rồng: `5101`, `5102`, `5103`, `5104`
  - Map Vòng Xoáy Thời Gian: `12001`, `12002`, `12003`, `12004`
  - Map Đấu Trường Dũng Sĩ: `13001`, `13002`, `13003`, `13004`
- Ngăn ngừa tình trạng Flash client gửi request tải ảnh chuyển ải bị mã lỗi HTTP 404.
