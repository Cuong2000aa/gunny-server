using Game.Base.Packets;
using Game.Logic;

namespace Game.Server.Packets.Client
{
    [PacketHandler((byte)ePackageType.GAME_CMD, "游戏数据")]
	public class GameDataHandler : IPacketHandler
    {
        public int HandlePacket(GameClient client, GSPacketIn packet)
        {
			if (client.Player.CurrentRoom != null)
			{
				if (UseBackupID(client.Player.CurrentRoom.RoomType))
				{
					packet.Parameter1 = client.Player.TempGameId;
				}
				else
				{
					packet.Parameter1 = client.Player.GamePlayerId;
				}
				TraceLoad(client, packet);
				if (client.Player.CurrentRoom.Game == null)
				{
					GSPacketIn gSPacketIn = packet.Clone();
					gSPacketIn.ClearOffset();
					byte pkg = gSPacketIn.ReadByte();
					if (pkg == 98)
					{
						int num = gSPacketIn.ReadByte();
						if (num > 9)
						{
							num = 8;
						}
						GSPacketIn gSPacketIn2 = new GSPacketIn(91, gSPacketIn.Parameter1);
						gSPacketIn2.Parameter1 = gSPacketIn.Parameter1;
						gSPacketIn2.WriteByte(98);
						gSPacketIn2.WriteBoolean(val: true);
						gSPacketIn2.WriteByte((byte)num);
						gSPacketIn2.WriteInt(0);
						gSPacketIn2.WriteInt(0);
						gSPacketIn2.WriteBoolean(val: false);
						client.Player.SendTCP(gSPacketIn2);
					}
					return 0;
				}
				client.Player.CurrentRoom.ProcessData(packet);
			}
			return 0;
        }

        private static void TraceLoad(GameClient client, GSPacketIn packet)
        {
			try
			{
				if (packet.Length <= 20 || packet.Buffer[20] != 16)
				{
					return;
				}
				var room = client.Player.CurrentRoom;
				log4net.LogManager.GetLogger(typeof(GameDataHandler)).WarnFormat(
					"[MatchTrace] Client→Road LOAD nick={0} p1={1} tempGameId={2} gamePlayerId={3} game={4} gameId={5}",
					client.Player.PlayerCharacter?.NickName,
					packet.Parameter1,
					client.Player.TempGameId,
					client.Player.GamePlayerId,
					room.Game?.GetType().Name ?? "null",
					room.Game?.Id);
			}
			catch
			{
			}
        }

        private bool UseBackupID(eRoomType roomType)
        {
			if (roomType == eRoomType.Match)
			{
				return true;
			}
			return false;
        }
    }
}
