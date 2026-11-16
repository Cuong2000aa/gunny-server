import os, shutil, urllib.request, subprocess

def run_sql(query):
    cmd = ['docker', 'exec', '-i', 'gunny-sql', '/opt/mssql-tools/bin/sqlcmd', '-S', 'localhost', '-U', 'sa', '-P', 'Gunny@2026Mac', '-d', 'Project_Game34', '-s', '|', '-W']
    res = subprocess.run(cmd, input=query.encode('utf-8'), capture_output=True)
    return res.stdout.decode('utf-8', errors='ignore')

# 1. Download map images for 1241, 1242, 1243, 1244
headers = {'User-Agent': 'Mozilla/5.0'}
map_assets = {
    1241: ['back.jpg', 'dead.png'],
    1242: ['back.jpg', 'fore.png', 'samll_map.png'],
    1243: ['back.jpg', 'dead.png', 'samll_map.png'],
    1244: ['back.jpg', 'dead.png']
}

for m, files in map_assets.items():
    mdir = f'Resources/image/map/{m}'
    os.makedirs(mdir, exist_ok=True)
    for f in files:
        dst = os.path.join(mdir, f)
        if not os.path.exists(dst):
            url = f'https://gunny.vcdn.vn/image/map/{m}/{f}'
            try:
                req = urllib.request.Request(url, headers=headers)
                with urllib.request.urlopen(req, timeout=5) as resp:
                    with open(dst, 'wb') as out_f:
                        out_f.write(resp.read())
                print(f"Downloaded {m}/{f}")
            except Exception as e:
                print(f"Failed {m}/{f}: {e}")

# If samll_map.png is missing for 1241 or 1244, use 1243's samll_map
sample_small = 'Resources/image/map/1243/samll_map.png'
if os.path.exists(sample_small):
    for m in [1241, 1244]:
        target = f'Resources/image/map/{m}/samll_map.png'
        if not os.path.exists(target):
            shutil.copy(sample_small, target)
            print(f"Copied sample samll_map to {m}")

# 2. Map copies mapping:
# 5101, 5201, 5301 -> 1241
# 5102, 5202, 5302 -> 1242
# 5103, 5203, 5303 -> 1243
# 5104, 5204, 5304 -> 1244
stage_mapping = {
    1241: [5101, 5201, 5301],
    1242: [5102, 5202, 5302],
    1243: [5103, 5203, 5303],
    1244: [5104, 5204, 5304]
}

# Copy client map images
for src_m, targets in stage_mapping.items():
    src_dir = f'Resources/image/map/{src_m}'
    for tgt in targets:
        tgt_dir = f'Resources/image/map/{tgt}'
        os.makedirs(tgt_dir, exist_ok=True)
        for f in os.listdir(src_dir):
            shutil.copy(os.path.join(src_dir, f), os.path.join(tgt_dir, f))
print("Copied all client map images for 510x, 520x, 530x.")

# Copy collision map files across all 4 map directories
map_dirs = [
    'Fighting.Service/bin/Release/net8.0/map',
    'Fighting.Service/map',
    'Road.Service/map',
    'Road.Service/bin/Release/net8.0/map'
]

for mdir in map_dirs:
    os.makedirs(mdir, exist_ok=True)
    for src_m, targets in stage_mapping.items():
        src_path = os.path.join('Fighting.Service/bin/Release/net8.0/map', str(src_m))
        if os.path.exists(src_path):
            local_src = os.path.join(mdir, str(src_m))
            os.makedirs(local_src, exist_ok=True)
            for f in os.listdir(src_path):
                sfile = os.path.join(src_path, f)
                dfile = os.path.join(local_src, f)
                if os.path.abspath(sfile) != os.path.abspath(dfile):
                    shutil.copy(sfile, dfile)
            for tgt in targets:
                tgt_path = os.path.join(mdir, str(tgt))
                os.makedirs(tgt_path, exist_ok=True)
                for f in os.listdir(src_path):
                    shutil.copy(os.path.join(src_path, f), os.path.join(tgt_path, f))

print("Copied all collision maps to all service directories.")

# 3. Update SQL Game_Map
sql_map = """
-- 1241, 5101, 5201, 5301
MERGE INTO Game_Map AS target
USING (VALUES 
    (1241, N'Hang núi lửa', '300,880|400,880|500,860|600,840', '300,880|400,880|500,860|600,840', 1797, 1097, 1100, 878, 1797, 1097, '', 'back', 'dead', 0, 0, 10, 2),
    (5101, N'Hang núi lửa Ải 1', '300,880|400,880|500,860|600,840', '300,880|400,880|500,860|600,840', 1797, 1097, 1100, 878, 1797, 1097, '', 'back', 'dead', 0, 0, 10, 2),
    (5201, N'Hang núi lửa Ải 1 (Khó)', '300,880|400,880|500,860|600,840', '300,880|400,880|500,860|600,840', 1797, 1097, 1100, 878, 1797, 1097, '', 'back', 'dead', 0, 0, 10, 2),
    (5301, N'Hang núi lửa Ải 1 (Anh hùng)', '300,880|400,880|500,860|600,840', '300,880|400,880|500,860|600,840', 1797, 1097, 1100, 878, 1797, 1097, '', 'back', 'dead', 0, 0, 10, 2),

    (1242, N'Dung nham', '300,520|350,550|400,580|450,600', '300,520|350,550|400,580|450,600', 2454, 1616, 1100, 700, 2454, 1616, 'fore', 'back', '', 0, 0, 10, 2),
    (5102, N'Dung nham Ải 2', '300,520|350,550|400,580|450,600', '300,520|350,550|400,580|450,600', 2454, 1616, 1100, 700, 2454, 1616, 'fore', 'back', '', 0, 0, 10, 2),
    (5202, N'Dung nham Ải 2 (Khó)', '300,520|350,550|400,580|450,600', '300,520|350,550|400,580|450,600', 2454, 1616, 1100, 700, 2454, 1616, 'fore', 'back', '', 0, 0, 10, 2),
    (5302, N'Dung nham Ải 2 (Anh hùng)', '300,520|350,550|400,580|450,600', '300,520|350,550|400,580|450,600', 2454, 1616, 1100, 700, 2454, 1616, 'fore', 'back', '', 0, 0, 10, 2),

    (1243, N'Cánh cửa rồng', '428,542|643,543|500,542|580,543', '428,542|643,543|500,542|580,543', 1362, 829, 1100, 702, 1362, 829, '', 'back', 'dead', 0, 0, 10, 2),
    (5103, N'Cánh cửa rồng Ải 3', '428,542|643,543|500,542|580,543', '428,542|643,543|500,542|580,543', 1362, 829, 1100, 702, 1362, 829, '', 'back', 'dead', 0, 0, 10, 2),
    (5203, N'Cánh cửa rồng Ải 3 (Khó)', '428,542|643,543|500,542|580,543', '428,542|643,543|500,542|580,543', 1362, 829, 1100, 702, 1362, 829, '', 'back', 'dead', 0, 0, 10, 2),
    (5303, N'Cánh cửa rồng Ải 3 (Anh hùng)', '428,542|643,543|500,542|580,543', '428,542|643,543|500,542|580,543', 1362, 829, 1100, 702, 1362, 829, '', 'back', 'dead', 0, 0, 10, 2),

    (1244, N'Hang Rồng Tà Diệm Long', '300,940|400,950|500,940|600,960', '300,940|400,950|500,940|600,960', 1600, 1152, 1100, 936, 1600, 1152, '', 'back', 'dead', 0, 0, 10, 2),
    (5104, N'Hang Rồng Ải 4', '300,940|400,950|500,940|600,960', '300,940|400,950|500,940|600,960', 1600, 1152, 1100, 936, 1600, 1152, '', 'back', 'dead', 0, 0, 10, 2),
    (5204, N'Hang Rồng Ải 4 (Khó)', '300,940|400,950|500,940|600,960', '300,940|400,950|500,940|600,960', 1600, 1152, 1100, 936, 1600, 1152, '', 'back', 'dead', 0, 0, 10, 2),
    (5304, N'Hang Rồng Ải 4 (Anh hùng)', '300,940|400,950|500,940|600,960', '300,940|400,950|500,940|600,960', 1600, 1152, 1100, 936, 1600, 1152, '', 'back', 'dead', 0, 0, 10, 2)
) AS source (ID, Name, PosX, PosX1, ForegroundWidth, ForegroundHeight, BackroundWidht, BackroundHeight, DeadWidth, DeadHeight, ForePic, BackPic, DeadPic, Weight, DragIndex, Type, Remark)
ON target.ID = source.ID
WHEN MATCHED THEN
    UPDATE SET 
        Name = source.Name,
        PosX = source.PosX,
        PosX1 = source.PosX1,
        ForegroundWidth = source.ForegroundWidth,
        ForegroundHeight = source.ForegroundHeight,
        BackroundWidht = source.BackroundWidht,
        BackroundHeight = source.BackroundHeight,
        DeadWidth = source.DeadWidth,
        DeadHeight = source.DeadHeight,
        ForePic = source.ForePic,
        BackPic = source.BackPic,
        DeadPic = source.DeadPic
WHEN NOT MATCHED THEN
    INSERT (ID, Name, PosX, PosX1, Description, ForegroundWidth, ForegroundHeight, BackroundWidht, BackroundHeight, DeadWidth, DeadHeight, Weight, DragIndex, ForePic, BackPic, DeadPic, Pic, Remark, BackMusic, Type)
    VALUES (source.ID, source.Name, source.PosX, source.PosX1, source.Name, source.ForegroundWidth, source.ForegroundHeight, source.BackroundWidht, source.BackroundHeight, source.DeadWidth, source.DeadHeight, source.Weight, source.DragIndex, source.ForePic, source.BackPic, source.DeadPic, '', '2', '1241', source.Type);
"""

res_m = run_sql(sql_map)
print("Updated Game_Map in SQL:", res_m)

# 4. Also fix Pve_Info for PVE 4, 6, 14
sql_pve_fixes = """
-- Pháo đài hắc ám (ID 4): Set Simple to Normal if empty
UPDATE Pve_Info SET SimpleGameScript = NormalGameScript, SimpleTemplateIds = NormalTemplateIds WHERE ID = 4 AND (SimpleGameScript IS NULL OR SimpleGameScript = '');

-- Đấu trường gà (ID 6): Set Simple to Normal if empty
UPDATE Pve_Info SET SimpleGameScript = NormalGameScript, SimpleTemplateIds = NormalTemplateIds WHERE ID = 6 AND (SimpleGameScript IS NULL OR SimpleGameScript = '');

-- Tàu hải tặc (ID 14): Set Normal to Simple if empty
UPDATE Pve_Info SET NormalGameScript = SimpleGameScript, NormalTemplateIds = SimpleTemplateIds WHERE ID = 14 AND (NormalGameScript IS NULL OR NormalGameScript = '');
"""
run_sql(sql_pve_fixes)
print("Applied Pve_Info fixes for 4, 6, 14.")
