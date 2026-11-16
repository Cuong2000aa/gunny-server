import json

with open('tools/full_audit_results.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

for p in data:
    if p['id'] in [8, 9, 10, 11, 12, 13]:
        print(f"PVE {p['id']}: {p['name']} (Issues: {len(p['issues'])})")
        for iss in p['issues']:
            print(f"   [ISSUE] {iss}")
        for m in p['missions']:
            print(f"   Ải {m['mission_id']}: {m['name']} -> Map {m['map_id']} (in_db={m['map_in_db']}, posx={m['map_has_posx']}, disk={m['map_on_disk']})")
