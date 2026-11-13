-- Fill Equip bag + Prop bag + Cards + Pets for admin (max) and max40 (limited).
-- MUST logout both accounts before/after running, then login again.
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @admin int = (SELECT TOP 1 UserID FROM Project_Player34.dbo.Sys_Users_Detail WHERE UserName=N'admin');
DECLARE @u40 int = (SELECT TOP 1 UserID FROM Project_Player34.dbo.Sys_Users_Detail WHERE UserName=N'max40');
IF @admin IS NULL SET @admin = 1;

------------------------------------------------------------
-- Common goods insert helper via temp tables
------------------------------------------------------------
IF OBJECT_ID('tempdb..#EquipSet') IS NOT NULL DROP TABLE #EquipSet;
CREATE TABLE #EquipSet (Place int, TemplateID int);
INSERT INTO #EquipSet VALUES
(0,1761),(1,2310),(2,3349),(3,4355),(4,5527),(5,6305),(6,71424),
(7,85384),(8,85204),(9,95384),(10,95354),(11,13726),(12,14056),(13,15154),(14,16967),(15,17013);

-- Pool of high-quality equip for bag fill
IF OBJECT_ID('tempdb..#BagPool') IS NOT NULL DROP TABLE #BagPool;
SELECT TemplateID, CategoryID,
  ROW_NUMBER() OVER (ORDER BY Quality DESC, Attack+Defence+Agility+Luck DESC, TemplateID) AS rn
INTO #BagPool
FROM Project_Game34.dbo.Shop_Goods
WHERE NeedSex IN (0,1)
  AND CategoryID IN (1,2,3,4,5,6,7,8,9,13,14,15,16,17)
  AND Quality >= 3
  AND TemplateID BETWEEN 1000 AND 999999
  AND TemplateID NOT IN (SELECT TemplateID FROM #EquipSet);

-- Useful props (đạo cụ)
IF OBJECT_ID('tempdb..#PropPool') IS NOT NULL DROP TABLE #PropPool;
SELECT TemplateID, Name, MaxCount,
  ROW_NUMBER() OVER (ORDER BY TemplateID) AS rn
INTO #PropPool
FROM Project_Game34.dbo.Shop_Goods
WHERE CategoryID = 11
  AND TemplateID IN (
    11001,11002,11003,11004,11005,11006,11007,11008,11009,11010,11011,11012,11013,11014,11015,
    11016,11017,11018,11019,11020,11021,11022,11023,11024,11025,
    11101,11102,11103,11104,11105,11106,11107,
    11201,11202,11203,11204,11205,
    11408,11560,11907,11912,11913,11914,11991,11992,11995,11997,11998,
    10060,10061,10062,10063,10064,10065,10066,10067,10068,10069,10070,10071
  );

------------------------------------------------------------
-- PROCEDURE-like: seed one user
------------------------------------------------------------
-- @stren / @compose / @equipBagSlots / @propSlots / @cardCount / @petCount / @petLevel

DECLARE @uid int, @stren int, @compose int, @bagSlots int, @propSlots int, @cardMax int, @petMax int, @petLv int;

-- ========== ADMIN ==========
SET @uid = @admin; SET @stren = 15; SET @compose = 80; SET @bagSlots = 90; SET @propSlots = 80; SET @cardMax = 50; SET @petMax = 18; SET @petLv = 50;

DELETE FROM Project_Player34.dbo.Sys_Users_Goods WHERE UserID=@uid;
DELETE FROM Project_Player34.dbo.Sys_Users_Card WHERE UserID=@uid;
DELETE FROM Project_Player34.dbo.Sys_Users_Pet WHERE UserID=@uid;

-- Equipped
INSERT INTO Project_Player34.dbo.Sys_Users_Goods
(UserID, BagType, TemplateID, Place, Count, IsJudge, Color, IsExist, StrengthenLevel, StrengthenExp,
 AttackCompose, DefendCompose, LuckCompose, AgilityCompose, Skin, IsBinds, IsUsed, BeginDate, ValidDate,
 RemoveDate, RemoveType, Hole1, Hole2, Hole3, Hole4, Hole5, Hole6, Hole5Level, Hole5Exp, Hole6Level, Hole6Exp,
 StrengthenTimes, StrengthenRefineryLevel, isGold, goldValidDate, goldBeginTime, Blood,
 latentEnergyCurStr, latentEnergyNewStr, latentEnergyEndTime, curExp, cellLocked)
SELECT @uid, 0, TemplateID, Place, 1, 1, N'', 1, @stren, 0,
       @compose, @compose, @compose, @compose, N'', 1, 1, GETDATE(), 0,
       GETDATE(), 0, 1,1,1,1,1,1, 0,0,0,0, 0,0, 0,0, GETDATE(), 0,
       N'0,0,0,0', N'0,0,0,0', GETDATE(), 0, 0
FROM #EquipSet;

-- Equip bag (BagType 0, Place 31+)
INSERT INTO Project_Player34.dbo.Sys_Users_Goods
(UserID, BagType, TemplateID, Place, Count, IsJudge, Color, IsExist, StrengthenLevel, StrengthenExp,
 AttackCompose, DefendCompose, LuckCompose, AgilityCompose, Skin, IsBinds, IsUsed, BeginDate, ValidDate,
 RemoveDate, RemoveType, Hole1, Hole2, Hole3, Hole4, Hole5, Hole6, Hole5Level, Hole5Exp, Hole6Level, Hole6Exp,
 StrengthenTimes, StrengthenRefineryLevel, isGold, goldValidDate, goldBeginTime, Blood,
 latentEnergyCurStr, latentEnergyNewStr, latentEnergyEndTime, curExp, cellLocked)
SELECT TOP (@bagSlots) @uid, 0, TemplateID, 30 + rn, 1, 1, N'', 1, @stren, 0,
       @compose/2, @compose/2, @compose/2, @compose/2, N'', 1, 0, GETDATE(), 0,
       GETDATE(), 0, -1,-1,-1,-1,-1,-1, 0,0,0,0, 0,0, 0,0, GETDATE(), 0,
       N'0,0,0,0', N'0,0,0,0', GETDATE(), 0, 0
FROM #BagPool
WHERE rn <= @bagSlots
ORDER BY rn;

-- Prop bag (BagType 1)
INSERT INTO Project_Player34.dbo.Sys_Users_Goods
(UserID, BagType, TemplateID, Place, Count, IsJudge, Color, IsExist, StrengthenLevel, StrengthenExp,
 AttackCompose, DefendCompose, LuckCompose, AgilityCompose, Skin, IsBinds, IsUsed, BeginDate, ValidDate,
 RemoveDate, RemoveType, Hole1, Hole2, Hole3, Hole4, Hole5, Hole6, Hole5Level, Hole5Exp, Hole6Level, Hole6Exp,
 StrengthenTimes, StrengthenRefineryLevel, isGold, goldValidDate, goldBeginTime, Blood,
 latentEnergyCurStr, latentEnergyNewStr, latentEnergyEndTime, curExp, cellLocked)
SELECT TOP (@propSlots) @uid, 1, TemplateID, rn - 1, CASE WHEN MaxCount > 999 THEN 999 ELSE CASE WHEN MaxCount < 1 THEN 99 ELSE MaxCount END END, 1, N'', 1, 0, 0,
       0,0,0,0, N'', 1, 0, GETDATE(), 0,
       GETDATE(), 0, -1,-1,-1,-1,-1,-1, 0,0,0,0, 0,0, 0,0, GETDATE(), 0,
       N'0,0,0,0', N'0,0,0,0', GETDATE(), 0, 0
FROM #PropPool
WHERE rn <= @propSlots
ORDER BY rn;

-- Cards: place 0-3 equipped, rest bag from 5
INSERT INTO Project_Player34.dbo.Sys_Users_Card
(UserID, TemplateID, Place, Count, Attack, Defence, Agility, Luck, Guard, AttackReset, DefenceReset, AgilityReset, LuckReset, Damage, Level, CardGP, isFirstGet)
SELECT TOP (@cardMax) @uid, CardID,
  CASE WHEN ROW_NUMBER() OVER (ORDER BY CardID) <= 4 THEN CAST(ROW_NUMBER() OVER (ORDER BY CardID) - 1 AS int)
       ELSE CAST(ROW_NUMBER() OVER (ORDER BY CardID) + 0 AS int) END,
  1, 50, 50, 50, 50, 20, 0,0,0,0, 30, 30, 999999, 1
FROM (SELECT DISTINCT CardID FROM Project_Game34.dbo.Card_Template_Info) c
ORDER BY CardID;

-- Fix card places: 0..3 equip, then 5,6,7...
;WITH c AS (
  SELECT CardID, ROW_NUMBER() OVER (ORDER BY TemplateID) AS rn
  FROM Project_Player34.dbo.Sys_Users_Card WHERE UserID=@uid
)
UPDATE sc SET Place = CASE WHEN c.rn <= 4 THEN c.rn - 1 ELSE c.rn + 0 END
FROM Project_Player34.dbo.Sys_Users_Card sc
JOIN c ON c.CardID = sc.CardID;
-- Actually for rn>4 place should be rn+0 starting at 5: place = rn + 0 when rn>=5 -> rn, but when rn=5 place=5. When rn=4 place=3. So place = CASE WHEN rn<=4 THEN rn-1 ELSE rn END
;WITH c2 AS (
  SELECT CardID, ROW_NUMBER() OVER (ORDER BY TemplateID) AS rn
  FROM Project_Player34.dbo.Sys_Users_Card WHERE UserID=@uid
)
UPDATE sc SET Place = CASE WHEN c2.rn <= 4 THEN c2.rn - 1 ELSE c2.rn END
FROM Project_Player34.dbo.Sys_Users_Card sc
JOIN c2 ON c2.CardID = sc.CardID;

-- Pets (max 18 for admin)
INSERT INTO Project_Player34.dbo.Sys_Users_Pet
(TemplateID, Name, UserID, Attack, Defence, Luck, Agility, Blood, Damage, Guard,
 AttackGrow, DefenceGrow, LuckGrow, AgilityGrow, BloodGrow, DamageGrow, GuardGrow,
 Level, GP, MaxGP, Hunger, PetHappyStar, MP, IsEquip, Place, IsExit, Skill, SkillEquip,
 currentStarExp, breakGrade, breakAttack, breakDefence, breakAgility, breakLuck, breakBlood, eQPets, BaseProp)
SELECT TOP (@petMax)
  p.TemplateID, p.Name, @uid,
  p.HighAttack, p.HighDefence, p.HighLuck, p.HighAgility, p.HighBlood, p.HighDamage, p.HighGuard,
  CAST(p.HighAttackGrow AS int), CAST(p.HighDefenceGrow AS int), CAST(p.HighLuckGrow AS int),
  CAST(p.HighAgilityGrow AS int), CAST(p.HighBloodGrow AS int), CAST(p.HighDamageGrow AS int), CAST(p.HighGuardGrow AS int),
  @petLv, 0, 0, 10000, 3, p.MP, CASE WHEN ROW_NUMBER() OVER (ORDER BY p.StarLevel DESC, p.HighAttack DESC)=1 THEN 1 ELSE 0 END,
  CAST(ROW_NUMBER() OVER (ORDER BY p.StarLevel DESC, p.HighAttack DESC) - 1 AS int),
  1, N'0,0|-1,1|-1,2|-1,3|-1,4', N'0,0|0,1|0,2|0,3|0,4',
  0, 0, 0,0,0,0,0, N'[]', N''
FROM Project_Game34.dbo.Pet_Template_Info p
WHERE p.StarLevel >= 4 AND RIGHT(CAST(p.TemplateID AS varchar(20)),1) IN ('1','3')
ORDER BY p.StarLevel DESC, p.HighAttack DESC;

-- ========== MAX40 (limited) ==========
IF @u40 IS NOT NULL
BEGIN
  SET @uid = @u40; SET @stren = 12; SET @compose = 50; SET @bagSlots = 40; SET @propSlots = 35; SET @cardMax = 20; SET @petMax = 8; SET @petLv = 30;

  DELETE FROM Project_Player34.dbo.Sys_Users_Goods WHERE UserID=@uid;
  DELETE FROM Project_Player34.dbo.Sys_Users_Card WHERE UserID=@uid;
  DELETE FROM Project_Player34.dbo.Sys_Users_Pet WHERE UserID=@uid;

  INSERT INTO Project_Player34.dbo.Sys_Users_Goods
  (UserID, BagType, TemplateID, Place, Count, IsJudge, Color, IsExist, StrengthenLevel, StrengthenExp,
   AttackCompose, DefendCompose, LuckCompose, AgilityCompose, Skin, IsBinds, IsUsed, BeginDate, ValidDate,
   RemoveDate, RemoveType, Hole1, Hole2, Hole3, Hole4, Hole5, Hole6, Hole5Level, Hole5Exp, Hole6Level, Hole6Exp,
   StrengthenTimes, StrengthenRefineryLevel, isGold, goldValidDate, goldBeginTime, Blood,
   latentEnergyCurStr, latentEnergyNewStr, latentEnergyEndTime, curExp, cellLocked)
  SELECT @uid, 0, TemplateID, Place, 1, 1, N'', 1, @stren, 0,
         @compose, @compose, @compose, @compose, N'', 1, 1, GETDATE(), 0,
         GETDATE(), 0, 1,1,1,1,1,1, 0,0,0,0, 0,0, 0,0, GETDATE(), 0,
         N'0,0,0,0', N'0,0,0,0', GETDATE(), 0, 0
  FROM #EquipSet;

  INSERT INTO Project_Player34.dbo.Sys_Users_Goods
  (UserID, BagType, TemplateID, Place, Count, IsJudge, Color, IsExist, StrengthenLevel, StrengthenExp,
   AttackCompose, DefendCompose, LuckCompose, AgilityCompose, Skin, IsBinds, IsUsed, BeginDate, ValidDate,
   RemoveDate, RemoveType, Hole1, Hole2, Hole3, Hole4, Hole5, Hole6, Hole5Level, Hole5Exp, Hole6Level, Hole6Exp,
   StrengthenTimes, StrengthenRefineryLevel, isGold, goldValidDate, goldBeginTime, Blood,
   latentEnergyCurStr, latentEnergyNewStr, latentEnergyEndTime, curExp, cellLocked)
  SELECT TOP (@bagSlots) @uid, 0, TemplateID, 30 + rn, 1, 1, N'', 1, @stren, 0,
         @compose/2, @compose/2, @compose/2, @compose/2, N'', 1, 0, GETDATE(), 0,
         GETDATE(), 0, -1,-1,-1,-1,-1,-1, 0,0,0,0, 0,0, 0,0, GETDATE(), 0,
         N'0,0,0,0', N'0,0,0,0', GETDATE(), 0, 0
  FROM #BagPool WHERE rn <= @bagSlots ORDER BY rn;

  INSERT INTO Project_Player34.dbo.Sys_Users_Goods
  (UserID, BagType, TemplateID, Place, Count, IsJudge, Color, IsExist, StrengthenLevel, StrengthenExp,
   AttackCompose, DefendCompose, LuckCompose, AgilityCompose, Skin, IsBinds, IsUsed, BeginDate, ValidDate,
   RemoveDate, RemoveType, Hole1, Hole2, Hole3, Hole4, Hole5, Hole6, Hole5Level, Hole5Exp, Hole6Level, Hole6Exp,
   StrengthenTimes, StrengthenRefineryLevel, isGold, goldValidDate, goldBeginTime, Blood,
   latentEnergyCurStr, latentEnergyNewStr, latentEnergyEndTime, curExp, cellLocked)
  SELECT TOP (@propSlots) @uid, 1, TemplateID, rn - 1, CASE WHEN MaxCount > 500 THEN 500 ELSE CASE WHEN MaxCount < 1 THEN 50 ELSE MaxCount END END, 1, N'', 1, 0, 0,
         0,0,0,0, N'', 1, 0, GETDATE(), 0,
         GETDATE(), 0, -1,-1,-1,-1,-1,-1, 0,0,0,0, 0,0, 0,0, GETDATE(), 0,
         N'0,0,0,0', N'0,0,0,0', GETDATE(), 0, 0
  FROM #PropPool WHERE rn <= @propSlots ORDER BY rn;

  INSERT INTO Project_Player34.dbo.Sys_Users_Card
  (UserID, TemplateID, Place, Count, Attack, Defence, Agility, Luck, Guard, AttackReset, DefenceReset, AgilityReset, LuckReset, Damage, Level, CardGP, isFirstGet)
  SELECT TOP (@cardMax) @uid, CardID, 0, 1, 30, 30, 30, 30, 10, 0,0,0,0, 15, 20, 100000, 1
  FROM (SELECT DISTINCT CardID FROM Project_Game34.dbo.Card_Template_Info) c
  ORDER BY CardID;

  ;WITH c2 AS (
    SELECT CardID, ROW_NUMBER() OVER (ORDER BY TemplateID) AS rn
    FROM Project_Player34.dbo.Sys_Users_Card WHERE UserID=@uid
  )
  UPDATE sc SET Place = CASE WHEN c2.rn <= 4 THEN c2.rn - 1 ELSE c2.rn END
  FROM Project_Player34.dbo.Sys_Users_Card sc
  JOIN c2 ON c2.CardID = sc.CardID;

  INSERT INTO Project_Player34.dbo.Sys_Users_Pet
  (TemplateID, Name, UserID, Attack, Defence, Luck, Agility, Blood, Damage, Guard,
   AttackGrow, DefenceGrow, LuckGrow, AgilityGrow, BloodGrow, DamageGrow, GuardGrow,
   Level, GP, MaxGP, Hunger, PetHappyStar, MP, IsEquip, Place, IsExit, Skill, SkillEquip,
   currentStarExp, breakGrade, breakAttack, breakDefence, breakAgility, breakLuck, breakBlood, eQPets, BaseProp)
  SELECT TOP (@petMax)
    p.TemplateID, p.Name, @uid,
    p.HighAttack/2, p.HighDefence/2, p.HighLuck/2, p.HighAgility/2, p.HighBlood/2, p.HighDamage/2, p.HighGuard/2,
    CAST(p.HighAttackGrow AS int), CAST(p.HighDefenceGrow AS int), CAST(p.HighLuckGrow AS int),
    CAST(p.HighAgilityGrow AS int), CAST(p.HighBloodGrow AS int), CAST(p.HighDamageGrow AS int), CAST(p.HighGuardGrow AS int),
    @petLv, 0, 0, 10000, 2, p.MP, CASE WHEN ROW_NUMBER() OVER (ORDER BY p.StarLevel DESC, p.HighAttack DESC)=1 THEN 1 ELSE 0 END,
    CAST(ROW_NUMBER() OVER (ORDER BY p.StarLevel DESC, p.HighAttack DESC) - 1 AS int),
    1, N'0,0|-1,1|-1,2|-1,3|-1,4', N'0,0|0,1|0,2|-1,3|-1,4',
    0, 0, 0,0,0,0,0, N'[]', N''
  FROM Project_Game34.dbo.Pet_Template_Info p
  WHERE p.StarLevel >= 3 AND RIGHT(CAST(p.TemplateID AS varchar(20)),1) IN ('1','3')
  ORDER BY p.StarLevel DESC, p.HighAttack DESC;
END

-- Refresh style on both
DECLARE @Style nvarchar(200) = N'1761,2310,3349,4355,5527,6305,71424,85384,85204,95384,95354,13726,14056,15154,16967,17013';
UPDATE Project_Player34.dbo.Sys_Users_Detail SET Style=@Style, Colors=N',,,,,,,,,,,,,,,,', State=0 WHERE UserID IN (@admin, @u40);

SELECT 'SUMMARY' AS t;
SELECT d.UserID, d.UserName, d.Grade,
  (SELECT COUNT(*) FROM Project_Player34.dbo.Sys_Users_Goods g WHERE g.UserID=d.UserID AND g.BagType=0 AND g.Place>=31) AS equipBag,
  (SELECT COUNT(*) FROM Project_Player34.dbo.Sys_Users_Goods g WHERE g.UserID=d.UserID AND g.BagType=1) AS propBag,
  (SELECT COUNT(*) FROM Project_Player34.dbo.Sys_Users_Card c WHERE c.UserID=d.UserID) AS cards,
  (SELECT COUNT(*) FROM Project_Player34.dbo.Sys_Users_Pet p WHERE p.UserID=d.UserID AND p.IsExit=1) AS pets
FROM Project_Player34.dbo.Sys_Users_Detail d
WHERE d.UserID IN (@admin, @u40);
