-- Seed Admin Lv60 FULL gear (strengthen +15) + Max40 Lv40 same gear (+12 only).
-- Equip places: cat-1 for 1..7; 8/9 bracelets; 9/10 rings; 11..15 specials.
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Style nvarchar(200) = N'1761,2310,3349,4355,5527,6305,71424,85384,85204,95384,95354,13726,14056,15154,16967,17013';
DECLARE @Colors nvarchar(200) = N',,,,,,,,,,,,,,,,';

------------------------------------------------------------
-- Helper: wipe + insert full equip set for a user
------------------------------------------------------------
-- Templates / places
-- 0:1761 1:2310 2:3349 3:4355 4:5527 5:6305 6:71424
-- 7:85384 8:85204 9:95384 10:95354 11:13726 12:14056 13:15154 14:16967 15:17013

IF OBJECT_ID('tempdb..#Gear') IS NOT NULL DROP TABLE #Gear;
CREATE TABLE #Gear (Place int NOT NULL, TemplateID int NOT NULL);
INSERT INTO #Gear(Place, TemplateID) VALUES
(0,1761),(1,2310),(2,3349),(3,4355),(4,5527),(5,6305),(6,71424),
(7,85384),(8,85204),(9,95384),(10,95354),(11,13726),(12,14056),(13,15154),(14,16967),(15,17013);

-- Extra bag copies (EquipBag inventory slots from 31+)
IF OBJECT_ID('tempdb..#BagExtra') IS NOT NULL DROP TABLE #BagExtra;
CREATE TABLE #BagExtra (TemplateID int NOT NULL);
INSERT INTO #BagExtra(TemplateID) VALUES
(1761),(2310),(3349),(4355),(5527),(6305),(71424),(85384),(85204),(95384),(95354),(13726),(14056),(15154),(16967),(17013),
(7142),(7103),(5528),(1779),(2310),(3349);

------------------------------------------------------------
-- 1) ADMIN (UserID=1) Lv60 max everything, strengthen +15
------------------------------------------------------------
UPDATE Project_Player34.dbo.Sys_Users_Detail
SET Grade = 60,
    GP = 211140995,
    Money = 99999999,
    Gold = 99999999,
    GiftToken = 99999999,
    Medal = 999999,
    Score = 99999999,
    petScore = 99999999,
    damageScores = 99999999,
    myHonor = 99999999,
    AchievementPoint = 999999,
    Offer = 99999999,
    Nimbus = 999,
    NickName = N'Admin',
    Style = @Style,
    Colors = @Colors,
    Skin = N'',
    Hide = N'1111111111',
    Sex = 1,
    State = 0,
    FightPower = 500000,
    IsFirst = 0
WHERE UserID = 1;

UPDATE Project_Player34.dbo.Sys_VIP_Info
SET typeVIP = 2, VIPLevel = 12, VIPExp = 99999999,
    VIPExpireDay = DATEADD(YEAR, 10, GETDATE()),
    VIPLastdate = GETDATE(), VIPNextLevelDaysNeeded = 0, CanTakeVipReward = 1
WHERE UserID = 1;

DELETE FROM Project_Player34.dbo.Sys_Users_Goods WHERE UserID = 1;

INSERT INTO Project_Player34.dbo.Sys_Users_Goods
(UserID, BagType, TemplateID, Place, Count, IsJudge, Color, IsExist, StrengthenLevel, StrengthenExp,
 AttackCompose, DefendCompose, LuckCompose, AgilityCompose, Skin, IsBinds, IsUsed, BeginDate, ValidDate,
 RemoveDate, RemoveType, Hole1, Hole2, Hole3, Hole4, Hole5, Hole6, Hole5Level, Hole5Exp, Hole6Level, Hole6Exp,
 StrengthenTimes, StrengthenRefineryLevel, isGold, goldValidDate, goldBeginTime, Blood,
 latentEnergyCurStr, latentEnergyNewStr, latentEnergyEndTime, curExp, cellLocked)
SELECT 1, 0, g.TemplateID, g.Place, 1, 1, N'', 1, 15, 0,
       80, 80, 80, 80, N'', 1, 1, GETDATE(), 0,
       GETDATE(), 0, 1,1,1,1,1,1, 0,0,0,0,
       0,0, 0,0, GETDATE(), 0,
       N'0,0,0,0', N'0,0,0,0', GETDATE(), 0, 0
FROM #Gear g;

-- bag extras for admin (places 31+)
INSERT INTO Project_Player34.dbo.Sys_Users_Goods
(UserID, BagType, TemplateID, Place, Count, IsJudge, Color, IsExist, StrengthenLevel, StrengthenExp,
 AttackCompose, DefendCompose, LuckCompose, AgilityCompose, Skin, IsBinds, IsUsed, BeginDate, ValidDate,
 RemoveDate, RemoveType, Hole1, Hole2, Hole3, Hole4, Hole5, Hole6, Hole5Level, Hole5Exp, Hole6Level, Hole6Exp,
 StrengthenTimes, StrengthenRefineryLevel, isGold, goldValidDate, goldBeginTime, Blood,
 latentEnergyCurStr, latentEnergyNewStr, latentEnergyEndTime, curExp, cellLocked)
SELECT 1, 0, e.TemplateID, 30 + ROW_NUMBER() OVER (ORDER BY e.TemplateID), 1, 1, N'', 1, 15, 0,
       50, 50, 50, 50, N'', 1, 0, GETDATE(), 0,
       GETDATE(), 0, -1,-1,-1,-1,-1,-1, 0,0,0,0,
       0,0, 0,0, GETDATE(), 0,
       N'0,0,0,0', N'0,0,0,0', GETDATE(), 0, 0
FROM #BagExtra e;

------------------------------------------------------------
-- 2) MAX40 user — same gear, strengthen +12 only
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM Project_Player34.dbo.Sys_Users_Detail WHERE UserName = N'max40')
BEGIN
    INSERT INTO Project_Player34.dbo.Sys_Users_Detail
    (UserName, Password, NickName, [Date], IsConsortia, ConsortiaID, Sex, Win, Total, [Escape],
     GP, Honor, Gold, Money, MoneyLock, Style, Colors, Hide, Grade, State, IsFirst, Repute,
     LastDate, ChargeDate, ExpendDate, ActiveIP, ForbidDate, Skin, Offer, IsExist,
     GiftToken, Nimbus, AchievementPoint, MoneyPlus, Medal, Score, petScore, damageScores, myHonor,
     FightPower, QuestSite)
    VALUES
     (N'max40', N'', N'Max40', GETDATE(), 0, 0, 1, 0, 0, 0,
     14324419, N'', 99999999, 99999999, 0, @Style, @Colors, N'1111111111', 40, 0, 0, 0,
     GETDATE(), GETDATE(), GETDATE(), N'127.0.0.1', '2000-01-01', N'', 99999999, 1,
     99999999, 500, 500000, 0, 999999, 99999999, 99999999, 99999999, 99999999,
     200000, 0x);

END
ELSE
BEGIN
    UPDATE Project_Player34.dbo.Sys_Users_Detail
    SET Grade = 40, GP = 14324419,
        Money = 99999999, Gold = 99999999, GiftToken = 99999999, Medal = 999999,
        Score = 99999999, petScore = 99999999, damageScores = 99999999, myHonor = 99999999,
        AchievementPoint = 500000, Offer = 99999999, Nimbus = 500,
        Style = @Style, Colors = @Colors, Hide = N'1111111111', Sex = 1, State = 0,
        NickName = N'Max40', FightPower = 200000, IsFirst = 0
    WHERE UserName = N'max40';
END

DECLARE @u40 int = (SELECT TOP 1 UserID FROM Project_Player34.dbo.Sys_Users_Detail WHERE UserName = N'max40');

IF NOT EXISTS (SELECT 1 FROM Project_Player34.dbo.Sys_VIP_Info WHERE UserID = @u40)
BEGIN
    SET IDENTITY_INSERT Project_Player34.dbo.Sys_VIP_Info ON;
    INSERT INTO Project_Player34.dbo.Sys_VIP_Info (UserID, typeVIP, VIPLevel, VIPExp, VIPOnlineDays, VIPOfflineDays, VIPExpireDay, LastVIPPackTime, VIPLastdate, VIPNextLevelDaysNeeded, CanTakeVipReward)
    VALUES (@u40, 2, 12, 99999999, 0, 0, DATEADD(YEAR,10,GETDATE()), GETDATE(), GETDATE(), 0, 1);
    SET IDENTITY_INSERT Project_Player34.dbo.Sys_VIP_Info OFF;
END
ELSE
    UPDATE Project_Player34.dbo.Sys_VIP_Info
    SET typeVIP=2, VIPLevel=12, VIPExp=99999999, VIPExpireDay=DATEADD(YEAR,10,GETDATE()), CanTakeVipReward=1
    WHERE UserID=@u40;

IF NOT EXISTS (SELECT 1 FROM Project_Player34.dbo.Sys_Users_Texp WHERE UserID = @u40)
    INSERT INTO Project_Player34.dbo.Sys_Users_Texp (UserID, spdTexpExp, attTexpExp, defTexpExp, hpTexpExp, lukTexpExp, texpTaskCount, texpCount, texpTaskDate)
    VALUES (@u40, 0,0,0,0,0,0,0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Project_Player34.dbo.Sys_Users_Extra WHERE UserID = @u40)
    INSERT INTO Project_Player34.dbo.Sys_Users_Extra (UserID, LastTimeHotSpring, LastFreeTimeHotSpring, MinHotSpring, LastTimeConsortiaPower)
    VALUES (@u40, GETDATE(), GETDATE(), 60, GETDATE());

DELETE FROM Project_Player34.dbo.Sys_Users_Goods WHERE UserID = @u40;

INSERT INTO Project_Player34.dbo.Sys_Users_Goods
(UserID, BagType, TemplateID, Place, Count, IsJudge, Color, IsExist, StrengthenLevel, StrengthenExp,
 AttackCompose, DefendCompose, LuckCompose, AgilityCompose, Skin, IsBinds, IsUsed, BeginDate, ValidDate,
 RemoveDate, RemoveType, Hole1, Hole2, Hole3, Hole4, Hole5, Hole6, Hole5Level, Hole5Exp, Hole6Level, Hole6Exp,
 StrengthenTimes, StrengthenRefineryLevel, isGold, goldValidDate, goldBeginTime, Blood,
 latentEnergyCurStr, latentEnergyNewStr, latentEnergyEndTime, curExp, cellLocked)
SELECT @u40, 0, g.TemplateID, g.Place, 1, 1, N'', 1, 12, 0,
       50, 50, 50, 50, N'', 1, 1, GETDATE(), 0,
       GETDATE(), 0, 1,1,1,1,1,1, 0,0,0,0,
       0,0, 0,0, GETDATE(), 0,
       N'0,0,0,0', N'0,0,0,0', GETDATE(), 0, 0
FROM #Gear g;

INSERT INTO Project_Player34.dbo.Sys_Users_Goods
(UserID, BagType, TemplateID, Place, Count, IsJudge, Color, IsExist, StrengthenLevel, StrengthenExp,
 AttackCompose, DefendCompose, LuckCompose, AgilityCompose, Skin, IsBinds, IsUsed, BeginDate, ValidDate,
 RemoveDate, RemoveType, Hole1, Hole2, Hole3, Hole4, Hole5, Hole6, Hole5Level, Hole5Exp, Hole6Level, Hole6Exp,
 StrengthenTimes, StrengthenRefineryLevel, isGold, goldValidDate, goldBeginTime, Blood,
 latentEnergyCurStr, latentEnergyNewStr, latentEnergyEndTime, curExp, cellLocked)
SELECT @u40, 0, e.TemplateID, 30 + ROW_NUMBER() OVER (ORDER BY e.TemplateID), 1, 1, N'', 1, 12, 0,
       40, 40, 40, 40, N'', 1, 0, GETDATE(), 0,
       GETDATE(), 0, -1,-1,-1,-1,-1,-1, 0,0,0,0,
       0,0, 0,0, GETDATE(), 0,
       N'0,0,0,0', N'0,0,0,0', GETDATE(), 0, 0
FROM #BagExtra e;

------------------------------------------------------------
-- Summary
------------------------------------------------------------
SELECT UserID, UserName, NickName, Grade, Money, Gold, GiftToken FROM Project_Player34.dbo.Sys_Users_Detail WHERE UserName IN (N'admin', N'max40');
SELECT UserID, StrengthenLevel, COUNT(*) AS items
FROM Project_Player34.dbo.Sys_Users_Goods
WHERE UserID IN (1, @u40) AND IsExist=1
GROUP BY UserID, StrengthenLevel
ORDER BY UserID, StrengthenLevel;
SELECT UserID, Place, TemplateID, StrengthenLevel, IsUsed
FROM Project_Player34.dbo.Sys_Users_Goods
WHERE UserID IN (1, @u40) AND Place BETWEEN 0 AND 15 AND IsExist=1
ORDER BY UserID, Place;
