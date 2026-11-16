import subprocess, os, re, json

# 1. Query all PVEs from Pve_Info
cmd = ['docker', 'exec', 'gunny-sql', '/opt/mssql-tools/bin/sqlcmd', '-S', 'localhost', '-U', 'sa', '-P', 'Gunny@2026Mac', '-d', 'Project_Game34', '-y', '0', '-Q', 
       'SELECT ID, Name, LevelLimits, SimpleGameScript, NormalGameScript, HardGameScript, TerrorGameScript, SimpleTemplateIds, NormalTemplateIds, HardTemplateIds, TerrorTemplateIds FROM Pve_Info ORDER BY ID;']
out = subprocess.check_output(cmd, text=True)

pves = []
for line in out.split('\n'):
    line = line.strip()
    if not line or line.startswith('-') or line.startswith('ID') or 'rows affected' in line:
        continue
    # parse cols
    parts = [p.strip() for p in line.split('  ') if p.strip()]
    if len(parts) >= 2:
        try:
            pid = int(parts[0])
            name = parts[1]
            pves.append((pid, name, line))
        except:
            pass

print(f"Found {len(pves)} PVE entries in Pve_Info")

# Load GameServerScripts to inspect
cs_files = {}
for root, dirs, files in os.walk('GameServerScripts'):
    for f in files:
        if f.endswith('.cs'):
            fp = os.path.join(root, f)
            with open(fp, 'r', encoding='utf-8', errors='ignore') as sfp:
                cs_files[f[:-3]] = sfp.read()

# Load Mission_Info from DB
cmd_m = ['docker', 'exec', 'gunny-sql', '/opt/mssql-tools/bin/sqlcmd', '-S', 'localhost', '-U', 'sa', '-P', 'Gunny@2026Mac', '-d', 'Project_Game34', '-y', '0', '-Q', 
         'SELECT Id, Name, Script, TotalTurn, Description, Title FROM Mission_Info ORDER BY Id;']
out_m = subprocess.check_output(cmd_m, text=True)
missions_db = {}
for line in out_m.split('\n'):
    line = line.strip()
    if not line or line.startswith('-') or line.startswith('Id') or 'rows affected' in line:
        continue
    parts = [p.strip() for p in line.split('  ') if p.strip()]
    if len(parts) >= 3:
        try:
            mid = int(parts[0])
            missions_db[mid] = {'name': parts[1], 'script': parts[2], 'raw': line}
        except:
            pass

print(f"Found {len(missions_db)} missions in Mission_Info")

# Load Game_Map from DB
cmd_map = ['docker', 'exec', 'gunny-sql', '/opt/mssql-tools/bin/sqlcmd', '-S', 'localhost', '-U', 'sa', '-P', 'Gunny@2026Mac', '-d', 'Project_Game34', '-y', '0', '-Q', 
           'SELECT ID, Name, PosX FROM Game_Map;']
out_map = subprocess.check_output(cmd_map, text=True)
maps_db = set()
for line in out_map.split('\n'):
    line = line.strip()
    if not line or line.startswith('-') or line.startswith('ID') or 'rows affected' in line:
        continue
    parts = [p.strip() for p in line.split('  ') if p.strip()]
    if parts:
        try:
            maps_db.add(int(parts[0]))
        except:
            pass

print(f"Found {len(maps_db)} maps in Game_Map")

# Output audit
with open('tools/pve_audit_report.txt', 'w', encoding='utf-8') as rep:
    rep.write(f"=== PVE AUDIT REPORT ===\nTotal PVEs: {len(pves)}\n\n")

print("Audit setup complete.")
