using System.Collections.Generic;
using Game.Logic;
using Game.Logic.AI;
using Game.Logic.Phy.Object;

namespace GameServerScript.AI.Messions
{
    public class GCGCK1161 : AMissionControl
    {
        private List<SimpleNpc> someNpc = new List<SimpleNpc>();
        private List<SimpleBoss> someBoss = new List<SimpleBoss>();
        private int redTotalCount = 0;
        private int dieRedCount = 0;

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
            int[] resources = { 7201 };
            Game.LoadResources(resources);
            Game.LoadNpcGameOverResources(resources);
            Game.SetMap(1161);
        }

        public override void OnStartGame()
        {
            base.OnStartGame();
            
            foreach (Player __p in Game.GetAllFightPlayers())
            {
                __p.PlayerDetail.SendMessage("Độ khó: biển gà con nhiễm độc!");
            }
Game.IsBossWar = "";
            SpawnWave();
        }

        public override void OnNewTurnStarted()
        {
            base.OnNewTurnStarted();
            if (Game.GetLivedLivings().Count == 0)
            {
                Game.PveGameDelay = 0;
            }
            if (!IsBossLiving() && redTotalCount < 30) SpawnWave();
        }

        private void SpawnWave()
        {
            if (redTotalCount >= 30) return;
            for (int i = 0; i < 6; i++)
            {
                if (redTotalCount >= 30) break;
                redTotalCount++;
                int x = 800 + (i % 5) * 120;
                int y = 500;
                someNpc.Add(Game.CreateNpc(7201, x, y, 1));
            }
        }

        private void SpawnBoss()
        {
            someBoss.Add(Game.CreateBoss(7201, 1000, 500, -1, 1));
            redTotalCount++;
        }

        private bool IsBossLiving()
        {
            foreach (SimpleBoss b in someBoss)
            {
                if (b != null && b.IsLiving) return true;
            }
            return false;
        }

        public override bool CanGameOver()
        {
            dieRedCount = 0;
            bool clear = true;
            foreach (SimpleNpc n in someNpc)
            {
                if (n != null && n.IsLiving) clear = false;
                else dieRedCount++;
            }
            foreach (SimpleBoss b in someBoss)
            {
                if (b != null && b.IsLiving) clear = false;
                else dieRedCount++;
            }
            if (clear && redTotalCount >= 30)
            {
                Game.IsWin = true;
                return true;
            }
            if (Game.TurnIndex > Game.MissionInfo.TotalTurn - 1)
            {
                return true;
            }
            return false;
        }

        public override int UpdateUIData()
        {
            return Game.TotalKillCount;
        }

        public override void OnGameOver()
        {
            base.OnGameOver();
            Game.IsWin = Game.GetLivedLivings().Count == 0;
        }
    }
}
