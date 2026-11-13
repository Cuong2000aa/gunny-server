using System.Collections.Generic;
using Game.Logic;
using Game.Logic.AI;
using Game.Logic.Phy.Object;

namespace GameServerScript.AI.Messions
{
    public class AC1243 : AMissionControl
    {
        private SimpleBoss m_boss;
        private PhysicalObj m_moive;
        private int bossID = 1243;
        private int IsSay = 0;

        public override int CalculateScoreGrade(int score)
        {
            base.CalculateScoreGrade(score);
            if (score > 900) return 3;
            if (score > 825) return 2;
            if (score > 725) return 1;
            return 0;
        }

        public override void OnPrepareNewSession()
        {
            base.OnPrepareNewSession();
            Game.AddLoadingFile(1, "bombs/61.swf", "tank.resource.bombs.Bomb61");
            Game.AddLoadingFile(2, "image/game/thing/BossBornBgAsset.swf", "game.asset.living.BossBgAsset");
            int[] resources = { bossID };
            Game.LoadResources(resources);
            Game.LoadNpcGameOverResources(resources);
            Game.SetMap(1243);
            Game.IsBossWar = "Rồng Cổ";
        }

        public override void OnStartGame()
        {
            base.OnStartGame();
            m_moive = Game.Createlayer(0, 0, "moive", "game.asset.living.BossBgAsset", "out", 1, 0);
            m_boss = Game.CreateBoss(bossID, 1000, -100, -1, 1);
            m_boss.FallFrom(1000, 500, "fall", 0, 2, 1000);
            m_boss.SetRelateDemagemRect(-50, -180, 100, 180);
            m_boss.AddDelay(10);
            m_boss.Say("Đảo Rồng Cổ — sinh vật ngàn năm thức tỉnh! Chỉ đội mạnh nhất mới sống sót.", 0, 2500);
            m_moive.PlayMovie("in", 5000, 0);
            m_moive.PlayMovie("out", 9000, 0);
            Game.BossCardCount = 1;
            foreach (Player p in Game.GetAllFightPlayers())
            {
                p.PlayerDetail.SendMessage("Đảo Rồng Cổ — sinh vật ngàn năm thức tỉnh! Chỉ đội mạnh nhất mới sống sót.");
            }
        }

        public override void OnBeginNewTurn()
        {
            base.OnBeginNewTurn();
            IsSay = 0;
            if (Game.TurnIndex > 1 && m_moive != null)
            {
                Game.RemovePhysicalObj(m_moive, true);
                m_moive = null;
            }
        }

        public override bool CanGameOver()
        {
            if (Game.TurnIndex > Game.MissionInfo.TotalTurn - 1) return true;
            return m_boss != null && m_boss.IsLiving == false;
        }

        public override int UpdateUIData()
        {
            if (m_boss == null) return 0;
            return m_boss.IsLiving ? 0 : 1;
        }

        public override void OnGameOver()
        {
            base.OnGameOver();
            Game.IsWin = m_boss != null && m_boss.IsLiving == false;
            var loading = new List<LoadingFileInfo>();
            loading.Add(new LoadingFileInfo(2, "image/map/show7.jpg", ""));
            Game.SendLoadResource(loading);
        }

        public override void OnShooted()
        {
            if (m_boss != null && m_boss.IsLiving && IsSay == 0)
            {
                m_boss.Say("Gầm... máu cổ đại sôi lên!", 0, 800);
                IsSay = 1;
            }
        }
    }
}
