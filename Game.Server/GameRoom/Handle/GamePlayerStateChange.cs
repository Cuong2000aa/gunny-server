using Bussiness;
using Game.Base.Packets;
using Game.Server.Rooms;
using log4net;
using System;
using System.Reflection;

namespace Game.Server.GameRoom.Handle
{
    [GameRoomHandleAttbute(15)]
	public class GamePlayerStateChange : IGameRoomCommandHadler
    {
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		public bool CommandHandler(GamePlayer Player, GSPacketIn packet)
        {
			if (Player.MainWeapon == null)
			{
				Player.SendMessage(LanguageMgr.GetTranslation("Game.Server.SceneGames.NoEquip"));
				return false;
			}
			if (Player.CurrentRoom != null)
			{
				byte state = packet.ReadByte();
				RoomMgr.UpdatePlayerState(Player, state);
				// state 2 = đang load trận; log WARN để lọc dễ hơn
				log.WarnFormat("[Match] RoomState nick={0} state={1} room={2}",
					Player.PlayerCharacter.NickName, state, Player.CurrentRoom?.RoomId);
			}
			return true;
        }
    }
}
