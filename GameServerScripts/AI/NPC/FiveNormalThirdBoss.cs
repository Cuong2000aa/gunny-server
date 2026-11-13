using System;
using Game.Logic.AI;
using Game.Logic.Phy.Object;

namespace GameServerScript.AI.NPC
{
    /// <summary>Dragon-war / world-dragon boss AI (ported pattern from original Gunny captains).</summary>
    public class FiveNormalThirdBoss : ABrain
    {
        private int m_attackTurn = 0;
        private static readonly string[] AllAttackChat = { "Cơn thịnh nộ của khăn hồng!" };
        private static readonly string[] ShootChat = { "Mũi tên bóng tối, xuyên thủng chúng!" };
        private static readonly string[] KillChat = { "Ngươi không thoát khỏi lời nguyền!" };
        private static readonly string[] HurtChat = { "Khăn hồng của ta... bị xé!" };

        public override void OnBeginNewTurn()
        {
            base.OnBeginNewTurn();
            Body.CurrentDamagePlus = 1;
            Body.CurrentShootMinus = 1;
        }

        public override void OnCreated()
        {
            base.OnCreated();
            Body.Say("Ta là Quỷ Khăn Hồng — kẻ canh giữ cánh cửa rồng!", 0, 2000);
        }

        public override void OnStartAttacking()
        {
            base.OnStartAttacking();
            Body.Direction = Game.FindlivingbyDir(Body);
            Player nearest = Game.FindNearestPlayer(Body.X, Body.Y);
            if (nearest != null && Body.Distance(nearest.X, nearest.Y) < 180)
            {
                Body.CurrentDamagePlus = 1.8f;
                Body.Say(KillChat[0], 1, 0);
                Body.Beat(nearest, "beat", 100, 0, 0);
                return;
            }
            if (m_attackTurn == 0)
            {
                Body.CurrentDamagePlus = 0.7f;
                Body.Say(AllAttackChat[0], 1, 0);
                Body.PlayMovie("beat", 1000, 0);
                Body.RangeAttacking(Body.X - 1000, Body.X + 1000, "cry", 3000, null);
                m_attackTurn++;
            }
            else
            {
                Body.CurrentDamagePlus = 1.0f;
                Body.Say(ShootChat[0], 1, 0);
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

        public override void OnKillPlayerSay()
        {
            Body.Say(KillChat[0], 1, 0);
        }

        public override void OnShootedSay(int delay)
        {
            Body.Say(HurtChat[0], 0, delay);
        }
    }
}
