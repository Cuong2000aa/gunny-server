import subprocess

cmd = ['docker', 'exec', 'gunny-sql', '/opt/mssql-tools/bin/sqlcmd', '-S', 'localhost', '-U', 'sa', '-P', 'Gunny@2026Mac', '-d', 'Project_Player34', '-y', '0', '-Q', 'SELECT eQPets FROM Sys_Users_Pet WHERE ID = 42;']
out = subprocess.check_output(cmd, text=True)
print("Length:", len(out))
for line in out.split('\n'):
    if line.strip().startswith('['):
        print("Found line:", line[:300])
        with open('tools/pet42_eq.json', 'w', encoding='utf-8') as f:
            f.write(line.strip())
