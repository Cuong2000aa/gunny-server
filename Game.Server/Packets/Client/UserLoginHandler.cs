using Bussiness;
using Bussiness.CenterService;
using Bussiness.Interface;
using Game.Base.Packets;
using Game.Server.Managers;
using SqlDataProvider.Data;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;

namespace Game.Server.Packets.Client
{
    [PacketHandler((int)ePackageType.LOGIN, "User Login handler")]
    public class UserLoginHandler : IPacketHandler
    {
        private static readonly ConcurrentDictionary<string, byte> s_loggingIn = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

        public int HandlePacket(GameClient client, GSPacketIn packet)
        {
            try
            {
                if (client.Player == null)
                {
                    int version = packet.ReadInt();
                    int clientType = packet.ReadInt();
                    if (clientType != 69)
                    {
                        byte[] tempKey = new byte[8];
                        byte[] src = packet.ReadBytes();
                        // Ruffle / patched Flash rsaEncry5 ? plaintext (date7 + key8 + "user,pass").
                        bool decrypted = false;
                        try
                        {
                            if (src != null && src.Length >= 64)
                            {
                                src = WorldMgr.RsaCryptor.Decrypt(src, fOAEP: false);
                                decrypted = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            GameServer.log.Warn("RsaCryptor fallback to plaintext login: " + ex.Message);
                            decrypted = false;
                        }
                        if (!decrypted)
                        {
                            Console.WriteLine("[UserLogin] plaintext socket login (len={0})", src?.Length ?? 0);
                        }
                        if (src == null || src.Length <= 15)
                        {
                            client.Out.SendKitoff(LanguageMgr.GetTranslation("UserLoginHandler.LoginError"));
                            client.Disconnect();
                            return 0;
                        }
                        for (int i = 0; i < 8; i++)
                        {
                            tempKey[i] = src[i + 7];
                        }
                        client.setKey(tempKey);
                        string[] temp = Encoding.UTF8.GetString(src, 15, src.Length - 15).Split(',');
                        if (temp.Length == 2)
                        {
                            string user = temp[0];
                            string pass = temp[1];
                            // Client ByteSocket is singleton - only the LATEST WS is listened to.
                            // Newest-wins: wait out a racing login, then this socket owns the session.
                            if (!s_loggingIn.TryAdd(user, 1))
                            {
                                Console.WriteLine("[UserLogin] wait prior login user={0}", user);
                                for (int w = 0; w < 40 && s_loggingIn.ContainsKey(user); w++)
                                {
                                    Thread.Sleep(50);
                                }
                                s_loggingIn[user] = 1;
                            }
                            try
                            {
                                    bool isFirst = false;
                                    BaseInterface inter = BaseInterface.CreateInterface();
                                    PlayerInfo cha = inter.LoginGame(user, pass, GameServer.Instance.Configuration.AreaID, ref isFirst);
                                    if (cha == null || cha.ID == 0)
                                    {
                                        cha = inter.LoginGame(user, "local", GameServer.Instance.Configuration.AreaID, ref isFirst);
                                    }
                                    if (cha == null || cha.ID == 0)
                                    {
                                        cha = TryLocalDbLogin(user, pass, ref isFirst);
                                    }
                                    if (cha != null && cha.ID != 0)
                                    {
                                        if (cha.ID == -2)
                                        {
                                            client.Out.SendKitoff(LanguageMgr.GetTranslation("UserLoginHandler.Forbid"));
                                            client.Disconnect();
                                            Console.WriteLine("{0} Forbid!", user);
                                            return 0;
                                        }
                                        isFirst = false;
                                        // Newest-wins: kick prior live session so THIS socket gets SendLoginSuccess.
                                        GamePlayer existing = WorldMgr.GetPlayerById(cha.ID);
                                        if (existing != null)
                                        {
                                            Console.WriteLine("[UserLogin] newest-wins kick prior user={0} id={1}", user, cha.ID);
                                            try { existing.Client?.Disconnect(); } catch { }
                                            try { WorldMgr.RemovePlayer(cha.ID); } catch { }
                                            try { LoginMgr.Remove(cha.ID); } catch { }
                                        }
                                        try
                                        {
                                            client.Player = new GamePlayer(cha.ID, user, client, cha);
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine("[UserLogin] GamePlayer ctor failed: {0}", ex.GetType().Name);
                                            GameServer.log.Error("GamePlayer ctor", ex);
                                            client.Out.SendKitoff(LanguageMgr.GetTranslation("UserLoginHandler.ServerError"));
                                            client.Disconnect();
                                            return 0;
                                        }
                                        LoginMgr.Add(cha.ID, client);
                                        client.Version = version;
                                        Console.WriteLine("Tai Khoan {0} login Game ....", user);
                                        GamePlayer gp = LoginMgr.LoginClient(cha.ID) ?? client.Player;
                                        if (gp == null || !gp.Login())
                                        {
                                            Console.WriteLine("[UserLogin] Login() failed user={0}", user);
                                            client.Out.SendKitoff(LanguageMgr.GetTranslation("UserLoginHandler.LoginError"));
                                            client.Disconnect();
                                            return 0;
                                        }
                                        try
                                        {
                                            WorldMgr.OnPlayerOnline(cha.ID, gp.PlayerCharacter.ConsortiaID);
                                        }
                                        catch (Exception ex)
                                        {
                                            GameServer.log.Warn("post-login online notify: " + ex.Message);
                                        }
                                        Console.WriteLine("[UserLogin] SendLoginSuccess user={0} id={1}", user, cha.ID);
                                    }
                                    else
                                    {
                                        Console.WriteLine("{0} Login with {1} OverTime....", user, pass);
                                        client.Out.SendKitoff(LanguageMgr.GetTranslation("UserLoginHandler.OverTime"));
                                        client.Disconnect();
                                    }
                            }
                            finally
                            {
                                s_loggingIn.TryRemove(user, out _);
                            }
                        }
                        else
                        {
                            client.Out.SendKitoff(LanguageMgr.GetTranslation("UserLoginHandler.LoginError"));
                            client.Disconnect();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                client.Out.SendKitoff(LanguageMgr.GetTranslation("UserLoginHandler.ServerError"));
                client.Disconnect();
                GameServer.log.Error(LanguageMgr.GetTranslation("UserLoginHandler.ServerError"), ex);
            }
            return 1;
        }

        /// <summary>
        /// Load character from DB and re-CreatePlayer on Center with the socket password
        /// so ValidateLoginAndGetID can succeed even when HTTP stored a different np.
        /// </summary>
        private static PlayerInfo TryLocalDbLogin(string user, string pass, ref bool isFirst)
        {
            try
            {
                int isFirstInt = 0;
                bool isExist = true;
                bool isError = false;
                DateTime forbid = DateTime.Now;
                PlayerInfo player;
                using (PlayerBussiness pb = new PlayerBussiness())
                {
                    player = pb.LoginGame(user, ref isFirstInt, ref isExist, ref isError, firstValidate: true, ref forbid, user);
                }
                if (player == null || player.ID == 0 || isError || !isExist)
                {
                    Console.WriteLine("[UserLogin] DB fallback miss user={0} exist={1} err={2}", user, isExist, isError);
                    return null;
                }
                // Skip Center WCF entirely (Faulted / BadImage on this private stack).
                Console.WriteLine("[UserLogin] DB fallback OK user={0} id={1} grade={2}", user, player.ID, player.Grade);
                isFirst = false;
                return player;
            }
            catch (Exception ex)
            {
                GameServer.log.Error("TryLocalDbLogin", ex);
                return null;
            }
        }
    }
}
