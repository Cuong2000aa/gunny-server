# Công cụ Flash: đọc và patch client SWF

Client core là SWF đổi tên **`Source Flash/FlashSV1/2.png`**. Loader: `Loading.swf`, `DDT_Loading.swf`.

## 1. Công cụ (không commit vào git)

Các thư mục sau nằm trong `.gitignore` vì nặng (~150MB), tải lại khi cần:

| Path | Là gì | Nguồn |
|---|---|---|
| `tools/jre17/` (+ `.zip`) | Java 17 JRE portable | Adoptium Temurin 17 JRE |
| `tools/ffdec/` (+ `.zip`) | JPEXS FFDec (decompiler/editor SWF) | https://github.com/jindrapetrik/jpexs-decompiler/releases |
| `tools/export_2png/scripts` | Script AS3 decompile từ `2.png` | Tự sinh (lệnh dưới) |
| `tools/export_loading`, `export_ddtload` | Script từ `Loading.swf`, `DDT_Loading.swf` | Tự sinh |
| `tools/verify_*` | Export lại sau khi patch để so sánh | Tự sinh |

Còn trong git: `tools/full_pve_stories.py/.sql`.

## 2. Export script để đọc

```powershell
$env:JAVA_HOME = "$PWD\tools\jre17\jdk-17.0.21+9-jre"
& "$env:JAVA_HOME\bin\java.exe" -jar tools\ffdec\ffdec.jar -export script tools\export_2png "Source Flash\FlashSV1\2.png"
```
Tìm class: `rg "function isProperTo_CanEmbedEquipList" tools/export_2png/scripts`.

Class hay tra:
| Class | Việc |
|---|---|
| `room.view.RoomLoadingView`, `LoadBombManager` | Màn loading vào trận, preload đạn |
| `ddt.manager.WeaponBallManager` | Đọc `bombconfig.xml` |
| `bagAndInfo.bag.BagEquipListView` | Túi trang bị (ô 31–127) |
| `store.data.StoreModel` | Lọc đồ khảm/cường hóa khi vào sảnh |
| `hall.HallStateView` | Sảnh — **không** replace cả class (VerifyError trên Ruffle) |
| `LoginStateView.forceLoginNow` | Patch login 1 lần |

## 3. Patch SWF

1. Backup `2.png` (git đã track bản sạch — `git checkout -- "Source Flash/FlashSV1/2.png"` để quay lại).
2. Sửa **tối thiểu** một method: `ffdec -replace 2.png 2.png.new <Class> <file.as>`.
3. Export lại vào `tools/verify_2png` để chắc compile đúng.
4. Thay file, tăng `version to` trong `config.xml`, test Ruffle (xem `client.log`).
5. **Ưu tiên sửa data/server** thay vì patch SWF. Patch SWF trước đây (`checkProgress` early return) từng làm vụ loading tệ hơn.
