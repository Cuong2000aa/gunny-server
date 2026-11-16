with open('Source Flash/FlashSV1/partical.xml', 'r', encoding='utf-8', errors='ignore') as f:
    c = f.read()

import re
matches = re.findall(r'<emitter[^>]*id=[\"\'](\d+)[\"\'][^>]*name=[\"\']([^\"\']+)[\"\']', c)
for eid, name in matches:
    if any(k in name.lower() for k in ['光', '金', '极', '火', '电', '龙', '神', '12', '10', '15', '棒', '猴']):
        print(f"Emitter {eid}: {name}")
