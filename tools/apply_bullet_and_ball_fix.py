import subprocess, os, re

def run_sql(query):
    cmd = ['docker', 'exec', '-i', 'gunny-sql', '/opt/mssql-tools/bin/sqlcmd', '-S', 'localhost', '-U', 'sa', '-P', 'Gunny@2026Mac', '-d', 'Project_Game34', '-s', '|', '-W']
    res = subprocess.run(cmd, input=query.encode('utf-8'), capture_output=True)
    return res.stdout.decode('utf-8', errors='ignore')

# 1. Update Ball table in Project_Game34
sql_ball = """
-- Update Ball for Gậy Như Ý (+15 and Gold):
-- 11369: +15 normal shot -> FlyingPartical = 758 (狼牙棒五级强 - Hào quang cực mạnh +15)
-- 11372: +15 POW -> FlyingPartical = 763 (狼牙棒五级必杀 - Hào quang POW cực mạnh +15), Crater = 878
-- 113691: MultiBall -> FlyingPartical = 758
-- 113692: AddWound -> FlyingPartical = 758

UPDATE Ball SET FlyingPartical = '756' WHERE ID = 11367;
UPDATE Ball SET FlyingPartical = '757' WHERE ID = 11368;
UPDATE Ball SET FlyingPartical = '758', Crater = 876 WHERE ID = 11369;

UPDATE Ball SET FlyingPartical = '759' WHERE ID = 11370;
UPDATE Ball SET FlyingPartical = '761', Crater = 878 WHERE ID = 11371;
UPDATE Ball SET FlyingPartical = '763', Crater = 878 WHERE ID = 11372;

UPDATE Ball SET FlyingPartical = '758' WHERE ID = 113691;
UPDATE Ball SET FlyingPartical = '758' WHERE ID = 113692;

-- Update Shop_Goods: Ensure +15 uses Pic SSSgcudgel3 (has animated game.png)
UPDATE Shop_Goods SET Pic = 'SSSgcudgel3' WHERE TemplateID = 71844;
UPDATE Shop_Goods SET Pic = 'SSgcudgel3' WHERE TemplateID = 71854;
UPDATE Shop_Goods SET Pic = 'Sgcudgel3' WHERE TemplateID = 71864;
"""
print("Applying SQL updates for Ball and Shop_Goods...")
out_sql = run_sql(sql_ball)
print("SQL output:", out_sql)

# 2. Update partical.xml to also define 898, 899, 900 as emitters
partical_path = 'Source Flash/FlashSV1/partical.xml'
with open(partical_path, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

# Add aliases if not present
if 'id="900"' not in content:
    alias_emitters = """
  <emitter beginAngle="0" endAngle="6.28" life="0" id="898" name="898Gậy Như Ý thường" interval="0.03">
    <particle sizeSize="0" vOrient="0" lifeSize="0" weightOrient="0" sizeOrient="1" keepColor="true" spinOrient="6" name="898" motionVSize="0" countSize="0" spinSize="0" colorOrient="0" countOrient="1" keepOldFirst="false" displayCreator="588" alphaOrient="1" motionVOrient="0" blendMode="add" lifeEasing="[object AbstractLifeEasing]" lifeOrient="0.3" weightSize="0" vSize="0" beginTime="0" endTime="1000" rotation="0">
      <easing name="vLine" value=""/>
      <easing name="rvLine" value=""/>
      <easing name="spLine" value=""/>
      <easing name="sizeLine" value="0:0.7,1:0.3,"/>
      <easing name="weightLine" value=""/>
      <easing name="alphaLine" value="0:0.6,1:0.1,"/>
    </particle>
  </emitter>
  <emitter beginAngle="0" endAngle="6.28" life="0" id="899" name="899Gậy Như Ý +12" interval="0.03">
    <particle sizeSize="0" vOrient="0" lifeSize="0" weightOrient="0" sizeOrient="1" keepColor="true" spinOrient="6" name="899" motionVSize="0" countSize="0" spinSize="0" colorOrient="0" countOrient="1" keepOldFirst="false" displayCreator="588" alphaOrient="1" motionVOrient="0" blendMode="add" lifeEasing="[object AbstractLifeEasing]" lifeOrient="0.3" weightSize="0" vSize="0" beginTime="0" endTime="1000" rotation="0">
      <easing name="vLine" value=""/>
      <easing name="rvLine" value=""/>
      <easing name="spLine" value=""/>
      <easing name="sizeLine" value="0:0.9,1:0.5,"/>
      <easing name="weightLine" value=""/>
      <easing name="alphaLine" value="0:0.8,1:0.2,"/>
    </particle>
  </emitter>
  <emitter beginAngle="0" endAngle="6.28" life="0" id="900" name="900Gậy Như Ý +15" interval="0.03">
    <particle sizeSize="0" vOrient="0" lifeSize="0" weightOrient="0" sizeOrient="1" keepColor="true" spinOrient="6" name="900" motionVSize="0" countSize="0" spinSize="0" colorOrient="0" countOrient="1" keepOldFirst="false" displayCreator="593" alphaOrient="1" motionVOrient="0" blendMode="add" lifeEasing="[object AbstractLifeEasing]" lifeOrient="0.3" weightSize="0" vSize="0" beginTime="0" endTime="1000" rotation="0">
      <easing name="vLine" value=""/>
      <easing name="rvLine" value=""/>
      <easing name="spLine" value=""/>
      <easing name="sizeLine" value="0:1.2,1:0.6,"/>
      <easing name="weightLine" value=""/>
      <easing name="alphaLine" value="0:1.0,1:0.3,"/>
    </particle>
  </emitter>
"""
    # Insert right before </list>
    content = content.replace('</list>', alias_emitters + '\n</list>')
    with open(partical_path, 'w', encoding='utf-8') as f:
        f.write(content)
    print("Updated partical.xml with emitters 898, 899, 900.")
else:
    print("partical.xml already has id 900.")
