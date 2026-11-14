-- Admin (UserID=1) legend equip bag: best weapons, 4 offhands, jewelry, clothes — all +15 / compose 50 / hole lv5.
-- Run AFTER seed-max-users.sql, seed-bags-cards-pets.sql, fix-stats-and-safe-gear.sql (they wipe UserID=1 goods).
-- Idempotent: skips TemplateIDs Admin already owns, fills free equip-bag places 31..126.
-- Every TemplateID here must exist in Tank.Request/TemplateAlllist.xml, otherwise the client
-- crashes on hall enter (StoreModel.isProperTo_CanEmbedEquipList null Hole) => yellow screen.
-- Weapons must have a BallConfig row in Project_Game34 (missing => match loading NPE).
SET NOCOUNT ON;
USE Project_Player34;

DECLARE @uid INT = 1;

CREATE TABLE #Want (ord INT IDENTITY(1,1), TemplateID INT NOT NULL);
INSERT INTO #Want(TemplateID) VALUES
-- Weapons (Category 7, LEGEND tier 4)
(72244),(72224),(72204),(72154),(72114),(72174),(71884),(71824),(71924),(75494),(71844),(71784),
-- Offhands (Category 17): shield, attack, special, heal
(17016),(17015),(17019),(17011),
-- Bracelets (8 / 28)
(85204),(85374),(85194),(85184),(8615),(8614),
-- Rings (9 / 29)
(95354),(95374),(95304),(95294),(9625),(9623),
-- Necklaces (14)
(14052),(14050),(14046),
-- Suits (13)
(13726),(13912),(13860),(13840),(13839),
-- Clothes (5, male/unisex)
(5527),(5734),(5528),(5950),
-- Hats (1)
(11000029),(11000501),(11000500),
-- Glasses (2)
(2310),(2311),
-- Wings (15)
(15137),(15149),(15140);

DELETE w FROM #Want w
WHERE EXISTS (SELECT 1 FROM Sys_Users_Goods g WHERE g.UserID=@uid AND g.IsExist=1 AND g.TemplateID=w.TemplateID)
   OR NOT EXISTS (SELECT 1 FROM Project_Game34.dbo.Shop_Goods s WHERE s.TemplateID=w.TemplateID);

;WITH n AS (SELECT TOP 127 ROW_NUMBER() OVER (ORDER BY (SELECT 1)) - 1 AS p FROM sys.all_objects),
free AS (
  SELECT p, ROW_NUMBER() OVER (ORDER BY p) AS rn FROM n
  WHERE p BETWEEN 31 AND 126
    AND p NOT IN (SELECT Place FROM Sys_Users_Goods WHERE UserID=@uid AND BagType=0 AND IsExist=1)
),
want AS (SELECT TemplateID, ROW_NUMBER() OVER (ORDER BY ord) AS rn FROM #Want)
INSERT INTO Sys_Users_Goods
(UserID, BagType, TemplateID, Place, Count, IsJudge, Color, IsExist, StrengthenLevel, StrengthenExp,
 AttackCompose, DefendCompose, LuckCompose, AgilityCompose, Skin, IsBinds, IsUsed, BeginDate, ValidDate,
 RemoveDate, RemoveType, Hole1, Hole2, Hole3, Hole4, Hole5, Hole6, Hole5Level, Hole5Exp, Hole6Level, Hole6Exp,
 StrengthenTimes, StrengthenRefineryLevel, isGold, goldValidDate, goldBeginTime, Blood,
 latentEnergyCurStr, latentEnergyNewStr, latentEnergyEndTime, curExp, cellLocked)
SELECT @uid, 0, w.TemplateID, f.p, 1, 1, N'', 1, 15, 0,
 50, 50, 50, 50, N'', 1, 1, GETDATE(), 0,
 GETDATE(), 0, 0, 0, 0, 0, -1, -1, 5, 0, 5, 0,
 0, 0, 0, 0, GETDATE(), 1000,
 N'0,0,0,0', N'0,0,0,0', GETDATE(), 0, 0
FROM want w JOIN free f ON f.rn = w.rn;

SELECT @@ROWCOUNT AS legend_items_added,
       (SELECT COUNT(*) FROM Sys_Users_Goods WHERE UserID=@uid AND BagType=0 AND IsExist=1) AS admin_equip_bag_total;

DROP TABLE #Want;
