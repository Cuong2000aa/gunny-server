import json

with open('tools/full_audit_results.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

for p in data:
    if p['status'] == 'READY' and p['missions']:
        print(f"=== PVE {p['id']}: {p['name']} ===")
        print(f"Mô tả: {p['desc']}")
        for m in p['missions']:
            print(f"  * Ải {m['mission_id']}: {m['name']} ({m.get('title','')})")
            if m.get('dialogues'):
                print(f"    Thoại: {m['dialogues'][:2]}")
            if m.get('bosses'):
                b_names = [b['name'] for b in m['bosses']]
                print(f"    Boss: {', '.join(b_names)}")
