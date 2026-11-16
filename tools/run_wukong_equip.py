import subprocess

sql = """
BEGIN TRANSACTION;

-- 1. Swap Weapon: Move 139 to temp, move 2250 to Slot 6, move 139 to Slot 86
UPDATE Sys_Users_Goods SET Place = 999 WHERE ItemID = 139;
UPDATE Sys_Users_Goods 
SET Place = 6,
    StrengthenLevel = 15,
    isGold = 1,
    goldValidDate = 36500,
    goldBeginTime = GETDATE(),
    AttackCompose = 50, DefendCompose = 50, AgilityCompose = 50, LuckCompose = 50,
    Hole1 = 311526, Hole2 = 313511, Hole3 = 313511, Hole4 = 313507, Hole5 = 313507, Hole6 = 313509,
    Hole5Level = 5, Hole6Level = 5
WHERE ItemID = 2250;
UPDATE Sys_Users_Goods SET Place = 86 WHERE ItemID = 139;

-- 2. Swap Set: Move 171 to temp, move 183 (Set Tôn Ngộ Không) to Slot 11, move 171 to Slot 67
UPDATE Sys_Users_Goods SET Place = 998 WHERE ItemID = 171;
UPDATE Sys_Users_Goods 
SET Place = 11,
    StrengthenLevel = 15,
    isGold = 1,
    goldValidDate = 36500,
    goldBeginTime = GETDATE(),
    AttackCompose = 50, DefendCompose = 50, AgilityCompose = 50, LuckCompose = 50,
    Hole1 = 313511, Hole2 = 313507, Hole3 = 313509, Hole4 = 313510, Hole5 = 313508, Hole6 = 313512,
    Hole5Level = 5, Hole6Level = 5
WHERE ItemID = 183;
UPDATE Sys_Users_Goods SET Place = 67 WHERE ItemID = 171;

-- 3. Move old Wing 146 (Cánh Cát Bụi) from Slot 13 to Slot 124
UPDATE Sys_Users_Goods SET Place = 124 WHERE ItemID = 146;

-- 4. Insert Wing Ngộ Không Mũm Mĩm (15087) into Slot 13 (Equipped)
INSERT INTO Sys_Users_Goods (
    UserID, BagType, TemplateID, Place, Count, IsJudge, IsExist, IsBinds, IsUsed,
    StrengthenLevel, AttackCompose, DefendCompose, AgilityCompose, LuckCompose,
    Hole1, Hole2, Hole3, Hole4, Hole5, Hole6, Hole5Level, Hole6Level, Hole5Exp, Hole6Exp,
    isGold, goldValidDate, goldBeginTime, ValidDate
) VALUES (
    1, 0, 15087, 13, 1, 1, 1, 1, 1,
    15, 50, 50, 50, 50,
    313511, 313507, 313509, 313510, 313508, 313512, 5, 5, 0, 0,
    1, 36500, GETDATE(), 0
);

-- 5. Insert Set Đại Thánh Nổi Giận (13720) into Slot 123 (In bag)
INSERT INTO Sys_Users_Goods (
    UserID, BagType, TemplateID, Place, Count, IsJudge, IsExist, IsBinds, IsUsed,
    StrengthenLevel, AttackCompose, DefendCompose, AgilityCompose, LuckCompose,
    Hole1, Hole2, Hole3, Hole4, Hole5, Hole6, Hole5Level, Hole6Level, Hole5Exp, Hole6Exp,
    isGold, goldValidDate, goldBeginTime, ValidDate
) VALUES (
    1, 0, 13720, 123, 1, 1, 1, 1, 1,
    15, 50, 50, 50, 50,
    313511, 313507, 313509, 313510, 313508, 313512, 5, 5, 0, 0,
    1, 36500, GETDATE(), 0
);

-- 6. Update Character Style
UPDATE Sys_Users_Detail
SET Style = '1761,2310,3351,4355,5527,6308,71844,13193,15087,,17017,,,,,'
WHERE UserID = 1;

COMMIT TRANSACTION;
"""

with open('tools/equip_wukong.sql', 'w', encoding='utf-8') as f:
    f.write(sql)

cmd = ['docker', 'exec', '-i', 'gunny-sql', '/opt/mssql-tools/bin/sqlcmd', '-S', 'localhost', '-U', 'sa', '-P', 'Gunny@2026Mac', '-d', 'Project_Player34']
res = subprocess.run(cmd, input=sql.encode('utf-8'), capture_output=True)
print("Return code:", res.returncode)
print("Stdout:", res.stdout.decode('utf-8', errors='ignore'))
print("Stderr:", res.stderr.decode('utf-8', errors='ignore'))
