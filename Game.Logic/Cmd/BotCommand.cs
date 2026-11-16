using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bussiness.Managers;
using Game.Base.Packets;
using Game.Logic;
using Game.Logic.Phy.Object;
using SqlDataProvider.Data;

namespace Game.Logic.Cmd
{
    [GameCommand((byte)eTankCmdType.BOT_COMMAND, "战胜关卡中Boss翻牌")]
    public class BotCommand : ICommandHandler
    {
        public void HandleCommand(BaseGame game, Player player, GSPacketIn packet)
        {

            if (game is PVPGame)
            {
                PVPGame pvp = game as PVPGame;
                List<Player> players = pvp.GetAllLivingPlayers();
                List<Player> enemies = new List<Player>();
                foreach (Player child in players)
                {
                    if (child.Team != player.Team)
                    {
                        enemies.Add(child);
                    }
                }

                List<String> randomText1 = new List<string>(new[] {
                    "Nhớ nyc quá ta~",
                    "Xin game nhé!",
                    "Đào nhẹ cái nè",
                    "Bắn như vậy mà đòi thắng à?",
                    "T1 vô địch",
                    "Nhìn ta mà học hỏi nè!",
                    "Chơi ăn gian quá điiii",
                    "Hụt rồi bạn à",
                    "Mạnh vậy ta",
                    "Thua nè",
                    "Non quá đi thoy",
                });

                List<String> randomText2 = new List<string>(new[] {
                    "Thoát game đi tôi không muốn hành hạ bạn đâu",
                    "Chấp vậy rồi mà...",
                    "Kết thúc trận này đi thoy",
                    "Gunny II muôn năm!!!",
                    "Ax Bruh",
                    "Bình tĩnh người anh em, chúng ta có thể giảng hoà không",
                    "Chán thế bạn ơi",
                    "Phía bên kia đã có người iu chưa 😘",
                    "Adu vjp",
                    "Sợ quá cơ",
                });

                Random rand = new Random();
                int next = rand.Next(0, enemies.Count);
                Player target = enemies.ElementAt(next);

                Random randchat = new Random();
                int chatI = randchat.Next(0, 3);
                if (chatI == 1)
                {
                    int b1 = target.TotalShootCount;
                    if (b1 < randomText1.Count)
                        game.SendChat(player.PlayerDetail, randomText1[b1]);
                }
                else if (chatI == 2)
                {
                    int b2 = target.TotalShootCount;
                    if (b2 < randomText2.Count)
                        game.SendChat(player.PlayerDetail, randomText2[b2]);
                }

                if (target.X > player.X)
                {
                    player.ChangeDirection(1, 500);
                }
                else
                {
                    player.ChangeDirection(-1, 500);
                }

                Random rd = new Random();
                int nt = rd.Next(0, 3);
                int a = 10001;
                int b = 10004;
                int c = 10008;
                int d = 0; //x
                int e = 0; //y
                int k = 1;
                float time_s = 1.0f;
                int boomcount = 1;
                int delayy = 1000;
                // Quyết định phát bắn lượt này:
                // ~65% lượt là TRÚNG (bắn dính thẳng vào người mục tiêu)
                // ~35% lượt là HỤT (lệch hẳn ra ngoài)
                bool isHitTurn = rd.Next(0, 100) < 65;
                void DetermineAim()
                {
                    if (isHitTurn)
                    {
                        // Trúng đích: nhắm chuẩn vào người đối thủ (dung sai rất nhỏ +-8px quanh tâm)
                        d = target.X + rd.Next(-8, 9);
                        e = target.Y + rd.Next(-10, 5);
                    }
                    else
                    {
                        // Hụt: bắn lệch rõ ràng ra trước hoặc sau đối thủ
                        int missOffset = (rd.Next(0, 2) == 0 ? 1 : -1) * rd.Next(130, 260);
                        d = target.X + missOffset;
                        e = target.Y + rd.Next(-40, 41);
                    }
                }
                if (Math.Abs(player.X - target.X) > 60)
                {
                    if (nt == 0)
                    {
                        boomcount = 1;
                        a = 0;
                        b = 0;
                        c = 0;
                        DetermineAim();
                        k = 1;
                    }
                    else
                    {
                        int ngu2 = rd.Next(0, 6);
                        if (target.X < player.X && (player.X - target.X) > 200 && (player.X - target.X) < 800)
                        {
                            if (ngu2 < 3)
                            {
                                boomcount = 1;
                                a = 0;
                                b = 0;
                                c = 0;
                                DetermineAim();
                                k = 1;
                            }
                            else
                            {
                                boomcount = 3;
                                a = 10001;
                                b = 10003;
                                c = 10004;
                                DetermineAim();
                                k = 3;
                            }
                        }
                        else if (Math.Abs(player.X - target.X) > 900 && player.TurnNum >= 2)
                        {
                            if (player.X > target.X)
                                d = (target.X + 300);
                            else
                                d = (target.X - 300);
                            boomcount = 1;
                            a = 0;
                            b = 10016;
                            c = 10010;
                            e = target.Y - 100 + rd.Next(-80, 81);
                            d += rd.Next(-160, 161);
                            k = 1;
                        }
                        else
                        {
                            boomcount = 1;
                            a = 10001;
                            b = 10004;
                            c = 10004;
                            DetermineAim();
                            k = 3;
                        }
                    }

                    if (a != 0)
                    {
                        ItemTemplateInfo itemTemplate = ItemMgr.FindItemTemplate(a);
                        //player.UseItem(itemTemplate);
                        player.CallFuction(delegate { player.UseItem(itemTemplate); }, delayy);
                    }
                    if (b != 0)
                    {
                        ItemTemplateInfo itemTemplate1 = ItemMgr.FindItemTemplate(b);
                        //player.UseItem(itemTemplate1);
                        player.CallFuction(delegate { player.UseItem(itemTemplate1); }, delayy + 500);

                    }
                    if (c != 0)
                    {
                        ItemTemplateInfo itemTemplate2 = ItemMgr.FindItemTemplate(c);
                        //player.UseItem(itemTemplate2);
                        player.CallFuction(delegate { player.UseItem(itemTemplate2); }, delayy + 1000);

                    }

                    if (Math.Abs(player.X - target.X) < 200)
                    {
                        time_s = 1.0f;
                    }
                    else if (Math.Abs(player.X - target.X) < 400)
                    {
                        time_s = 1.5f;
                    }
                    else if (Math.Abs(player.X - target.X) < 700)
                    {
                        time_s = 2.0f;
                    }
                    else if (Math.Abs(player.X - target.X) < 1000)
                    {
                        time_s = 2.5f;
                    }
                    else if (Math.Abs(player.X - target.X) < 1100)
                    {
                        time_s = 3.0f;
                    }
                    else
                    {
                        time_s = 3.5f;
                    }
                }
                    else
                    {
                        rd = new Random();
                        int nt2 = rd.Next(0, 4);
                        if (nt2 == 0 && player.TurnNum >= 2)
                        {
                            boomcount = 1;
                            b = 10010;
                            c = 10016;
                            e = player.Y + rd.Next(-10, 10) * 20;
                            k = 1;
                            time_s = 4.0f;
                            if (player.X > 700)
                                d = (player.X - 600);
                            else
                                d = (player.X + 600);
                            d += rd.Next(-120, 121);
                            ItemTemplateInfo itemTemplate1 = ItemMgr.FindItemTemplate(b);
                            ItemTemplateInfo itemTemplate2 = ItemMgr.FindItemTemplate(c);
                            player.CallFuction(delegate { player.UseItem(itemTemplate2); }, delayy);
                            player.CallFuction(delegate { player.UseItem(itemTemplate1); }, delayy + 500);

                        }
                        else
                        {
                            boomcount = 1;
                            a = 0;
                            b = 0;
                            c = 0;
                            DetermineAim();
                            k = 1;
                        }
                    }

                int delayDisX = game.GetDelayDistance(player.X, target.X, 6) + 1200;
                player.CallFuction(delegate
                {
                    for (int i = 0; i < k; i++)
                    {
                        player.ShootPoint(d, e, player.CurrentBall.ID, 1001, 10001, boomcount, time_s, 3000);
                    }
                }, delayy + 1500);

                player.CallFuction(delegate
                {
                    if (player.IsAttacking)
                        player.StopAttacking();
                }, delayy + 2000);

                GSPacketIn pkg = new GSPacketIn((byte)ePackageTypeLogic.GAME_CMD, player.Id);
                pkg.WriteByte((byte)eTankCmdType.BOT_COMMAND);
                game.SendToAll(pkg);
            }
        }
    }
}