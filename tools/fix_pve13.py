import os, shutil, urllib.request, struct, subprocess
from PIL import Image
import io

def run_sql(query):
    cmd = ['docker', 'exec', '-i', 'gunny-sql', '/opt/mssql-tools/bin/sqlcmd', '-S', 'localhost', '-U', 'sa', '-P', 'Gunny@2026Mac', '-d', 'Project_Game34', '-s', '|', '-W']
    res = subprocess.run(cmd, input=query.encode('utf-8'), capture_output=True)
    return res.stdout.decode('utf-8', errors='ignore')

# 1. Download images for 13001-13004
headers = {'User-Agent': 'Mozilla/5.0'}
sample_small = 'Resources/image/map/1243/samll_map.png'

for m in [13001, 13002, 13003, 13004]:
    mdir = f'Resources/image/map/{m}'
    os.makedirs(mdir, exist_ok=True)
    for f in ['back.jpg', 'dead.png']:
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
    # small map
    dst_small = os.path.join(mdir, 'samll_map.png')
    if not os.path.exists(dst_small) and os.path.exists(sample_small):
        shutil.copy(sample_small, dst_small)

# 2. Build dead.map for 13004
img_13004 = Image.open('Resources/image/map/13004/dead.png').convert('RGBA')
w, h = img_13004.size
bw = w // 8 + 1
data = bytearray(bw * h)
for j in range(h):
    for i in range(w):
        r, g, b, a = img_13004.getpixel((i, j))
        flag = 0 if a <= 100 else 1
        data[j * bw + i // 8] |= (flag << (7 - (i % 8)))

# Save to all 4 directories
map_dirs = [
    'Fighting.Service/bin/Release/net8.0/map',
    'Fighting.Service/map',
    'Road.Service/map',
    'Road.Service/bin/Release/net8.0/map'
]

for mdir in map_dirs:
    os.makedirs(f'{mdir}/13004', exist_ok=True)
    with open(f'{mdir}/13004/dead.map', 'wb') as f:
        f.write(struct.pack('<ii', w, h))
        f.write(data)
    
    # Also sync 13001, 13002, 13003
    for m in [13001, 13002, 13003]:
        src = f'Fighting.Service/bin/Release/net8.0/map/{m}'
        dst = f'{mdir}/{m}'
        if os.path.exists(src) and os.path.abspath(src) != os.path.abspath(dst):
            os.makedirs(dst, exist_ok=True)
            for fn in os.listdir(src):
                shutil.copy(os.path.join(src, fn), os.path.join(dst, fn))

print("Created 13004 dead.map and synced 13001-13004 collision maps.")

# 3. Update SQL Game_Map
sql = """
MERGE INTO Game_Map AS target
USING (VALUES
    (13001, N'Đấu trường Mở màn', '300,540|400,510|500,530|600,520', '300,540|400,510|500,530|600,520', 1248, 800, 1100, 700, 1248, 800, '', 'back', 'dead', 10, 2),
    (13002, N'Đấu trường Sulan', '300,600|400,600|500,610|600,600', '300,600|400,600|500,610|600,600', 1320, 800, 1100, 700, 1320, 800, '', 'back', 'dead', 10, 2),
    (13003, N'Đấu trường Minotaure', '300,560|400,560|500,570|600,600', '300,560|400,560|500,570|600,600', 1320, 800, 1100, 700, 1320, 800, '', 'back', 'dead', 10, 2),
    (13004, N'Đấu trường Mathias', '500,940|550,940|600,940|650,940', '500,940|550,940|600,940|650,940', 1132, 1055, 1100, 700, 1132, 1055, '', 'back', 'dead', 10, 2)
) AS source (ID, Name, PosX, PosX1, ForegroundWidth, ForegroundHeight, BackroundWidht, BackroundHeight, DeadWidth, DeadHeight, ForePic, BackPic, DeadPic, Type, Remark)
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
    VALUES (source.ID, source.Name, source.PosX, source.PosX1, source.Name, source.ForegroundWidth, source.ForegroundHeight, source.BackroundWidht, source.BackroundHeight, source.DeadWidth, source.DeadHeight, 0, 0, source.ForePic, source.BackPic, source.DeadPic, '', '2', '13001', source.Type);
"""
run_sql(sql)
print("PVE 13 (Đấu Trường Dũng Sĩ) fixed completely!")
