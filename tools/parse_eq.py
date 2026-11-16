import json
with open('tools/pet42_eq.json', 'r', encoding='utf-8') as f:
    data = json.load(f)
print(f"Total pet items: {len(data)}")
for item in data:
    t = item.get('Template', {})
    print(f"Place: {item.get('Place')}, TemplateID: {item.get('TemplateID')}, Name: {t.get('Name')}, Cat: {t.get('CategoryID')}, Att: {t.get('Attack')}, Def: {t.get('Defence')}, Agi: {t.get('Agility')}, Luck: {t.get('Luck')}")
