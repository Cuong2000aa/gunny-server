using Bussiness;
using Game.Logic.Phy.Maps;
using Game.Logic.Phy.Object;
using log4net;
using SqlDataProvider.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Game.Logic
{
    public class BallMgr
    {
        private static Dictionary<int, BallInfo> dictionary_0;

        private static Dictionary<int, Tile> dictionary_1;

        private static Dictionary<int, Tile> dictionary_2;

        private static readonly ILog ilog_0;

        static BallMgr()
        {
			ilog_0 = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        public static BallInfo FindBall(int id)
        {
			if (dictionary_0.ContainsKey(id))
			{
				return dictionary_0[id];
			}
			return null;
        }

        public static Tile FindTile(int id)
        {
			if (dictionary_1.ContainsKey(id))
			{
				return dictionary_1[id];
			}
			return null;
        }

        public static Tile FindBorderTile(int id)
        {
			if (dictionary_2 != null && dictionary_2.ContainsKey(id))
			{
				return dictionary_2[id];
			}
			return null;
        }

        public static BombType GetBallType(int ballId)
        {
			switch (ballId)
			{
			case 1:
			case 56:
			case 99:
				return BombType.FORZEN;
			case 3:
				return BombType.FLY;
			case 5:
			case 59:
			case 64:
			case 97:
			case 98:
			case 120:
			case 10009:
				return BombType.CURE;
			case 110:
			case 117:
				return BombType.WORLDCUP;
			case 128:
			case 129:
				return BombType.CATCHINSECT;
			default:
				return BombType.Normal;
			}
        }

        public static bool Init()
        {
			return ReLoad();
        }

        public static bool ReLoad()
        {
			try
			{
				Dictionary<int, BallInfo> dictionary = smethod_0();
				Dictionary<int, Tile> dictionary2 = smethod_1(dictionary);
				Dictionary<int, Tile> dictionary3 = LoadBorderTiles(dictionary);
				if (dictionary.Values.Count > 0 && dictionary2.Values.Count > 0)
				{
					Interlocked.Exchange(ref dictionary_0, dictionary);
					Interlocked.Exchange(ref dictionary_1, dictionary2);
					Interlocked.Exchange(ref dictionary_2, dictionary3);
					return true;
				}
			}
			catch (Exception exception)
			{
				ilog_0.Error("Ball Mgr init error:", exception);
			}
			return false;
        }

        private static Dictionary<int, BallInfo> smethod_0()
        {
			Dictionary<int, BallInfo> dictionary = new Dictionary<int, BallInfo>();
			using ProduceBussiness produceBussiness = new ProduceBussiness();
			BallInfo[] allBall = produceBussiness.GetAllBall();
			BallInfo[] array = allBall;
			foreach (BallInfo ballInfo in array)
			{
				if (!dictionary.ContainsKey(ballInfo.ID))
				{
					dictionary.Add(ballInfo.ID, ballInfo);
				}
			}
			return dictionary;
        }

        private static Dictionary<int, Tile> smethod_1(Dictionary<int, BallInfo> list)
        {
			Dictionary<int, Tile> dictionary = new Dictionary<int, Tile>();
			foreach (BallInfo current in list.Values)
			{
				if (current.HasTunnel)
				{
					// Prefer Crater ID mask so dig depth/size matches client crater visuals
					// (many weapons share one Crater; ball-ID .bomb can be a wrong/oversized clone).
					Tile tile = LoadBombTile(current);
					dictionary.Add(current.ID, tile);
					if (tile == null && current.ID != 1 && current.ID != 2 && current.ID != 3)
					{
						ilog_0.ErrorFormat("can't find bomb file for ball {0} (crater={1})", current.ID, current.Crater);
					}
				}
			}
			return dictionary;
        }

        private static Dictionary<int, Tile> LoadBorderTiles(Dictionary<int, BallInfo> list)
        {
			Dictionary<int, Tile> dictionary = new Dictionary<int, Tile>();
			foreach (BallInfo current in list.Values)
			{
				if (current.HasTunnel)
				{
					Tile tile = LoadBorderTile(current);
					if (tile != null)
					{
						dictionary.Add(current.ID, tile);
					}
				}
			}
			return dictionary;
        }

        private static Tile LoadBombTile(BallInfo ball)
        {
			var candidates = new List<string>();
			string baseDir = AppDomain.CurrentDomain.BaseDirectory;
			if (!string.IsNullOrWhiteSpace(ball.Crater) &&
			    int.TryParse(ball.Crater.Trim(), out int craterId) &&
			    craterId > 0)
			{
				candidates.Add(Path.Combine(baseDir, "bomb", $"{craterId}.bomb"));
				candidates.Add(Path.Combine("bomb", $"{craterId}.bomb"));
			}
			candidates.Add(Path.Combine(baseDir, "bomb", $"{ball.ID}.bomb"));
			candidates.Add(Path.Combine("bomb", $"{ball.ID}.bomb"));

			foreach (string path in candidates)
			{
				if (File.Exists(path))
				{
					return new Tile(path, digable: false);
				}
			}
			return null;
        }

        private static Tile LoadBorderTile(BallInfo ball)
        {
			var candidates = new List<string>();
			string baseDir = AppDomain.CurrentDomain.BaseDirectory;
			if (!string.IsNullOrWhiteSpace(ball.Crater) &&
			    int.TryParse(ball.Crater.Trim(), out int craterId) &&
			    craterId > 0)
			{
				candidates.Add(Path.Combine(baseDir, "bomb", $"{craterId}_brink.bomb"));
				candidates.Add(Path.Combine(baseDir, "bomb", $"{craterId}_border.bomb"));
				candidates.Add(Path.Combine("bomb", $"{craterId}_brink.bomb"));
				candidates.Add(Path.Combine("bomb", $"{craterId}_border.bomb"));
			}
			candidates.Add(Path.Combine(baseDir, "bomb", $"{ball.ID}_brink.bomb"));
			candidates.Add(Path.Combine(baseDir, "bomb", $"{ball.ID}_border.bomb"));
			candidates.Add(Path.Combine("bomb", $"{ball.ID}_brink.bomb"));
			candidates.Add(Path.Combine("bomb", $"{ball.ID}_border.bomb"));

			foreach (string path in candidates)
			{
				if (File.Exists(path))
				{
					return new Tile(path, digable: false);
				}
			}
			return null;
        }
    }
}
