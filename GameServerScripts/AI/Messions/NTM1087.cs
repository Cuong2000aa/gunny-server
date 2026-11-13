using System.Collections.Generic;
using Game.Logic;
using Game.Logic.AI;
using Game.Logic.Phy.Object;

namespace GameServerScript.AI.Messions
{
    public class NTM1087 : AMissionControl
    {
        private List<SimpleNpc> someNpc = new List<SimpleNpc>();
        public override void OnPrepareNewSession()
        {
            base.OnPrepareNewSession();
            int[] resources = { 1001 };
            Game.LoadResources(resources);
            Game.LoadNpcGameOverResources(resources);
            Game.SetMap(1084);
        }
        public override void OnStartGame()
        {
            base.OnStartGame();
            for (int i = 0; i < 6; i++)
            {
                someNpc.Add(Game.CreateNpc(1001, 900 + i * 80, 500, 1));
            }
        }
        public override bool CanGameOver()
        {
            foreach (var n in someNpc) if (n != null && n.IsLiving) return false;
            Game.IsWin = true;
            return true;
        }
        public override int UpdateUIData() { return Game.TotalKillCount; }
        public override void OnGameOver() { base.OnGameOver(); Game.IsWin = Game.GetLivedLivings().Count == 0; }
    }
}
