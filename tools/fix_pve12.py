import os, shutil, urllib.request, subprocess

def run_sql(query):
    cmd = ['docker', 'exec', '-i', 'gunny-sql', '/opt/mssql-tools/bin/sqlcmd', '-S', 'localhost', '-U', 'sa', '-P', 'Gunny@2026Mac', '-d', 'Project_Game34', '-s', '|', '-W']
    res = subprocess.run(cmd, input=query.encode('utf-8'), capture_output=True)
    return res.stdout.decode('utf-8', errors='ignore')

# 1. Download images for 12001-12004
headers = {'User-Agent': 'Mozilla/5.0'}
sample_small = 'Resources/image/map/1243/samll_map.png'

for m in [12001, 12002, 12003, 12004]:
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

# 2. Copy collision maps across all 4 map directories
map_dirs = [
    'Fighting.Service/bin/Release/net8.0/map',
    'Fighting.Service/map',
    'Road.Service/map',
    'Road.Service/bin/Release/net8.0/map'
]

for mdir in map_dirs:
    os.makedirs(mdir, exist_ok=True)
    for m in [12001, 12002, 12003, 12004]:
        src = os.path.join('Fighting.Service/bin/Release/net8.0/map', str(m))
        dst = os.path.join(mdir, str(m))
        if os.path.exists(src) and os.path.abspath(src) != os.path.abspath(dst):
            os.makedirs(dst, exist_ok=True)
            for f in os.listdir(src):
                shutil.copy(os.path.join(src, f), os.path.join(dst, f))

# 3. Insert/Update Game_Map
sql = """
MERGE INTO Game_Map AS target
USING (VALUES
    (12001, N'Bão thời không', '300,500|400,500|500,500|600,500', '300,500|400,500|500,500|600,500', 1246, 797, 1100, 700, 1246, 797, '', 'back', 'dead', 10, 2),
    (12002, N'Thuyền trưởng Cá Sấu', '300,550|350,550|400,300|500,300', '300,550|350,550|400,300|500,300', 1320, 800, 1100, 700, 1320, 800, '', 'back', 'dead', 10, 2),
    (12003, N'Hồi phục Gà', '650,600|750,600|850,600|950,600', '650,600|750,600|850,600|950,600', 1600, 800, 1100, 700, 1600, 800, '', 'back', 'dead', 10, 2),
    (12004, N'Bác học Vẹt', '300,560|400,550|500,530|600,550', '300,560|400,550|500,530|600,550', 1248, 799, 1100, 700, 1248, 799, '', 'back', 'dead', 10, 2)
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
    VALUES (source.ID, source.Name, source.PosX, source.PosX1, source.Name, source.ForegroundWidth, source.ForegroundHeight, source.BackroundWidht, source.BackroundHeight, source.DeadWidth, source.DeadHeight, 0, 0, source.ForePic, source.BackPic, source.DeadPic, '', '2', '12001', source.Type);
"""
run_sql(sql)
print("PVE 12 (Vòng xoáy thời gian) fixed completely!")
