using System.Collections.Generic;
using Game.Logic;
using Game.Logic.AI;
using Game.Logic.Phy.Object;

namespace GameServerScript.AI.Messions
{
    public class CrosairBoss : AMissionControl
    {
        private List<SimpleBoss> _bosses = new List<SimpleBoss>();
        private int[] bossIDs = { 72000 };

        public override void OnPrepareNewSession()
        {
            base.OnPrepareNewSession();
            Game.AddLoadingFile(1, "bombs/61.swf", "tank.resource.bombs.Bomb61");
            Game.AddLoadingFile(2, "image/game/thing/BossBornBgAsset.swf", "game.asset.living.BossBgAsset");
            Game.LoadResources(bossIDs);
            Game.LoadNpcGameOverResources(bossIDs);
            Game.SetMap(12016);
            Game.IsBossWar = "Hải Tặc Trưởng";
        }

        public override void OnStartGame()
        {
            base.OnStartGame();
            foreach (Player p in Game.GetAllFightPlayers())
                p.PlayerDetail.SendMessage("Hạ Hải Tặc Trưởng để lấy kho báu!");
            var b0 = Game.CreateBoss(72000, 900, -80, -1, 1);
            b0.FallFrom(900, 500, "fall", 0, 2, 1000);
            b0.SetRelateDemagemRect(-42, -180, 84, 180);
            _bosses.Add(b0);
            Game.BossCardCount = 1;
        }

        public override bool CanGameOver()
        {
            if (Game.MissionInfo != null && Game.TurnIndex > Game.MissionInfo.TotalTurn - 1) return true;
            foreach (var b in _bosses)
                if (b != null && b.IsLiving) return false;
            return _bosses.Count > 0;
        }

        public override int UpdateUIData()
        {
            int dead = 0;
            foreach (var b in _bosses) if (b != null && !b.IsLiving) dead++;
            return dead;
        }

        public override void OnGameOver()
        {
            base.OnGameOver();
            bool win = _bosses.Count > 0;
            foreach (var b in _bosses) if (b != null && b.IsLiving) win = false;
            Game.IsWin = win;
        }
    }
}
