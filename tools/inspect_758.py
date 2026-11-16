with open('Source Flash/FlashSV1/partical.xml', 'r', encoding='utf-8', errors='ignore') as f:
    c = f.read()

import re
for eid in [758, 763]:
    m = re.findall(rf'(<emitter[^>]*id=[\"\']{eid}[\"\'][^>]*>.*?</emitter>)', c, re.DOTALL)
    if m:
        print(f"=== EMITTER {eid} ===")
        print(m[0])
