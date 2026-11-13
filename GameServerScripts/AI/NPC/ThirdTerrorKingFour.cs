using Game.Logic.AI;
using Game.Logic.Phy.Object;

namespace GameServerScript.AI.NPC
{
    public class ThirdTerrorKingFour : ABrain
    {
        private int m_attackTurn = 0;
        public override void OnBeginNewTurn()
        {
            base.OnBeginNewTurn();
            Body.CurrentDamagePlus = 1;
            Body.CurrentShootMinus = 1;
        }
        public override void OnCreated()
        {
            base.OnCreated();
            Body.Say("Sulan hình thái tối thượng!", 0, 2000);
        }
        public override void OnStartAttacking()
        {
            base.OnStartAttacking();
            Body.Direction = Game.FindlivingbyDir(Body);
            Player nearest = Game.FindNearestPlayer(Body.X, Body.Y);
            if (nearest != null && Body.Distance(nearest.X, nearest.Y) < 180)
            {
                Body.CurrentDamagePlus = 1.6f;
                Body.Beat(nearest, "beat", 100, 0, 0);
                return;
            }
            if (m_attackTurn == 0)
            {
                Body.CurrentDamagePlus = 0.8f;
                Body.Say("Sức mạnh bộ lạc!", 1, 0);
                Body.PlayMovie("beat", 1000, 0);
                Body.RangeAttacking(Body.X - 1000, Body.X + 1000, "cry", 3000, null);
                m_attackTurn++;
            }
            else
            {
                Body.CurrentDamagePlus = 1.0f;
                Player t = Game.FindRandomPlayer();
                if (t != null)
                {
                    int mtX = Game.Random.Next(t.X - 40, t.X + 40);
                    if (Body.ShootPoint(mtX, t.Y, 61, 1000, 10000, 1, 1.0f, 2000))
                        Body.PlayMovie("beat", 1500, 0);
                }
                m_attackTurn = 0;
            }
        }
    }
}
