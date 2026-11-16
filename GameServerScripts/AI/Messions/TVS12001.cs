using System.Collections.Generic;
using Game.Logic;
using Game.Logic.AI;
using Game.Logic.Phy.Object;

namespace GameServerScript.AI.Messions
{
    public class TVS12001 : AMissionControl
    {
        private List<SimpleNpc> _npcs = new List<SimpleNpc>();
        private int[] npcIDs = { 12001, 12002 };
        private int[] spawnX = { 720, 800, 880, 960, 1040, 1100 };
        private int totalNeed = 20;

        public override void OnPrepareNewSession()
        {
            base.OnPrepareNewSession();
            Game.LoadResources(npcIDs);
            Game.LoadNpcGameOverResources(npcIDs);
            Game.SetMap(12001);
            Game.IsBossWar = "Bão thời không";
        }

        public override void OnStartGame()
        {
            base.OnStartGame();
            foreach (Player p in Game.GetAllFightPlayers())
                p.PlayerDetail.SendMessage("Hạ hết sói thời không!");
            totalNeed = Game.MissionInfo != null && Game.MissionInfo.TotalCount > 0 ? Game.MissionInfo.TotalCount : 20;
            if (totalNeed > 40) totalNeed = 20;

            int initial = totalNeed < 6 ? totalNeed : 6;
            for (int i = 0; i < initial; i++)
            {
                int id = npcIDs[i % npcIDs.Length];
                int x = spawnX[i % spawnX.Length];
                _npcs.Add(Game.CreateNpc(id, x, 520, 1));
            }
        }

        public override void OnNewTurnStarted()
        {
            base.OnNewTurnStarted();
            if (Game.GetLivedLivings().Count == 0) Game.PveGameDelay = 0;

            if (Game.TurnIndex > 1 && Game.CurrentPlayer != null && Game.CurrentPlayer.Delay > Game.PveGameDelay)
            {
                int livingCount = Game.GetLivedLivings().Count;
                if (livingCount < 6 && _npcs.Count < totalNeed)
                {
                    int toSpawn = 6 - livingCount;
                    for (int i = 0; i < toSpawn && _npcs.Count < totalNeed; i++)
                    {
                        int id = npcIDs[Game.Random.Next(0, npcIDs.Length)];
                        int x = spawnX[Game.Random.Next(0, spawnX.Length)];
                        _npcs.Add(Game.CreateNpc(id, x, 520, 1));
                    }
                }
            }
        }

        public override bool CanGameOver()
        {
            base.CanGameOver();
            if (_npcs.Count >= totalNeed && Game.GetLivedLivings().Count == 0)
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
            Game.IsWin = _npcs.Count >= totalNeed && Game.GetLivedLivings().Count == 0;
            var loading = new List<LoadingFileInfo>();
            loading.Add(new LoadingFileInfo(2, "image/map/12002/show2.jpg", ""));
            Game.SendLoadResource(loading);
        }
    }
}
