using Game.Logic.Phy.Object;
using log4net;
using System.Collections.Generic;
using System.Reflection;

namespace Game.Logic.Actions
{
	public class WaitPlayerLoadingAction : IAction
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private long m_time;

		private bool m_isFinished;

		public WaitPlayerLoadingAction(BaseGame game, int maxTime)
		{
			m_time = TickHelper.GetTickCount() + maxTime;
			game.GameStarted += game_GameStarted;
			log.WarnFormat("[MatchTrace] WaitPlayerLoading start gameId={0} timeoutMs={1}", game?.Id, maxTime);
		}

		private void game_GameStarted(AbstractGame game)
		{
			game.GameStarted -= game_GameStarted;
			m_isFinished = true;
			log.WarnFormat("[MatchTrace] WaitPlayerLoading cancelled — game started gameId={0}", game?.Id);
		}

		public void Execute(BaseGame game, long tick)
		{
			if (m_isFinished || tick <= m_time || game.GameState != eGameState.Loading)
			{
				return;
			}
			if (game.GameState == eGameState.Loading)
			{
				List<Player> player = game.GetAllFightPlayers();
				log.ErrorFormat("[MatchTrace] LOADING TIMEOUT gameId={0} — kick players still <100%", game.Id);
				foreach (Player p in player)
				{
					log.ErrorFormat("[MatchTrace]   timeout player={0} loading={1}% autoBot={2}",
						p?.PlayerDetail?.PlayerCharacter?.NickName,
						p?.LoadingProcess,
						p?.PlayerDetail?.PlayerCharacter?.IsAutoBot);
					if (p.LoadingProcess < 100)
					{
						game.SendPlayerRemove(p);
						game.RemovePlayer(p.PlayerDetail, IsKick: false);
					}
				}
				game.CheckState(0);
			}
			m_isFinished = true;
		}

		public bool IsFinished(long tick)
		{
			return m_isFinished;
		}
	}
}
