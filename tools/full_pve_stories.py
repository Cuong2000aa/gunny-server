#!/usr/bin/env python3
"""Full Gunny-style per-map stories for ALL dungeons (Mission_Info + Pve_Info)."""
from __future__ import annotations

import re
from pathlib import Path

# (id, name, description, success)
MISSIONS: list[tuple[int, str, str, str]] = []

def m(id_: int, name: str, desc: str, ok: str) -> None:
    MISSIONS.append((id_, name, desc, ok))

# ---- Fight Lab / training ----
for i, (name, tip) in {
    101: ("Đo khoảng cách (Dễ)", "Bài tập đo khoảng cách — hạ bia để nắm thước ngắm."),
    102: ("Góc 20° (Dễ)", "Luyện góc thấp 20° — đạn bay sát đất."),
    103: ("Góc 65° (Dễ)", "Luyện góc cao 65° — đạn vòng cung."),
    104: ("Siêu cao (Dễ)", "Bắn từ vị trí siêu cao — canh lực theo độ cao."),
    105: ("Cao–thấp (Dễ)", "Địch ở cao và thấp xen kẽ — đổi góc liên tục."),
    111: ("Đo khoảng cách (Thường)", "Đo khoảng cách độ thường — nhiều bia hơn."),
    112: ("Góc 20° (Thường)", "Góc 20° độ thường."),
    113: ("Góc 65° (Thường)", "Góc 65° độ thường."),
    114: ("Siêu cao (Thường)", "Siêu cao độ thường."),
    115: ("Cao–thấp (Thường)", "Cao–thấp độ thường."),
    121: ("Đo khoảng cách (Khó)", "Đo khoảng cách độ khó — không được lệch."),
    122: ("Góc 20° (Khó)", "Góc 20° độ khó."),
    123: ("Góc 65° (Khó)", "Góc 65° độ khó."),
    124: ("Siêu cao (Khó)", "Siêu cao độ khó."),
    125: ("Cao–thấp (Khó)", "Cao–thấp độ khó."),
}.items():
    m(i, name, tip, "Hạ hết bia tập luyện.")

# ---- Sư đồ / disciple maps ----
m(600, "Vịnh Cariga — Bãi đáp", "Bá tước đưa các ngươi tập huấn tại Vịnh Cariga. Quái biển tràn lên bãi — quét sạch để mở đường vào hang đá.", "Hạ hết quái vật bãi đáp.")
m(601, "Vịnh Cariga — Thủ lĩnh", "Trong hang đá, thủ lĩnh vịnh xuất hiện. Hạ nó để hoàn thành khóa huấn luyện Cariga.", "Hạ thủ lĩnh Vịnh Cariga.")
m(700, "Lâu đài Dracula — Hành lang", "Lâu đài Daucula tối om. Đám tạo vật Franken canh hành lang — tiêu diệt chúng để tới phòng thí nghiệm.", "Hạ hết quái vật hành lang.")
m(701, "Lâu đài Dracula — Franken", "Phòng thí nghiệm mở ra. Franken thức tỉnh! Kết thúc khóa huấn luyện Dracula.", "Hạ Franken.")
m(800, "Rừng Gamazon — Đường mòn", "Rừng rậm Gamazon. Người lựu đạn bộ lạc chặn đường mòn — mở đường cho sư phụ.", "Hạ hết Người lựu đạn.")
m(801, "Rừng Gamazon — Chiến sĩ tà thần", "Sâu rừng, Chiến sĩ tà thần xuất hiện. Hạ hắn để hoàn thành khóa Gamazon.", "Hạ Chiến sĩ tà thần.")
m(900, "Hoang mạc Xatara — Cồn cát", "Gió cát Xatara. Người lựu đạn mai phục trên cồn — quét sạch rồi mới gặp vua bò cạp.", "Hạ hết Người lựu đạn.")
m(901, "Hoang mạc Xatara — Vua Bò Cạp", "Dưới lớp cát, Vua Bò Cạp trồi lên. Trận cuối khóa huấn luyện Xatara.", "Hạ Vua Bò Cạp.")

# ---- Chicken castle (already partly done; keep consistent) ----
m(1071, "Cổng Lâu đài Gà", "Đảo Porti từng yên bình. Ba năm trước đế quốc xâm lược — hôm nay các ngươi đứng trước cổng. Hạ Gà hồng / tinh anh để mở đường.", "Hạ đủ Gà tinh anh và Gà hồng.")
m(1072, "Đội trưởng Baroff", "Cổng đã vỡ. Đội trưởng Baroff chặn hành lang đại điện.", "Đánh bại Đội trưởng Baroff.")
m(1171, "Cổng Lâu đài Gà", "Porti dưới bóng chiến tranh. Cổng lâu đài có thêm vệ binh — quét sạch để tiến sâu.", "Tiêu diệt đủ 20 Gà hồng và 5 Gà tinh anh.")
m(1172, "Đội trưởng Baroff", "Baroff — tay chân đắc lực của Quốc Vương — chặn giữa hành lang.", "Đánh bại Đội trưởng Baroff.")
m(1173, "Quốc Vương Gà", "Đại điện mở ra. Quốc Vương biến dị thức tỉnh cả hai dạng. Tiêu diệt hoàn toàn mới giải phóng Porti.", "Đánh bại Quốc Vương Gà (cả 2 dạng).")
m(1271, "Cổng Lâu đài Gà (Khó)", "Độ khó tăng — cổng dày đặc vệ binh tinh nhuệ.", "Tiêu diệt đủ 20 Gà hồng và 5 Gà tinh anh.")
m(1272, "Đội trưởng Baroff (Khó)", "Baroff giận dữ sau lần thua — lần này không còn đường lui.", "Đánh bại Đội trưởng Baroff.")
m(1273, "Quốc Vương Gà (Khó)", "Quốc Vương hấp thụ tà thuật. Cả hai dạng phải bị đánh bại.", "Đánh bại Quốc Vương Gà (cả 2 dạng).")
m(1274, "Matthias", "Sau Quốc Vương xuất hiện Matthias — bóng đen đứng sau cuộc xâm lược.", "Tiêu diệt Matthias.")
m(1371, "Cổng Lâu đài Gà (Anh hùng)", "Phiên bản anh hùng — chỉ đội mạnh nhất mới phá được cổng.", "Tiêu diệt đủ 20 Gà hồng và 5 Gà tinh anh.")
m(1372, "Đội trưởng Baroff (Anh hùng)", "Baroff hình thái tối thượng canh giữ đại điện.", "Đánh bại Đội trưởng Baroff.")
m(1373, "Chìa khóa ma thuật", "Cửa kết tinh khóa chặt ngai vàng. Thu đủ chìa khóa ma thuật để mở đường.", "Thu đủ chìa khóa và mở cửa kết tinh.")
m(1374, "Quốc Vương Gà (Anh hùng)", "Quốc Vương hóa thân hoàn toàn — hai dạng hủy diệt.", "Đánh bại Quốc Vương Gà (cả 2 dạng).")
m(1375, "Bóng ma Matthias", "Matthias không chết — linh hồn hắn bám lấy lâu đài.", "Tiêu diệt Bóng ma Matthias.")

# ---- Ant ----
m(2001, "Cửa hang Huyệt Ma Kiến", "Thương nhân mất hàng trong rừng ma pháp. Lần theo dấu vết tới cửa hang — đàn kiến tiên phong/trinh sát tràn ra.", "Tiêu diệt đủ kiến tiên phong & trinh sát.")
m(2002, "Nữ Vương Kiến Daniliz", "Sau lớp kiến lính là phòng trứng. Daniliz tuyên bố mọi thứ trong hang đều thuộc về nàng.", "Đánh bại Nữ Vương Kiến Daniliz.")
m(2101, "Cửa hang Huyệt Ma Kiến (Thường)", "Độ thường — kiến đông hơn, phòng thủ dày.", "Tiêu diệt đủ kiến.")
m(2102, "Nữ Vương Kiến Daniliz (Thường)", "Daniliz phòng thủ vững hơn. Hạ nàng để thông tuyến thương mại.", "Đánh bại Nữ Vương Kiến Daniliz.")
m(2401, "Ác mộng — Cửa hang kiến", "Ác mộng: bóng tối nuốt hang, kiến hung hãn gấp bội.", "Tiêu diệt đủ kiến.")
m(2402, "Ác mộng — Nữ Vương Daniliz", "Trong ác mộng Daniliz không còn hình dạng quen thuộc.", "Đánh bại Nữ Vương Kiến.")
m(2501, "Ác mộng Thường — Cửa hang", "Ác mộng thường: cửa hang dày đặc kiến trinh sát.", "Tiêu diệt đủ kiến.")
m(2502, "Ác mộng Thường — Nữ Vương", "Phòng trứng biến thành bẫy.", "Đánh bại Nữ Vương Kiến.")
m(2601, "Ác mộng Khó — Cửa hang", "Ác mộng khó: từng bước đều là máu.", "Tiêu diệt đủ kiến.")
m(2602, "Ác mộng Khó — Nữ Vương", "Daniliz hình thái ác mộng khó.", "Đánh bại Nữ Vương Kiến.")
m(2701, "Ác mộng Anh hùng — Cửa hang", "Chỉ đội anh hùng mới qua nổi cửa hang ác mộng.", "Tiêu diệt đủ kiến.")
m(2702, "Ác mộng Anh hùng — Nữ Vương", "Trận quyết chiến với Daniliz tối thượng.", "Đánh bại Nữ Vương Kiến.")
m(3001, "Huyệt Ma Kiến (CSM)", "Hang kiến sâu hơn — Nữ Vương chờ phía cuối.", "Đánh bại Nữ Vương Kiến.")
m(4001, "Huyệt Ma Kiến (DCSM)", "Hang kiến cấp cao — tiêu diệt Nữ Vương để kết thúc.", "Đánh bại Nữ Vương Kiến.")

# ---- Evil Tribe ----
m(3101, "Bộ lạc Tà thần — Cổng làng", "Bình gốm cổ vỡ lộ ra bản đồ bộ lạc. Cổng làng có dũng sĩ bộ lạc chặn lối — hạ họ để vào sâu.", "Hạ dũng sĩ bộ lạc.")
m(3102, "Bộ lạc Tà thần — Đậu thần", "Giữa làng, đậu thần khô héo. Hồi sinh đậu thần để mở nghi thức và cửa rừng.", "Hồi sinh đậu thần.")
m(3103, "Bộ lạc Tà thần — Anh em Bulan", "Anh em Bulan bảo vệ thánh địa. Hạ cả hai mới tới Chúa Tể.", "Hạ anh em Bulan.")
m(3106, "Bộ lạc Tà thần — Chúa Tể Rừng Xanh", "Thánh địa mở. Chúa Tể Rừng Xanh thức tỉnh — kết thúc hành trình bộ lạc.", "Đánh bại Chúa Tể Rừng Xanh.")
m(3201, "Bộ lạc Tà thần (Khó) — Cổng làng", "Độ khó: dũng sĩ bộ lạc đông và mạnh hơn.", "Hạ dũng sĩ bộ lạc.")
m(3202, "Bộ lạc Tà thần (Khó) — Đậu thần", "Đậu thần bị nguyền — phải hồi sinh dưới áp lực.", "Hồi sinh đậu thần.")
m(3203, "Bộ lạc Tà thần (Khó) — Anh em Bulan", "Bulan kích nộ. Hạ cả hai.", "Hạ anh em Bulan.")
m(3206, "Bộ lạc Tà thần (Khó) — Chúa Tể", "Chúa Tể Rừng Xanh hình thái khó.", "Đánh bại Chúa Tể Rừng Xanh.")
m(3301, "Bộ lạc Tà thần (AH) — Cổng làng", "Anh hùng: cổng làng là biển người.", "Hạ dũng sĩ bộ lạc.")
m(3302, "Bộ lạc Tà thần (AH) — Đậu thần", "Nghi thức đậu thần dưới lửa địa ngục.", "Hồi sinh đậu thần.")
m(3303, "Bộ lạc Tà thần (AH) — Anh em Bulan", "Bulan tối thượng.", "Hạ anh em Bulan.")
m(3306, "Bộ lạc Tà thần (AH) — Chúa Tể", "Chúa Tể Rừng Xanh tối thượng — trận cuối.", "Đánh bại Chúa Tể Rừng Xanh.")

# ---- Dark Castle ----
m(4101, "Pháo đài hắc ám — Cửa thành", "Cuộc phiêu lưu mới: phá vỡ cửa thành hắc ám để vào bên trong.", "Phá vỡ cửa thành.")
m(4102, "Pháo đài hắc ám — Sói Tinh & Chim Ưng", "Trong sân thành, Sói Tinh và Chim Ưng phối hợp tấn công. Hạ cả hai.", "Hạ Sói Tinh và Chim Ưng.")
m(4103, "Pháo đài hắc ám — Minotaur", "Hầm tối — Minotaur canh giữ lõi pháo đài. Đánh bại nó để giải phóng pháo đài.", "Đánh bại Minotaur.")
m(4201, "Pháo đài hắc ám (Khó) — Cửa thành", "Cửa thành kiên cố hơn — phá vỡ bằng sức mạnh.", "Phá vỡ cửa thành.")
m(4202, "Pháo đài hắc ám (Khó) — Sói & Ưng", "Sói Tinh và Chim Ưng điên cuồng.", "Hạ Sói Tinh và Chim Ưng.")
m(4203, "Pháo đài hắc ám (Khó) — Minotaur", "Minotaur hình thái khó.", "Đánh bại Minotaur.")
m(4301, "Pháo đài hắc ám (AH) — Cửa thành", "Anh hùng: cửa thành gần như bất khả xâm phạm.", "Phá vỡ cửa thành.")
m(4302, "Pháo đài hắc ám (AH) — Sói & Ưng", "Song thủ hộ vệ tối thượng.", "Hạ Sói Tinh và Chim Ưng.")
m(4303, "Pháo đài hắc ám (AH) — Minotaur", "Minotaur tối thượng — kết thúc pháo đài.", "Đánh bại Minotaur.")

# ---- Dragon war ----
m(5101, "Hang núi lửa — Quỷ Pochi", "Ngọn núi lửa giữa đảo. Quỷ Pochi chặn cửa hang.", "Hạ Quỷ Pochi.")
m(5102, "Pochi tái sinh", "Pochi hấp thụ dung nham và tái sinh mạnh hơn.", "Hạ Quỷ Pochi (dạng 2).")
m(5103, "Quỷ Khăn Hồng", "Cánh cửa rồng do Quỷ Khăn Hồng canh giữ.", "Hạ Quỷ Khăn Hồng.")
m(5104, "Tà Diệm Long", "Đỉnh núi lửa — Tà Diệm Long thức tỉnh.", "Hạ Tà Diệm Long.")
m(5201, "Hang núi lửa (Khó) — Pochi", "Độ khó tăng. Pochi giận dữ ngay cửa hang.", "Hạ Quỷ Pochi.")
m(5202, "Pochi dung nham đỏ", "Pochi hấp thụ dung nham đỏ.", "Hạ Quỷ Pochi (dạng 2).")
m(5203, "Khăn Hồng nhuốm máu", "Khăn hồng nhuốm máu. Chỉ kẻ mạnh mới qua.", "Hạ Quỷ Khăn Hồng.")
m(5204, "Tà Diệm Long (Khó)", "Long Vương khó tính thức tỉnh.", "Hạ Tà Diệm Long.")
m(5301, "Địa ngục mở cửa — Pochi", "Pochi hình thái hủy diệt.", "Hạ Quỷ Pochi.")
m(5302, "Pochi lửa đen", "Pochi hấp thụ lửa đen.", "Hạ Quỷ Pochi (dạng 2).")
m(5303, "Cửa ngõ địa ngục", "Khăn hồng — cửa ngõ địa ngục.", "Hạ Quỷ Khăn Hồng.")
m(5304, "Rồng Chúa núi lửa", "Tà Diệm Long hình thái tận thế.", "Hạ Tà Diệm Long.")
m(1243, "Rồng Cổ", "Đảo Rồng Cổ — sinh vật ngàn năm thức tỉnh.", "Tiêu diệt Rồng Cổ.")

# ---- Chicken Olympics / arena ----
m(6101, "Đấu trường gà — Vòng điểm", "Quốc Vương tổ chức đại hội thể thao. Đạt 99 điểm để qua vòng loại.", "Đạt 99 điểm.")
m(6102, "Đấu trường gà — Đua về đích", "Đội đỏ phải về đích trước — đừng để đội xanh thắng!", "Đội đỏ về đích trước.")
m(6103, "Đấu trường gà — Oai Tử", "Oai Tử khiêu chiến trên đấu trường. Hạ hắn để vào chung kết.", "Hạ Oai Tử.")
m(6104, "Đấu trường gà — Trọng Tài Đen", "Trọng Tài Đen thao túng luật lệ. Đánh bại hắn để trả lại công bằng cho đại hội.", "Hạ Trọng Tài Đen.")
m(6201, "Đấu trường gà (Khó) — Vòng điểm", "Độ khó: đạt 99 điểm dưới áp lực lớn hơn.", "Đạt 99 điểm.")
m(6202, "Đấu trường gà (Khó) — Đua về đích", "Cuộc đua sinh tử.", "Đội đỏ về đích trước.")
m(6203, "Đấu trường gà (Khó) — Oai Tử", "Oai Tử điên cuồng.", "Hạ Oai Tử.")
m(6204, "Đấu trường gà (Khó) — Trọng Tài Đen", "Trọng Tài Đen hình thái khó.", "Hạ Trọng Tài Đen.")
m(6301, "Đấu trường gà (AH) — Vòng điểm", "Anh hùng: vòng điểm khắc nghiệt.", "Đạt 99 điểm.")
m(6302, "Đấu trường gà (AH) — Đua về đích", "Một sai sót là thua.", "Đội đỏ về đích trước.")
m(6303, "Đấu trường gà (AH) — Oai Tử", "Oai Tử tối thượng.", "Hạ Oai Tử.")
m(6304, "Đấu trường gà (AH) — Trọng Tài Đen", "Trọng Tài Đen tối thượng — kết thúc đại hội.", "Hạ Trọng Tài Đen.")

# ---- Rescue chicken ----
m(7001, "Giải cứu gà con — Đàn nhiễm độc", "Nông trại yên bình bị gà mái xấu xí phá tan. Đàn gà con nhiễm độc — hạ chúng để cứu nông trại.", "Hạ gà con nhiễm độc.")
m(7002, "Giải cứu gà con — Gà mái", "Gà mái xuất hiện sau đàn con. Đánh bại nó để mở đường tới chuồng.", "Đánh bại gà mái.")
m(7004, "Giải cứu gà con — Phá lồng", "Gà lương thiện bị nhốt. Phá hủy lồng để giải cứu chúng.", "Phá hủy lồng gà.")
m(7101, "Giải cứu gà con (Thường) — Đàn nhiễm độc", "Độ thường: nhiều gà con nhiễm độc hơn.", "Hạ gà con nhiễm độc.")
m(7102, "Giải cứu gà con (Thường) — Gà mái", "Gà mái mạnh hơn.", "Đánh bại gà mái.")
m(7103, "Giải cứu gà con (Thường) — Chuồng gà", "Chuồng gà biến dị chặn lối. Hạ chuồng để tới lồng.", "Hạ chuồng gà.")
m(7104, "Giải cứu gà con (Thường) — Phá lồng", "Phá lồng — giải cứu gà lương thiện.", "Phá hủy lồng gà.")
m(7201, "Giải cứu gà con (Khó) — Đàn nhiễm độc", "Độ khó: biển gà con nhiễm độc.", "Hạ gà con nhiễm độc.")
m(7202, "Giải cứu gà con (Khó) — Gà mái", "Gà mái điên loạn.", "Đánh bại gà mái.")
m(7203, "Giải cứu gà con (Khó) — Chuồng gà", "Chuồng gà kiên cố.", "Hạ chuồng gà.")
m(7204, "Giải cứu gà con (Khó) — Phá lồng", "Phá lồng dưới áp lực cao.", "Phá hủy lồng gà.")

for base, label in [(7400, "Ác mộng Dễ"), (7500, "Ác mộng Thường"), (7600, "Ác mộng Khó"), (7700, "Ác mộng Anh hùng")]:
    m(base + 1, f"Giải cứu gà con ({label}) — Đàn nhiễm độc", f"{label}: bóng tối nuốt nông trại — đàn nhiễm độc hung hãn.", "Hạ gà con nhiễm độc.")
    m(base + 2, f"Giải cứu gà con ({label}) — Gà mái", f"{label}: gà mái trong ác mộng.", "Đánh bại gà mái.")
    m(base + 3, f"Giải cứu gà con ({label}) — Chuồng gà", f"{label}: chuồng gà biến dị.", "Hạ chuồng gà.")
    m(base + 4, f"Giải cứu gà con ({label}) — Phá lồng", f"{label}: phá lồng để kết thúc ác mộng.", "Phá hủy lồng gà.")

# ---- Time vortex ----
def tv(ids, tag):
    m(ids[0], f"Vòng xoáy thời gian ({tag}) — Bão thời không", f"{tag}: bước vào vòng xoáy — hạ hết kẻ địch bị kéo theo dòng thời gian.", "Đánh bại toàn bộ kẻ địch.")
    m(ids[1], f"Vòng xoáy thời gian ({tag}) — Thuyền trưởng Cá sấu", f"{tag}: Thuyền trưởng Cá sấu chặn dòng chảy thời gian.", "Đánh bại Thuyền trưởng Cá sấu.")
    m(ids[2], f"Vòng xoáy thời gian ({tag}) — Hồi phục Gà", f"{tag}: gà bị thương trong vòng xoáy — hồi phục chúng trước khi quá muộn.", "Hồi phục Gà.")
    m(ids[3], f"Vòng xoáy thời gian ({tag}) — Bác học Vẹt", f"{tag}: Bác học Vẹt — kẻ gây biến dị gà con — xuất hiện ở cuối dòng thời gian.", "Đánh bại Bác học Vẹt.")

tv([12001, 12002, 12003, 12004], "Dễ")
tv([12101, 12102, 12103, 12104], "Thường")
tv([12201, 12202, 12203, 12204], "Khó")
tv([12301, 12302, 12303, 12304], "Anh hùng")
m(12016, "Tàu Hải Tặc — Hải Tặc Trưởng", "Tàu hải tặc tàng trữ kho báu. Hải Tặc Trưởng không dễ buông tha — hạ hắn để lấy kho báu.", "Hạ Hải Tặc Trưởng.")

# ---- Warriors arena ----
def wa(ids, tag):
    m(ids[0], f"Đấu trường Dũng Sĩ ({tag}) — Mở màn", f"{tag}: đấu trường cổ xưa thức tỉnh — hạ vệ binh mở màn.", "Hạ vệ binh đấu trường.")
    m(ids[1], f"Đấu trường Dũng Sĩ ({tag}) — Sulan", f"{tag}: Sulan bước vào đấu trường. Đánh bại cô ấy để tiếp tục.", "Đánh bại Sulan.")
    m(ids[2], f"Đấu trường Dũng Sĩ ({tag}) — Minotaure", f"{tag}: Minotaure gầm vang. Trận boss trước chung kết.", "Đánh bại Minotaure.")
    m(ids[3], f"Đấu trường Dũng Sĩ ({tag}) — Chung kết", f"{tag}: trận chung kết đấu trường — sống sót và hạ hết địch.", "Hạ hết địch trong chung kết.")

wa([13001, 13002, 13003, 13004], "Dễ")
wa([13101, 13102, 13103, 13104], "Thường")
wa([13201, 13202, 13203, 13204], "Khó")
wa([13301, 13302, 13303, 13304], "Anh hùng")

# ---- Misc / bosses ----
m(15002, "Boss vợ chồng", "Cặp đôi boss khiêu chiến — đánh bại đối thủ để chứng tỏ tình yêu chiến đấu.", "Đánh bại đối thủ.")
m(30001, "Bá tước hút máu — Dạng 1", "Bá tước hút máu thức tỉnh trong lâu đài. Tiêu diệt dạng đầu tiên.", "Hạ Bá tước hút máu.")
m(30002, "Bá tước hút máu — Dạng 2", "Bá tước chưa chết — dạng mạnh hơn xuất hiện.", "Hạ Bá tước hút máu.")
m(30004, "Đội trưởng (World Soccer)", "Đội trưởng bước ra sân — hạ hắn trong trận đấu đặc biệt.", "Hạ Đội trưởng.")
m(4002, "Ải gà phụ — Gà con", "Ải phụ: hạ 16 gà con.", "Hạ 16 gà con.")
m(4003, "Ải gà phụ — Gà mái", "Ải phụ: hạ 3 gà mái.", "Hạ 3 gà mái.")
m(71059, "Lâu đài Phép thuật — Ải 1", "Lâu đài phép thuật mở cửa. Hạ hết địch canh giữ hành lang ma pháp.", "Hạ tất cả địch.")
m(71060, "Lâu đài Phép thuật — Thú Bảo Vệ", "Thú Bảo Vệ Ma Pháp chặn lõi lâu đài. Hạ nó để kết thúc.", "Hạ Thú Bảo Vệ Ma Pháp.")

# Training leftover mission names if present
for mid, title, tip in [
    (1085, "Huấn luyện mới 1", "Bài huấn luyện cơ bản 1."),
    (1086, "Huấn luyện mới 2", "Bài huấn luyện cơ bản 2."),
    (1087, "Huấn luyện mới 3", "Bài huấn luyện cơ bản 3."),
    (1088, "Huấn luyện mới 4", "Bài huấn luyện cơ bản 4."),
    (1089, "Huấn luyện mới 5", "Bài huấn luyện cơ bản 5."),
]:
    m(mid, title, tip, "Hoàn thành bài huấn luyện.")

PVE = {
    1: ("Lâu đài Gà",
        "Lâu đài Gà　 Số người: 2-4\n\n"
        "Tại đảo Porti màu mỡ từng có tộc Gà hiền hòa. Ba năm trước một đế quốc hung hăng xâm lược. "
        "Quốc Vương biến dị — các dũng sĩ phải xông vào cổng, hạ Baroff, rồi đánh bại Quốc Vương "
        "(và Matthias ở độ khó cao) để giải phóng vương quốc."),
    2: ("Huyệt Ma Kiến",
        "Huyệt Ma Kiến　 Số người: 2-4\n\n"
        "Ải viễn chinh là đầu mối thương mại. Gần đây thương nhân vào rừng ma pháp đều bị cướp. "
        "Hội trưởng nhờ các dũng sĩ lần theo dấu vết tới Huyệt Ma Kiến: quét kiến lính ở cửa hang, "
        "rồi đánh bại Nữ Vương Daniliz để thông tuyến đường."),
    3: ("Bộ lạc Tà thần",
        "Bộ lạc Tà thần　 Số người: 2-4\n\n"
        "Người quản lý cầu cảng rất thích đồ cổ. Trong một trận động đất bình gốm vỡ ra lộ bản đồ bộ lạc. "
        "Các dũng sĩ tiến vào làng: hạ dũng sĩ cổng → hồi sinh đậu thần → hạ anh em Bulan → "
        "đánh bại Chúa Tể Rừng Xanh."),
    4: ("Pháo đài hắc ám",
        "Pháo đài hắc ám　 Số người: 2-4\n\n"
        "Cuộc phiêu lưu mới bắt đầu. Phá cửa thành → hạ Sói Tinh & Chim Ưng → đánh bại Minotaur "
        "để giải phóng pháo đài khỏi bóng tối."),
    5: ("Đại chiến rồng",
        "Đại chiến rồng　 Đề nghị: 2-4 người\n\n"
        "Sau khi đánh bại kẻ trong bóng tối, các dũng sĩ thấy ngọn núi lửa giữa đảo. "
        "Quỷ Pochi → Pochi tái sinh → Quỷ Khăn Hồng → Tà Diệm Long. Mỗi ải một trận boss."),
    6: ("Đấu trường gà",
        "Đấu trường gà　 Số người: 3-4\n\n"
        "Để ăn mừng chiến thắng, Quốc Vương tổ chức đại hội thể thao. "
        "Vòng điểm → đua về đích → Oai Tử → Trọng Tài Đen. Em họ Garce cũng tham dự."),
    7: ("Giải cứu gà con",
        "Giải cứu gà con　 Số người: 2-4\n\n"
        "Nông trại bình yên bị một con gà mái xấu xí phá tan. Gà con nhiễm độc — "
        "hạ đàn nhiễm độc → gà mái → chuồng → phá lồng để cứu gà lương thiện."),
    8: ("Rừng rậm Gamazon",
        "Rừng rậm Gamazon　 Sư đồ — Số người: 2-4\n\n"
        "Từ khi ải viễn chinh mở cửa, Bá tước huấn luyện dũng sĩ tại Gamazon. "
        "Quét Người lựu đạn trên đường mòn, rồi hạ Chiến sĩ tà thần."),
    9: ("Hoang mạc Xatara",
        "Hoang mạc Xatara　 Sư đồ — Số người: 2-4\n\n"
        "Khóa huấn luyện trên cát nóng. Hạ mai phục Người lựu đạn, rồi đánh bại Vua Bò Cạp."),
    10: ("Lâu đài Daucula",
        "Lâu đài Daucula　 Sư đồ — Số người: 2-4\n\n"
        "Huấn luyện trong lâu đài tối. Quét tạo vật ở hành lang, rồi hạ Franken trong phòng thí nghiệm."),
    11: ("Vịnh Cariga",
        "Vịnh Cariga　 Sư đồ — Số người: 2-4\n\n"
        "Huấn luyện ven biển. Quét quái bãi đáp, rồi hạ thủ lĩnh hang đá."),
    12: ("Vòng xoáy thời gian",
        "Vòng xoáy thời gian　 Số người: 2-4\n\n"
        "Biến dị gà con do Bác học Vẹt gây ra. Bước vào vòng xoáy: bão thời không → "
        "Thuyền trưởng Cá sấu → hồi phục Gà → đánh bại Bác học Vẹt."),
    13: ("Đấu Trường Dũng Sĩ",
        "Đấu Trường Dũng Sĩ　 Yêu cầu: 2-4 người\n\n"
        "Đấu trường cổ trên Main Lander bị tà khí bao phủ. Mở màn → Sulan → Minotaure → chung kết "
        "để điều tra nguồn gốc tiếng thét rợn người."),
    14: ("Tàu Hải Tặc",
        "Tàu Hải Tặc\n\n"
        "Tàu hải tặc tàng trữ nhiều kho báu. Chỉ dũng sĩ mới có thể hạ Hải Tặc Trưởng và sở hữu kho báu "
        "(vé vào qua hoạt động trong game)."),
    38: ("Huyệt Ma Kiến Ác Mộng",
         "Huyệt Ma Kiến Ác Mộng　 Số người: 2-4\n\n"
         "Phiên bản ác mộng của Huyệt Ma Kiến. Bóng tối nuốt hang — quét kiến cửa hang rồi hạ Daniliz biến dị."),
    39: ("Giải Cứu Gà Con Ác Mộng",
         "Giải Cứu Gà Con Ác Mộng　 Số người: 2-4\n\n"
         "Nông trại trong ác mộng. Đàn nhiễm độc → gà mái → chuồng → phá lồng dưới bóng tối."),
    1243: ("Rồng Cổ",
           "Rồng Cổ\n\nSinh vật ngàn năm trên đảo thức tỉnh. Một trận boss quyết chiến."),
    30001: ("Bá tước hút máu", "Bá tước hút máu thức tỉnh — tiêu diệt dạng đầu."),
    30002: ("Bá tước hút máu", "Bá tước dạng mạnh hơn — kết thúc hắn."),
    30004: ("Đội trưởng", "Trận đấu đặc biệt với Đội trưởng."),
}

# Intros injected into mission C# OnStartGame (class_name -> message)
INTROS = {
    "BLTTT1122": "Bản đồ bộ lạc dẫn các ngươi tới cổng làng. Dũng sĩ bộ lạc tràn ra — hạ họ để vào sâu!",
    "BLTTT1123": "Đậu thần khô héo giữa làng. Hồi sinh nó để mở nghi thức!",
    "BLTTT1124": "Anh em Bulan bảo vệ thánh địa — hạ cả hai!",
    "BLTTT1126": "Chúa Tể Rừng Xanh thức tỉnh! Trận quyết chiến bộ lạc bắt đầu!",
    "BLTTK1122": "Độ khó tăng — dũng sĩ bộ lạc đông hơn chặn cổng!",
    "BLTTK1123": "Đậu thần bị nguyền. Mau hồi sinh!",
    "BLTTK1124": "Bulan kích nộ — không được lơ là!",
    "BLTTK1126": "Chúa Tể Rừng Xanh hình thái khó!",
    "BLTTAH1122": "Anh hùng: cổng làng là biển người!",
    "BLTTAH1123": "Nghi thức đậu thần dưới lửa địa ngục!",
    "BLTTAH1124": "Bulan tối thượng xuất trận!",
    "BLTTAH1126": "Chúa Tể Rừng Xanh tối thượng — kết thúc bộ lạc!",
    "PDHAT1142": "Pháo đài hắc ám trước mắt. Phá vỡ cửa thành!",
    "PDHAT1143": "Sói Tinh và Chim Ưng phối hợp — hạ cả hai!",
    "PDHAT1144": "Minotaur canh lõi pháo đài. Đánh bại nó!",
    "PDHAK1142": "Cửa thành kiên cố hơn — phá bằng sức mạnh!",
    "PDHAK1143": "Sói và Ưng điên cuồng!",
    "PDHAK1144": "Minotaur hình thái khó!",
    "PDHAAH1142": "Anh hùng: phá cửa thành bất khả xâm phạm!",
    "PDHAAH1143": "Song thủ hộ vệ tối thượng!",
    "PDHAAH1144": "Minotaur tối thượng — giải phóng pháo đài!",
    "CBS600": "Vịnh Cariga — quét quái bãi đáp để vào hang đá!",
    "CBS601": "Thủ lĩnh vịnh xuất hiện. Kết thúc khóa Cariga!",
    "DTS700": "Lâu đài Daucula tối om. Quét tạo vật hành lang!",
    "DTS701": "Franken thức tỉnh trong phòng thí nghiệm!",
    "GZS800": "Rừng Gamazon. Người lựu đạn chặn đường mòn!",
    "GZS801": "Chiến sĩ tà thần xuất hiện sâu rừng!",
    "XDS900": "Hoang mạc Xatara. Mai phục trên cồn cát!",
    "XDS901": "Vua Bò Cạp trồi lên từ cát!",
    "DTGT1165": "Đại hội thể thao bắt đầu — đạt 99 điểm!",
    "DTGT1166": "Cuộc đua: đội đỏ phải về đích trước!",
    "DTGT1167": "Oai Tử khiêu chiến trên đấu trường!",
    "DTGT1168": "Trọng Tài Đen thao túng luật — hạ hắn!",
    "DTGK1165": "Độ khó: vòng điểm khắc nghiệt hơn!",
    "DTGK1166": "Cuộc đua sinh tử!",
    "DTGK1167": "Oai Tử điên cuồng!",
    "DTGK1168": "Trọng Tài Đen hình thái khó!",
    "DTGAH1165": "Anh hùng: đạt 99 điểm!",
    "DTGAH1166": "Một sai sót là thua cuộc đua!",
    "DTGAH1167": "Oai Tử tối thượng!",
    "DTGAH1168": "Trọng Tài Đen tối thượng!",
    "GCGCD1161": "Nông trại bị phá — hạ đàn gà con nhiễm độc!",
    "GCGCD1162": "Gà mái xuất hiện sau đàn con!",
    "GCGCD1164": "Phá lồng — giải cứu gà lương thiện!",
    "GCGCT1161": "Độ thường: nhiều gà con nhiễm độc hơn!",
    "GCGCT1162": "Gà mái mạnh hơn!",
    "GCGCT1163": "Chuồng gà biến dị chặn lối!",
    "GCGCT1164": "Phá lồng để cứu gà!",
    "GCGCK1161": "Độ khó: biển gà con nhiễm độc!",
    "GCGCK1162": "Gà mái điên loạn!",
    "GCGCK1163": "Chuồng gà kiên cố!",
    "GCGCK1164": "Phá lồng dưới áp lực cao!",
    "TVS12001": "Bước vào vòng xoáy — hạ hết kẻ địch thời không!",
    "TVS12002": "Thuyền trưởng Cá sấu chặn dòng chảy!",
    "TVS12003": "Hồi phục những chú Gà bị thương!",
    "TVS12004": "Bác học Vẹt — thủ phạm biến dị — xuất hiện!",
    "TVN12001": "Vòng xoáy độ thường — bão thời không!",
    "TVN12002": "Thuyền trưởng Cá sấu mạnh hơn!",
    "TVN12003": "Mau hồi phục Gà!",
    "TVN12004": "Bác học Vẹt độ thường!",
    "TVH12001": "Vòng xoáy khó — không được sơ suất!",
    "TVH12002": "Cá sấu điên cuồng!",
    "TVH12003": "Hồi phục Gà dưới áp lực!",
    "TVH12004": "Bác học Vẹt hình thái khó!",
    "TVT12001": "Anh hùng: bão thời không tối thượng!",
    "TVT12002": "Cá sấu tối thượng!",
    "TVT12003": "Hồi phục Gà — anh hùng!",
    "TVT12004": "Bác học Vẹt tối thượng — kết thúc vòng xoáy!",
    "WAS13001": "Đấu trường cổ thức tỉnh — hạ vệ binh mở màn!",
    "WAS13002": "Sulan bước vào đấu trường!",
    "WAS13003": "Minotaure gầm vang!",
    "WAS13004": "Chung kết đấu trường — sống sót!",
    "WAN13101": "Đấu trường độ thường — mở màn!",
    "WAN13102": "Sulan mạnh hơn!",
    "WAN13103": "Minotaure độ thường!",
    "WAN13104": "Chung kết độ thường!",
    "WAH13201": "Đấu trường khó — mở màn!",
    "WAH13202": "Sulan điên cuồng!",
    "WAH13203": "Minotaure hình thái khó!",
    "WAH13204": "Chung kết khó!",
    "WAT13301": "Anh hùng: mở màn đấu trường!",
    "WAT13302": "Sulan tối thượng!",
    "WAT13303": "Minotaure tối thượng!",
    "WAT13304": "Chung kết anh hùng!",
    "CrosairBoss": "Tàu hải tặc — hạ Hải Tặc Trưởng để lấy kho báu!",
    "AC30001": "Bá tước hút máu thức tỉnh — dạng đầu!",
    "AC30002": "Bá tước dạng mạnh hơn xuất hiện!",
    "AC30004": "Đội trưởng bước ra sân!",
    "DCNM2401": "Ác mộng: cửa hang kiến tối om — quét sạch!",
    "DCNM2402": "Ác mộng: Daniliz biến dị!",
    "DCNM2501": "Ác mộng thường — cửa hang dày đặc!",
    "DCNM2502": "Ác mộng thường — Nữ Vương!",
    "DCNM2601": "Ác mộng khó — cửa hang máu!",
    "DCNM2602": "Ác mộng khó — Daniliz!",
    "DCNM2701": "Ác mộng anh hùng — cửa hang!",
    "DCNM2702": "Ác mộng anh hùng — Daniliz tối thượng!",
}

# Nightmare chicken intros
for prefix, label in [("RRCNM74", "Ác mộng dễ"), ("RRCNM75", "Ác mộng thường"), ("RRCNM76", "Ác mộng khó"), ("RRCNM77", "Ác mộng anh hùng")]:
    for suf, msg in [("01", "đàn nhiễm độc"), ("02", "gà mái"), ("03", "chuồng gà"), ("04", "phá lồng")]:
        INTROS[f"{prefix}{suf}"] = f"Giải cứu gà con ({label}) — {msg}!"


def sql_escape(s: str) -> str:
    return s.replace("'", "''")


def emit_sql() -> str:
    lines = ["SET NOCOUNT ON;"]
    for id_, name, desc, ok in MISSIONS:
        lines.append(
            f"UPDATE Mission_Info SET Name=N'{sql_escape(name)}', Description=N'{sql_escape(desc)}', "
            f"Success=N'{sql_escape(ok)}' WHERE Id={id_};"
        )
    for pid, (name, desc) in PVE.items():
        lines.append(
            f"UPDATE Pve_Info SET Name=N'{sql_escape(name)}', Description=N'{sql_escape(desc)}' WHERE ID={pid};"
        )
    return "\n".join(lines)


def patch_mission_intros(missions_dir: Path) -> int:
    patched = 0
    for cls, msg in INTROS.items():
        path = missions_dir / f"{cls}.cs"
        if not path.exists():
            continue
        text = path.read_text(encoding="utf-8")
        safe = msg.replace("\\", "\\\\").replace('"', '\\"')
        if safe in text:
            continue
        m = re.search(r"(public override void OnStartGame\(\)\s*\{[\s\S]*?base\.OnStartGame\(\);\s*)", text)
        if not m:
            continue
        # already has an intro nearby
        if "SendMessage" in text[m.end() : m.end() + 500]:
            continue
        insert = (
            "\n            foreach (Player __p in Game.GetAllFightPlayers())\n"
            "            {\n"
            f'                __p.PlayerDetail.SendMessage("{safe}");\n'
            "            }\n"
        )
        if "using Game.Logic.Phy.Object;" not in text:
            if "using Game.Logic;" in text:
                text = text.replace("using Game.Logic;", "using Game.Logic;\nusing Game.Logic.Phy.Object;", 1)
            else:
                text = "using Game.Logic.Phy.Object;\n" + text
        text = text[: m.end()] + insert + text[m.end() :]
        if "IsBossWar" not in text and "OnPrepareNewSession" in text:
            title = msg[:48].replace('"', "")
            text2, nsub = re.subn(
                r"(Game\.SetMap\([^)]+\);\s*)",
                rf'\1\n            Game.IsBossWar = "{title}";\n',
                text,
                count=1,
            )
            if nsub:
                text = text2
            else:
                text = re.sub(
                    r"(public override void OnPrepareNewSession\(\)\s*\{)",
                    rf'\1\n            Game.IsBossWar = "{title}";',
                    text,
                    count=1,
                )
        path.write_text(text, encoding="utf-8")
        patched += 1
    return patched


def main() -> None:
    root = Path("/Users/it/Documents/Gunny")
    sql_path = root / "tools" / "full_pve_stories.sql"
    sql_path.write_text(emit_sql(), encoding="utf-8")
    print(f"missions={len(MISSIONS)} pve={len(PVE)} sql={sql_path}")
    n = patch_mission_intros(root / "GameServerScripts" / "AI" / "Messions")
    print(f"patched_intros={n}")


if __name__ == "__main__":
    main()
