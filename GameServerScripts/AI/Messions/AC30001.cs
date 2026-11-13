using System.Collections.Generic;
using Game.Logic;
using Game.Logic.AI;
using Game.Logic.Phy.Object;

namespace GameServerScript.AI.Messions
{
    public class AC30001 : AMissionControl
    {
        private List<SimpleNpc> _npcs = new List<SimpleNpc>();

        public override void OnPrepareNewSession()
        {
            base.OnPrepareNewSession();
            int[] resources = { 1001 };
            Game.LoadResources(resources);
            Game.LoadNpcGameOverResources(resources);
            Game.SetMap(1072);
        
            Game.IsBossWar = "Bá tước hút máu thức tỉnh — dạng đầu!";
}

        public override void OnStartGame()
        {
            base.OnStartGame();
            
            foreach (Player __p in Game.GetAllFightPlayers())
            {
                __p.PlayerDetail.SendMessage("Bá tước hút máu thức tỉnh — dạng đầu!");
            }
int need = Game.MissionInfo != null && Game.MissionInfo.TotalCount > 0 ? Game.MissionInfo.TotalCount : 6;
            if (need > 20) need = 12;
            for (int i = 0; i < need; i++)
            {
                int x = 850 + (i % 6) * 110;
                int y = 480 + (i % 3) * 20;
                _npcs.Add(Game.CreateNpc(1001, x, y, 1));
            }
        }

        public override void OnNewTurnStarted()
        {
            base.OnNewTurnStarted();
            if (Game.GetLivedLivings().Count == 0) Game.PveGameDelay = 0;
        }

        public override bool CanGameOver()
        {
            foreach (var n in _npcs)
            {
                if (n != null && n.IsLiving) return false;
            }
            if (Game.GetLivedLivings().Count == 0)
            {
                Game.IsWin = true;
                return true;
            }
            if (Game.MissionInfo != null && Game.TurnIndex > Game.MissionInfo.TotalTurn - 1) return true;
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
