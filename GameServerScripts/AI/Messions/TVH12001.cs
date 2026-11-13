using System.Collections.Generic;
using Game.Logic;
using Game.Logic.AI;
using Game.Logic.Phy.Object;

namespace GameServerScript.AI.Messions
{
    public class TVH12001 : AMissionControl
    {
        private List<SimpleNpc> _npcs = new List<SimpleNpc>();
        private int[] npcIDs = { 12201, 12202 };

        public override void OnPrepareNewSession()
        {
            base.OnPrepareNewSession();
            Game.LoadResources(npcIDs);
            Game.LoadNpcGameOverResources(npcIDs);
            Game.SetMap(12001);
            Game.IsBossWar = "Bão thời không (Khó)";
        }

        public override void OnStartGame()
        {
            base.OnStartGame();
            foreach (Player p in Game.GetAllFightPlayers())
                p.PlayerDetail.SendMessage("Sói khó!");
            int need = Game.MissionInfo != null && Game.MissionInfo.TotalCount > 0 ? Game.MissionInfo.TotalCount : 20;
            if (need > 40) need = 20;
            for (int i = 0; i < need; i++)
            {
                int id = npcIDs[i % npcIDs.Length];
                int x = 700 + (i % 8) * 90;
                int y = 480 + (i % 3) * 20;
                _npcs.Add(Game.CreateNpc(id, x, y, 1));
            }
        }

        public override void OnNewTurnStarted()
        {
            base.OnNewTurnStarted();
            if (Game.GetLivedLivings().Count == 0) Game.PveGameDelay = 0;
        }

        public override bool CanGameOver()
        {
            if (Game.GetLivedLivings().Count == 0)
            {
                Game.IsWin = true;
                return true;
            }
            if (Game.MissionInfo != null && Game.TurnIndex > Game.MissionInfo.TotalTurn - 1) return true;
            return false;
        }

        public override int UpdateUIData() => Game.TotalKillCount;

        public override void OnGameOver()
        {
            base.OnGameOver();
            Game.IsWin = Game.GetLivedLivings().Count == 0;
        }
    }
}
