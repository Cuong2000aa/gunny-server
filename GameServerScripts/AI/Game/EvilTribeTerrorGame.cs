using Game.Logic.AI;

namespace GameServerScript.AI.Game
{
    public class EvilTribeTerrorGame : APVEGameControl
    {
        public override void OnCreated()
        {
            Game.SetupMissions("3301,3302,3303,3306");
            Game.TotalMissionCount = 4;
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
