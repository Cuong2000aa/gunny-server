import json

with open('tools/full_audit_results.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

p13 = next(p for p in data if p['id'] == 13)
for m in p13['missions']:
    print(f"Ải {m['mission_id']}: {m['name']} -> script: {m['script_class']}")
