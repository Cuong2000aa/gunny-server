using System;
using System.Collections.Generic;
using Game.Logic.AI;
using Game.Logic.Phy.Object;

namespace GameServerScript.AI.NPC
{
    /// <summary>Kiến lính — thoại riêng (không dùng chat Gà).</summary>
    public class AntNpcAi : ABrain
    {
        protected Player m_targer;

        private static readonly Random random = new Random();
        private static readonly string[] listChat = new string[]
        {
            "Bảo vệ Nữ Vương! Không cho chúng tiến sâu!",
            "Kẻ xâm lược… tiêu diệt hết!",
            "Hang kiến là nhà của chúng ta!",
            "Vì tổ kiến! Xông lên!",
            "Ngửi thấy mùi thịt sống…",
            "Đừng để chúng chạm đến trứng!",
            "Đội hình kiến bất khả xâm phạm!",
            "Chi viện từ sâu hang đang tới!",
            "Cắn đứt chân chúng!",
            "Nữ Vương đang chờ… mau chặn chúng!"
        };

        public override void OnBeginNewTurn()
        {
            base.OnBeginNewTurn();
            m_body.CurrentDamagePlus = 1;
            m_body.CurrentShootMinus = 1;
            if (m_body.IsSay)
            {
                m_body.Say(GetOneChat(), 0, Game.Random.Next(0, 5000));
            }
        }

        public override void OnStartAttacking()
        {
            base.OnStartAttacking();
            m_targer = Game.FindNearestPlayer(Body.X, Body.Y);
            Beating();
        }

        public void MoveToPlayer(Player player)
        {
            if (player == null) return;
            int dis = (int)player.Distance(Body.X, Body.Y);
            int ramdis = Game.Random.Next(((SimpleNpc)Body).NpcInfo.MoveMin, ((SimpleNpc)Body).NpcInfo.MoveMax);
            if (dis > 97)
            {
                if (dis > ((SimpleNpc)Body).NpcInfo.MoveMax)
                    dis = ramdis;
                else
                    dis = dis - 90;

                if (player.X > Body.X)
                    Body.MoveTo(Body.X + dis, Body.Y, "walk", 1200, new LivingCallBack(MoveBeat));
                else
                    Body.MoveTo(Body.X - dis, Body.Y, "walk", 1200, new LivingCallBack(MoveBeat));
            }
        }

        public void MoveBeat()
        {
            Body.Beat(m_targer, "beat", 100, 0, 0);
        }

        public void Beating()
        {
            if (m_targer != null && !Body.Beat(m_targer, "beat", 100, 0, 0))
                MoveToPlayer(m_targer);
        }

        public static string GetOneChat()
        {
            return listChat[random.Next(0, listChat.Length)];
        }

        public static void LivingSay(List<Living> livings)
        {
            if (livings == null || livings.Count == 0) return;
            foreach (Living living in livings) living.IsSay = false;
            int livCount = livings.Count;
            int sayCount = livCount <= 5 ? random.Next(0, 2) : (livCount <= 10 ? random.Next(1, 3) : random.Next(1, 4));
            for (int i = 0; i < sayCount;)
            {
                int index = random.Next(0, livCount);
                if (livings[index].IsSay) continue;
                livings[index].IsSay = true;
                livings[index].Say(GetOneChat(), 0, random.Next(0, 5000));
                i++;
            }
        }
    }
}
