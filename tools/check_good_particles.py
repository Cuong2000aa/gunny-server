with open('Source Flash/FlashSV1/partical.xml', 'r', encoding='utf-8', errors='ignore') as f:
    c = f.read()

import re
for pid in [3, 6, 27, 33, 38, 54, 68, 91, 107]:
    m = re.findall(rf'(<partical[^>]*id=[\"\']{pid}[\"\'][^>]*>.*?</partical>)', c, re.DOTALL | re.IGNORECASE)
    if m:
        print(f"=== PARTICAL {pid} ===")
        # print name or type or first 150 chars
        print(m[0][:150].replace('\n', ' '))
