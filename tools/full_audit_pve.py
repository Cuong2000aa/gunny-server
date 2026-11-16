import subprocess, os, re, json, shutil

def run_sql(query):
    cmd = ['docker', 'exec', '-i', 'gunny-sql', '/opt/mssql-tools/bin/sqlcmd', '-S', 'localhost', '-U', 'sa', '-P', 'Gunny@2026Mac', '-d', 'Project_Game34', '-s', '|', '-W']
    res = subprocess.run(cmd, input=query.encode('utf-8'), capture_output=True)
    return res.stdout.decode('utf-8', errors='ignore')

# 1. First, apply fix for Dragon War (ID 5) in Pve_Info
sql_fix_dcr = """
UPDATE Pve_Info 
SET SimpleGameScript = 'GameServerScript.AI.Game.DragonWarNormalGame', 
    SimpleTemplateIds = NormalTemplateIds 
WHERE ID = 5 AND (SimpleGameScript IS NULL OR SimpleGameScript = '');
"""
run_sql(sql_fix_dcr)
print("Updated Pve_Info for Dragon War ID 5.")

# 2. Query all PVEs
pve_raw = run_sql("SELECT ID, Name, LevelLimits, ISNULL(SimpleGameScript,''), ISNULL(NormalGameScript,''), ISNULL(HardGameScript,''), ISNULL(TerrorGameScript,''), ISNULL(Description,'') FROM Pve_Info ORDER BY ID;")

pves = {}
for line in pve_raw.split('\n'):
    line = line.strip()
    if not line or line.startswith('-') or line.startswith('ID') or 'rows affected' in line:
        continue
    parts = line.split('|')
    if parts and parts[0].strip().isdigit():
        pid = int(parts[0].strip())
        pves[pid] = {
            'id': pid,
            'name': parts[1].strip() if len(parts) > 1 else '',
            'level': parts[2].strip() if len(parts) > 2 else '',
            'simple_script': parts[3].strip() if len(parts) > 3 else '',
            'normal_script': parts[4].strip() if len(parts) > 4 else '',
            'hard_script': parts[5].strip() if len(parts) > 5 else '',
            'terror_script': parts[6].strip() if len(parts) > 6 else '',
            'desc': parts[7].strip() if len(parts) > 7 else ''
        }

print(f"Loaded {len(pves)} PVEs from Pve_Info.")

# 3. Load all C# files
cs_game_scripts = {}
cs_mission_scripts = {}
cs_npc_scripts = {}

for root, dirs, files in os.walk('GameServerScripts'):
    norm_root = root.replace('\\', '/')
    for f in files:
        if f.endswith('.cs'):
            name = f[:-3]
            fp = os.path.join(root, f)
            with open(fp, 'r', encoding='utf-8', errors='ignore') as sfp:
                content = sfp.read()
                if '/Messions' in norm_root or '/Mession' in norm_root:
                    cs_mission_scripts[name] = content
                elif '/NPC' in norm_root:
                    cs_npc_scripts[name] = content
                elif '/Game' in norm_root:
                    cs_game_scripts[name] = content
                else:
                    cs_mission_scripts[name] = content

print(f"Loaded {len(cs_game_scripts)} GameScripts, {len(cs_mission_scripts)} MissionScripts, {len(cs_npc_scripts)} NPCScripts.")

# 4. Load Mission_Info
mission_raw = run_sql("SELECT Id, Name, ISNULL(Script,''), ISNULL(TotalTurn,0), ISNULL(Description,''), ISNULL(Title,'') FROM Mission_Info ORDER BY Id;")
missions = {}
for line in mission_raw.split('\n'):
    line = line.strip()
    if not line or line.startswith('-') or line.startswith('Id') or 'rows affected' in line:
        continue
    parts = line.split('|')
    if parts and parts[0].strip().isdigit():
        mid = int(parts[0].strip())
        missions[mid] = {
            'id': mid,
            'name': parts[1].strip() if len(parts) > 1 else '',
            'script': parts[2].strip() if len(parts) > 2 else '',
            'turns': parts[3].strip() if len(parts) > 3 else '',
            'desc': parts[4].strip() if len(parts) > 4 else '',
            'title': parts[5].strip() if len(parts) > 5 else ''
        }

print(f"Loaded {len(missions)} missions from Mission_Info.")

# 5. Load Game_Map
map_raw = run_sql("SELECT ID, Name, ISNULL(PosX,''), ISNULL(ForegroundWidth,0), ISNULL(ForegroundHeight,0) FROM Game_Map;")
maps = {}
for line in map_raw.split('\n'):
    line = line.strip()
    if not line or line.startswith('-') or line.startswith('ID') or 'rows affected' in line:
        continue
    parts = line.split('|')
    if parts and parts[0].strip().isdigit():
        mid = int(parts[0].strip())
        maps[mid] = {
            'id': mid,
            'name': parts[1].strip() if len(parts) > 1 else '',
            'posx': parts[2].strip() if len(parts) > 2 else ''
        }

print(f"Loaded {len(maps)} maps from Game_Map.")

# 6. Load NPC_Info
npc_raw = run_sql("SELECT ID, Name, ISNULL(Script,''), ISNULL(Blood,0) FROM NPC_Info;")
npcs = {}
for line in npc_raw.split('\n'):
    line = line.strip()
    if not line or line.startswith('-') or line.startswith('ID') or 'rows affected' in line:
        continue
    parts = line.split('|')
    if parts and parts[0].strip().isdigit():
        nid = int(parts[0].strip())
        npcs[nid] = {
            'id': nid,
            'name': parts[1].strip() if len(parts) > 1 else '',
            'script': parts[2].strip() if len(parts) > 2 else '',
            'blood': parts[3].strip() if len(parts) > 3 else ''
        }

print(f"Loaded {len(npcs)} NPCs from NPC_Info.")

# 7. Check Map files on disk
def map_exists_on_disk(map_id):
    for base in ['Fighting.Service/bin/Release/net8.0/map', 'Fighting.Service/map', 'Road.Service/map']:
        p = os.path.join(base, str(map_id))
        if os.path.exists(p):
            if os.path.exists(os.path.join(p, 'fore.map')) or os.path.exists(os.path.join(p, 'dead.map')):
                return True
    return False

# Audit each PVE
audit_results = []

for pid, pve in sorted(pves.items()):
    res = {
        'id': pid,
        'name': pve['name'],
        'level': pve.get('level', ''),
        'desc': pve.get('desc', ''),
        'scripts': {},
        'missions': [],
        'issues': [],
        'status': 'OK'
    }
    
    script_types = [
        ('Simple', pve.get('simple_script', '')),
        ('Normal', pve.get('normal_script', '')),
        ('Hard', pve.get('hard_script', '')),
        ('Terror', pve.get('terror_script', ''))
    ]
    
    has_any_script = False
    all_setup_missions = []
    
    for diff, sname in script_types:
        if not sname:
            res['scripts'][diff] = {'configured': False}
            if diff in ['Simple', 'Normal']:
                res['issues'].append(f"Thiếu script cho độ khó {diff}")
            continue
        
        has_any_script = True
        cls_name = sname.split('.')[-1]
        exists = cls_name in cs_game_scripts
        res['scripts'][diff] = {
            'configured': True,
            'class': cls_name,
            'exists': exists
        }
        
        if not exists:
            res['issues'].append(f"Không tìm thấy file C# {cls_name}.cs")
        else:
            cs = cs_game_scripts[cls_name]
            m_setups = re.findall(r'SetupMissions\s*\(\s*["\']([^"\']+)["\']\s*\)', cs)
            for m_str in m_setups:
                m_ids = [int(x.strip()) for x in m_str.split(',') if x.strip().isdigit()]
                for mid in m_ids:
                    if mid not in all_setup_missions:
                        all_setup_missions.append(mid)
    
    if not has_any_script:
        res['status'] = 'NO_SCRIPTS'
        res['issues'].append("Chưa cấu hình bất kỳ GameScript nào trong Pve_Info")
        audit_results.append(res)
        continue
    
    # Audit all missions found
    for mid in all_setup_missions:
        m_data = {
            'mission_id': mid,
            'name': '',
            'title': '',
            'desc': '',
            'in_db': mid in missions,
            'script_class': '',
            'script_exists': False,
            'map_id': None,
            'map_in_db': False,
            'map_has_posx': False,
            'map_on_disk': False,
            'bosses': [],
            'dialogues': []
        }
        
        if mid in missions:
            m_data['name'] = missions[mid]['name']
            m_data['script_class'] = missions[mid]['script'].split('.')[-1]
            m_data['title'] = missions[mid]['title']
            m_data['desc'] = missions[mid]['desc']
        
        cls_m = m_data['script_class']
        if cls_m and cls_m in cs_mission_scripts:
            m_data['script_exists'] = True
            cs = cs_mission_scripts[cls_m]
            
            # Map
            maps_found = re.findall(r'SetMap\s*\(\s*(\d+)\s*\)', cs)
            if maps_found:
                map_id = int(maps_found[0])
                m_data['map_id'] = map_id
                m_data['map_in_db'] = map_id in maps
                m_data['map_has_posx'] = bool(maps.get(map_id, {}).get('posx'))
                m_data['map_on_disk'] = map_exists_on_disk(map_id)
                
                if not m_data['map_in_db']:
                    res['issues'].append(f"Ải {mid}: Map {map_id} thiếu trong Game_Map")
                elif not m_data['map_has_posx']:
                    res['issues'].append(f"Ải {mid}: Map {map_id} thiếu tọa độ PosX trong Game_Map")
                if not m_data['map_on_disk']:
                    res['issues'].append(f"Ải {mid}: Map {map_id} thiếu file collision fore.map/dead.map")
            
            # Bosses
            bosses_found = re.findall(r'(?:CreateBoss|bossID\s*=)\s*\(?\s*(\d+)', cs)
            for b_str in set(bosses_found):
                bid = int(b_str)
                b_info = {
                    'boss_id': bid,
                    'name': npcs.get(bid, {}).get('name', 'Unknown'),
                    'in_db': bid in npcs,
                    'script': npcs.get(bid, {}).get('script', ''),
                    'script_exists': False
                }
                b_cls = b_info['script'].split('.')[-1]
                if b_cls and (b_cls in cs_npc_scripts or b_cls in cs_mission_scripts):
                    b_info['script_exists'] = True
                m_data['bosses'].append(b_info)
                
                if not b_info['in_db']:
                    res['issues'].append(f"Ải {mid}: Boss ID {bid} không có trong NPC_Info")
                elif not b_info['script_exists'] and b_info['script']:
                    res['issues'].append(f"Ải {mid}: Boss {b_info['name']} ({bid}) thiếu script {b_cls}.cs")
            
            # Dialogues / Say
            says = re.findall(r'(?:Say|SendMessage)\s*\(\s*["\']([^"\']+)["\']', cs)
            for s in says:
                if len(s.strip()) > 3:
                    m_data['dialogues'].append(s.strip())
        else:
            if cls_m:
                res['issues'].append(f"Ải {mid}: Thiếu file script {cls_m}.cs")
            else:
                res['issues'].append(f"Ải {mid}: Thiếu cấu hình script trong Mission_Info")
        
        res['missions'].append(m_data)
    
    if res['issues']:
        res['status'] = 'HAS_ISSUES'
    else:
        res['status'] = 'READY'
    
    audit_results.append(res)

with open('tools/full_audit_results.json', 'w', encoding='utf-8') as jf:
    json.dump(audit_results, jf, ensure_ascii=False, indent=2)

print("\n=== SUMMARY OF AUDIT ===")
for r in audit_results:
    print(f"[{r['status']}] PVE {r['id']}: {r['name']} - {len(r['missions'])} missions, Issues: {len(r['issues'])}")
    if r['issues']:
        for iss in r['issues'][:3]:
            print(f"    - {iss}")
        if len(r['issues']) > 3:
            print(f"    ... and {len(r['issues']) - 3} more issues")
