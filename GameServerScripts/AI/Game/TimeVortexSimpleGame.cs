using Game.Logic.AI;

namespace GameServerScript.AI.Game
{
    public class TimeVortexSimpleGame : APVEGameControl
    {
        public override void OnCreated()
        {
            Game.SetupMissions("12001,12002,12003,12004");
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
