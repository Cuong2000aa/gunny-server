using Game.Logic.AI;
using Game.Logic.Phy.Object;

namespace GameServerScript.AI.NPC
{
    public class ThirdNormalBloodNpc : ABrain
    {
        private Player m_targer;
        public override void OnBeginNewTurn()
        {
            base.OnBeginNewTurn();
            Body.CurrentDamagePlus = 1;
            Body.CurrentShootMinus = 1;
        }
        public override void OnStartAttacking()
        {
            base.OnStartAttacking();
            m_targer = Game.FindNearestPlayer(Body.X, Body.Y);
            if (m_targer == null) return;
            if (!Body.Beat(m_targer, "beat", 100, 0, 0))
            {
                int dis = (int)m_targer.Distance(Body.X, Body.Y);
                if (dis > 90)
                {
                    int step = Game.Random.Next(80, 160);
                    int nx = m_targer.X > Body.X ? Body.X + step : Body.X - step;
                    Body.MoveTo(nx, Body.Y, "walk", 800, () => Body.Beat(m_targer, "beat", 100, 0, 0));
                }
            }
        }
    }
}
