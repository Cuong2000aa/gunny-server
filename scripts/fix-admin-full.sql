-- Apply after restoring Database/*.bak — Admin full resources (safe max level = 60).
-- LevelInfo in this DB only goes to Grade 60; Grade 65 causes client error 1033.

SET NOCOUNT ON;

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
    Style = N'1142,,3158,,5160,6103,7008,,,,,,,,,',
    Colors = N',,,,,,,,,,,,,,,',
    Skin = N'',
    Hide = N'1111111111',
    Sex = 1,
    State = 0,
    FightPower = 684
WHERE UserID = 1;

UPDATE Project_Player34.dbo.Sys_VIP_Info
SET typeVIP = 2,
    VIPLevel = 12,
    VIPExp = 99999999,
    VIPExpireDay = DATEADD(YEAR, 10, GETDATE()),
    VIPLastdate = GETDATE(),
    VIPNextLevelDaysNeeded = 0,
    CanTakeVipReward = 1
WHERE UserID = 1;

DELETE FROM Project_Player34.dbo.Sys_Users_Goods WHERE UserID = 1;

INSERT INTO Project_Player34.dbo.Sys_Users_Goods
(UserID, BagType, TemplateID, Place, Count, IsJudge, Color, IsExist, StrengthenLevel, StrengthenExp,
 AttackCompose, DefendCompose, LuckCompose, AgilityCompose, Skin, IsBinds, IsUsed, BeginDate, ValidDate,
 RemoveDate, RemoveType, Hole1, Hole2, Hole3, Hole4, Hole5, Hole6, Hole5Level, Hole5Exp, Hole6Level, Hole6Exp,
 StrengthenTimes, StrengthenRefineryLevel, isGold, goldValidDate, goldBeginTime, Blood,
 latentEnergyCurStr, latentEnergyNewStr, latentEnergyEndTime, curExp, cellLocked)
VALUES
(1, 0, 1142, 0, 1, 1, N'', 1, 0, 0, 0, 0, 0, 0, N'', 1, 1, GETDATE(), 0, GETDATE(), 0, -1,-1,-1,-1,-1,-1, 0,0,0,0, 0,0, 0,0, GETDATE(), 0, N'0,0,0,0', N'0,0,0,0', GETDATE(), 0, 0),
(1, 0, 3158, 2, 1, 1, N'', 1, 0, 0, 0, 0, 0, 0, N'', 1, 1, GETDATE(), 0, GETDATE(), 0, -1,-1,-1,-1,-1,-1, 0,0,0,0, 0,0, 0,0, GETDATE(), 0, N'0,0,0,0', N'0,0,0,0', GETDATE(), 0, 0),
(1, 0, 5160, 4, 1, 1, N'', 1, 0, 0, 0, 0, 0, 0, N'', 1, 1, GETDATE(), 0, GETDATE(), 0, -1,-1,-1,-1,-1,-1, 0,0,0,0, 0,0, 0,0, GETDATE(), 0, N'0,0,0,0', N'0,0,0,0', GETDATE(), 0, 0),
(1, 0, 6103, 5, 1, 1, N'', 1, 0, 0, 0, 0, 0, 0, N'', 1, 1, GETDATE(), 0, GETDATE(), 0, -1,-1,-1,-1,-1,-1, 0,0,0,0, 0,0, 0,0, GETDATE(), 0, N'0,0,0,0', N'0,0,0,0', GETDATE(), 0, 0),
(1, 0, 7008, 6, 1, 1, N'', 1, 0, 0, 0, 0, 0, 0, N'', 1, 1, GETDATE(), 0, GETDATE(), 0, -1,-1,-1,-1,-1,-1, 0,0,0,0, 0,0, 0,0, GETDATE(), 0, N'0,0,0,0', N'0,0,0,0', GETDATE(), 0, 0);

SELECT UserID, NickName, Grade, Money, Gold, GiftToken FROM Project_Player34.dbo.Sys_Users_Detail WHERE UserID = 1;
SELECT typeVIP, VIPLevel FROM Project_Player34.dbo.Sys_VIP_Info WHERE UserID = 1;
SELECT COUNT(*) AS goods FROM Project_Player34.dbo.Sys_Users_Goods WHERE UserID = 1;
