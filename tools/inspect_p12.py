import json

with open('tools/full_audit_results.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

p12 = next(p for p in data if p['id'] == 12)
for m in p12['missions'][:4]:
    print(f"Ải {m['mission_id']}: {m['name']}")
    for b in m['bosses']:
        print(f"   Boss {b['boss_id']}: {b['name']} (in_db={b['in_db']}, script={b['script']}, script_exists={b['script_exists']})")
