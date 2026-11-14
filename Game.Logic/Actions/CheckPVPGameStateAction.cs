using log4net;
using System.Reflection;

namespace Game.Logic.Actions
{
	public class CheckPVPGameStateAction : IAction
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private bool m_isFinished;

		private long m_tick;

		public CheckPVPGameStateAction(int delay)
		{
			m_tick += TickHelper.GetTickCount() + delay;
		}

		public void Execute(BaseGame game, long tick)
		{
			if (m_tick > tick)
			{
				return;
			}
			PVPGame pvp = game as PVPGame;
			if (pvp != null)
			{
				log.WarnFormat("[MatchTrace] CheckState gameId={0} state={1} allComplete={2} turn={3}",
					pvp.Id, game.GameState, pvp.IsAllComplete(), pvp.TurnIndex);
				switch (game.GameState)
				{
					case eGameState.Inited:
						log.WarnFormat("[MatchTrace] → Prepare gameId={0}", pvp.Id);
						pvp.Prepare();
						break;
					case eGameState.Prepared:
						log.WarnFormat("[MatchTrace] → StartLoading gameId={0}", pvp.Id);
						pvp.StartLoading();
						break;
					case eGameState.Loading:
						if (pvp.IsAllComplete())
						{
							log.WarnFormat("[MatchTrace] → StartGame (all loaded) gameId={0}", pvp.Id);
							pvp.StartGame();
						}
						else
						{
							foreach (var pl in pvp.GetAllFightPlayers())
							{
								log.WarnFormat("[MatchTrace]   waiting nick={0} load={1}%",
									pl?.PlayerDetail?.PlayerCharacter?.NickName, pl?.LoadingProcess);
							}
						}
						break;
					case eGameState.Playing:
						if (pvp.CurrentPlayer == null || !pvp.CurrentPlayer.IsAttacking)
						{
							if (pvp.TurnIndex >= 100 && pvp.RoomType == eRoomType.Match)
							{
								log.WarnFormat("[MatchTrace] → GameOver turnCap gameId={0} turn={1}", pvp.Id, pvp.TurnIndex);
								pvp.GameOver();
							}
							if (pvp.CanGameOver())
							{
								log.WarnFormat("[MatchTrace] → GameOver canGameOver gameId={0}", pvp.Id);
								pvp.GameOver();
							}
							else
							{
								log.WarnFormat("[MatchTrace] → NextTurn gameId={0} turn={1} current={2}",
									pvp.Id, pvp.TurnIndex, pvp.CurrentPlayer?.PlayerDetail?.PlayerCharacter?.NickName);
								pvp.NextTurn();
							}
						}
						break;
					case eGameState.GameOver:
						log.WarnFormat("[MatchTrace] → Stop gameId={0}", pvp.Id);
						pvp.Stop();
						break;
				}
			}
			m_isFinished = true;
		}

		public bool IsFinished(long tick)
		{
			return m_isFinished;
		}
	}
}
