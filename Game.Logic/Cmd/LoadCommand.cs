using Game.Base.Packets;
using Game.Logic.Phy.Object;
using log4net;
using System.Reflection;

namespace Game.Logic.Cmd
{
	[GameCommand((byte)eTankCmdType.LOAD, "游戏加载进度")]
	public class LoadCommand : ICommandHandler
    {
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void HandleCommand(BaseGame game, Player player, GSPacketIn packet)
        {
            int progress = packet.ReadInt();
            // Client đôi khi gửi 0–1000; chuẩn hoá về 0–100 để IsAllComplete không treo
            if (progress > 100)
            {
                progress = progress >= 1000 ? 100 : progress / 10;
            }
            player.LoadingProcess = progress;
            if (progress >= 100 || progress == 0 || progress % 10 == 0)
            {
                log.WarnFormat("[MatchTrace] LOAD←Client gameId={0} nick={1} progress={2}% state={3} autoBot={4} allComplete={5}",
                    game?.Id,
                    player?.PlayerDetail?.PlayerCharacter?.NickName,
                    player.LoadingProcess,
                    game?.GameState,
                    player?.PlayerDetail?.PlayerCharacter?.IsAutoBot,
                    game?.IsAllComplete());
            }
            GSPacketIn pkg = new GSPacketIn((short)ePackageTypeLogic.GAME_CMD);
            pkg.WriteByte((byte)eTankCmdType.LOAD);
            if (game.GameState == eGameState.Loading)
            {
                if (player.LoadingProcess >= 100)
                {
                    game.CheckState(0);
                }
                pkg.WriteInt(player.LoadingProcess);
                pkg.WriteInt(player.PlayerDetail.ZoneId);//zoneID
                pkg.WriteInt(player.PlayerDetail.PlayerCharacter.ID);
                game.SendToAll(pkg);
            }
        }
    }
}
