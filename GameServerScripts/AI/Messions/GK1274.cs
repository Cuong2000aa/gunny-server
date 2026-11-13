using System;
using System.Collections.Generic;
using System.Text;
using Game.Logic.AI;
using Game.Logic.Phy.Object;
using Game.Logic;

namespace GameServerScript.AI.Messions
{
    public class GK1274 : AMissionControl
    {
        private SimpleBoss m_king = null;

        SimpleBoss m_preKing = null;

        private int m_kill = 0;

        private int IsSay = 0;

        private int bossID = 1207;

        private int npcID = 1204;

        private static string[] KillChat = new string[]{
           "灭亡是你唯一的归宿！",                  
 
            "太不堪一击了！"
        };

        private static string[] ShootedChat = new string[]{
            "哎哟～你打的我好疼啊！<br/>啊哈哈哈哈！",
               
            "你们就只有这点本事？！",
               
            "哼～有点意思了"
        };

        public override int CalculateScoreGrade(int score)
        {
            base.CalculateScoreGrade(score);
            if (score > 900)
            {
                return 3;
            }
            else if (score > 825)
            {
                return 2;
            }
            else if (score > 725)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public override void OnPrepareNewSession()
        {
            base.OnPrepareNewSession();
            int[] resources = { npcID, bossID };
            Game.LoadResources(resources);
            Game.LoadNpcGameOverResources(resources);
            Game.SetMap(1076);
            Game.IsBossWar = "炸弹人王";
        }

        public override void OnStartGame()
        {
            base.OnStartGame();

            m_king = Game.CreateBoss(bossID, 750, 510, -1, 0);
            m_king.SetRelateDemagemRect(-41, -187, 83, 140);
            m_king.AddDelay(16);
            /*
            int i = 0;
            foreach (Player player in Game.GetAllFightPlayers())
            {
                player.Direction = 1;
                player.SetXY((i + 1) * 100, 520);
                i++;
            }
            m_preKing = Game.CreateBoss(1205, 600, 510, -1, 0);
            m_preKing.Say("Á~~ Đầu ta đau quá... Sao ta lại ở đây!? Vua Bom Matthias đâu rồi?... Ta nhớ ra rồi.", 1000);
            m_preKing.Say("Cảm ơn các dũng sĩ! Ta là Quốc Vương Gà Nieruo. Không có các ngươi, ta không bao giờ thoát khỏi sự khống chế của Matthias.", 3000);
            m_preKing.Say("Nhiều năm trước, tộc Bom tấn công Vương quốc Gà. Trong một trận chiến, ta trúng mưu của Vua Bom Matthias và bị lời nguyền khóa tinh thần.", 5000);
            m_preKing.Say("Trong trận vừa rồi, lời nguyền đã vỡ, ta lấy lại tự do! Thời gian không còn nhiều — hắn chắc chắn cảm nhận được và đang lao tới đây!", 7000);
            m_preKing.Say("Matthias cực kỳ mạnh. Giờ ta còn yếu, khó chống nổi lâu. Trước khi hắn tới, hãy mau rời khỏi đây!", 9000);
             */
        }

        public override void OnNewTurnStarted()
        {
            base.OnNewTurnStarted();
            /*
            m_king = Game.CreateBoss(1207, 750, 510, -1, 0);
            m_king.SetRelateDemagemRect(-41, -187, 83, 140);
            m_king.Say("Nieruo... không ngờ ngươi phá được khóa tinh thần của ta!", 3000);
            m_king.Say("Hóa ra vậy... nhờ ngoại lực. Ta cho ngươi cơ hội cuối: đầu hàng ta, hoặc bị đày vào hư không chịu khổ mãi mãi?", 3000);
            m_king.AddDelay(16);
             */
        }

        public override void OnBeginNewTurn()
        {
            base.OnBeginNewTurn();
            IsSay = 0;
        }

        public override bool CanGameOver()
        {

            if (m_king.IsLiving == false)
            {
                m_kill++;
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
            return m_kill;
        }

        public override void OnGameOver()
        {
            base.OnGameOver();
            bool IsAllPlayerDie = true;
            foreach (Player player in Game.GetAllFightPlayers())
            {
                if (player.IsLiving == true)
                {
                    IsAllPlayerDie = false;
                }
            }
            if (m_king.IsLiving == false && IsAllPlayerDie == false)
            {
                Game.IsWin = true;
            }
            else
            {
                Game.IsWin = false;
            }

            //List<LoadingFileInfo> loadingFileInfos = new List<LoadingFileInfo>();
            //loadingFileInfos.Add(new LoadingFileInfo(2, "image/map/8", ""));
            //Game.SendLoadResource(loadingFileInfos);
        }

        public override void DoOther()
        {
            base.DoOther();

            int index = Game.Random.Next(0, KillChat.Length);
            m_king.Say(KillChat[index], 0, 0);
        }

        public override void OnShooted()
        {
            if (m_king.IsLiving && IsSay == 0)
            {
                int index = Game.Random.Next(0, ShootedChat.Length);
                m_king.Say(ShootedChat[index], 0, 1500);
                IsSay = 1;
            }

        }


    }
}
