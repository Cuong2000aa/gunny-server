import re

with open('Source Flash/FlashSV1/partical.xml', 'r', encoding='utf-8', errors='ignore') as f:
    c = f.read()

ids = re.findall(r'id=[\"\'](\d+)[\"\']', c)
ids = sorted(list(set([int(x) for x in ids])))
print('Count of IDs in partical.xml:', len(ids))
print('Max ID in partical.xml:', max(ids) if ids else 0)
print('IDs around 800-1000:', [x for x in ids if 800 <= x <= 1000])
print('Sample IDs:', ids[:40])
