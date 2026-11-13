-- Fix status numbers (Damage/Armor/HP) + replace broken jewelry + max Texp.
-- Logout/login after running.
SET NOCOUNT ON;

DECLARE @Style60 nvarchar(200) = N'1761,2310,3349,4355,5527,6305,7142,8516,8319,9018,9019,13726,14052,15154,16064,17013';
DECLARE @admin int = (SELECT TOP 1 UserID FROM Project_Player34.dbo.Sys_Users_Detail WHERE UserName=N'admin');
DECLARE @u40 int = (SELECT TOP 1 UserID FROM Project_Player34.dbo.Sys_Users_Detail WHERE UserName=N'max40');
IF @admin IS NULL SET @admin = 1;

-- Safe equip map place -> template
IF OBJECT_ID('tempdb..#SafeEquip') IS NOT NULL DROP TABLE #SafeEquip;
CREATE TABLE #SafeEquip(Place int, TemplateID int);
INSERT INTO #SafeEquip VALUES
(0,1761),(1,2310),(2,3349),(3,4355),(4,5527),(5,6305),(6,7142),
(7,8516),(8,8319),(9,9018),(10,9019),(11,13726),(12,14052),(13,15154),(14,16064),(15,17013);

------------------------------------------------------------
-- ADMIN: replace equipped gear with safe set, holes=-1, stren=15
------------------------------------------------------------
DELETE FROM Project_Player34.dbo.Sys_Users_Goods WHERE UserID=@admin AND BagType=0 AND Place BETWEEN 0 AND 15;

INSERT INTO Project_Player34.dbo.Sys_Users_Goods
(UserID, BagType, TemplateID, Place, Count, IsJudge, Color, IsExist, StrengthenLevel, StrengthenExp,
 AttackCompose, DefendCompose, LuckCompose, AgilityCompose, Skin, IsBinds, IsUsed, BeginDate, ValidDate,
 RemoveDate, RemoveType, Hole1, Hole2, Hole3, Hole4, Hole5, Hole6, Hole5Level, Hole5Exp, Hole6Level, Hole6Exp,
 StrengthenTimes, StrengthenRefineryLevel, isGold, goldValidDate, goldBeginTime, Blood,
 latentEnergyCurStr, latentEnergyNewStr, latentEnergyEndTime, curExp, cellLocked)
SELECT @admin, 0, TemplateID, Place, 1, 1, N'', 1, 15, 0,
       80, 80, 80, 80, N'', 1, 1, GETDATE(), 0,
       GETDATE(), 0, -1,-1,-1,-1,-1,-1, 0,0,0,0, 0,0, 0,0, GETDATE(), 0,
       N'0,0,0,0', N'0,0,0,0', GETDATE(), 0, 0
FROM #SafeEquip;

-- Ensure bag items also have holes=-1 (avoid bead template 1)
UPDATE Project_Player34.dbo.Sys_Users_Goods
SET Hole1=-1, Hole2=-1, Hole3=-1, Hole4=-1, Hole5=-1, Hole6=-1
WHERE UserID=@admin;

-- Max Texp so Tu luyện / derived HP bars have values
UPDATE Project_Player34.dbo.Sys_Users_Texp
SET attTexpExp=1280550, defTexpExp=1280550, spdTexpExp=1280550, lukTexpExp=1280550, hpTexpExp=1280550,
    texpCount=999, texpTaskCount=0
WHERE UserID=@admin;

UPDATE Project_Player34.dbo.Sys_Users_Detail
SET Style=@Style60, Colors=N',,,,,,,,,,,,,,,,', State=0, FightPower=0, Nimbus=999
WHERE UserID=@admin;

------------------------------------------------------------
-- MAX40: same safe set, +12, mid Texp
------------------------------------------------------------
IF @u40 IS NOT NULL
BEGIN
  DELETE FROM Project_Player34.dbo.Sys_Users_Goods WHERE UserID=@u40 AND BagType=0 AND Place BETWEEN 0 AND 15;

  INSERT INTO Project_Player34.dbo.Sys_Users_Goods
  (UserID, BagType, TemplateID, Place, Count, IsJudge, Color, IsExist, StrengthenLevel, StrengthenExp,
   AttackCompose, DefendCompose, LuckCompose, AgilityCompose, Skin, IsBinds, IsUsed, BeginDate, ValidDate,
   RemoveDate, RemoveType, Hole1, Hole2, Hole3, Hole4, Hole5, Hole6, Hole5Level, Hole5Exp, Hole6Level, Hole6Exp,
   StrengthenTimes, StrengthenRefineryLevel, isGold, goldValidDate, goldBeginTime, Blood,
   latentEnergyCurStr, latentEnergyNewStr, latentEnergyEndTime, curExp, cellLocked)
  SELECT @u40, 0, TemplateID, Place, 1, 1, N'', 1, 12, 0,
         50, 50, 50, 50, N'', 1, 1, GETDATE(), 0,
         GETDATE(), 0, -1,-1,-1,-1,-1,-1, 0,0,0,0, 0,0, 0,0, GETDATE(), 0,
         N'0,0,0,0', N'0,0,0,0', GETDATE(), 0, 0
  FROM #SafeEquip;

  UPDATE Project_Player34.dbo.Sys_Users_Goods
  SET Hole1=-1, Hole2=-1, Hole3=-1, Hole4=-1, Hole5=-1, Hole6=-1
  WHERE UserID=@u40;

  UPDATE Project_Player34.dbo.Sys_Users_Texp
  SET attTexpExp=500000, defTexpExp=500000, spdTexpExp=500000, lukTexpExp=500000, hpTexpExp=500000,
      texpCount=200, texpTaskCount=0
  WHERE UserID=@u40;

  IF @@ROWCOUNT=0
    INSERT INTO Project_Player34.dbo.Sys_Users_Texp (UserID, spdTexpExp, attTexpExp, defTexpExp, hpTexpExp, lukTexpExp, texpTaskCount, texpCount, texpTaskDate)
    VALUES (@u40, 500000, 500000, 500000, 500000, 500000, 0, 200, GETDATE());

  UPDATE Project_Player34.dbo.Sys_Users_Detail
  SET Style=@Style60, Colors=N',,,,,,,,,,,,,,,,', State=0, FightPower=0
  WHERE UserID=@u40;
END

SELECT 'equip' AS t, UserID, Place, TemplateID, StrengthenLevel, Hole1
FROM Project_Player34.dbo.Sys_Users_Goods
WHERE UserID IN (@admin,@u40) AND BagType=0 AND Place<=15
ORDER BY UserID, Place;

SELECT 'texp' AS t, * FROM Project_Player34.dbo.Sys_Users_Texp WHERE UserID IN (@admin,@u40);
