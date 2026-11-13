using Game.Logic.AI;

namespace GameServerScript.AI.Game
{
    public class DarkCastleHardGame : APVEGameControl
    {
        public override void OnCreated()
        {
            Game.SetupMissions("4201,4202,4203");
            Game.TotalMissionCount = 3;
        }

        public override void OnPrepated()
        {
            Game.SessionId = 0;
        }

        public override int CalculateScoreGrade(int score)
        {
            if (score > 800) return 3;
            if (score > 725) return 2;
            if (score > 650) return 1;
            return 0;
        }
    }
}
