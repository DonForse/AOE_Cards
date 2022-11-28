namespace Features.Game.Scripts.Domain
{
    public enum MatchState
    {
        InitializeGame,
        StartRound,
        StartRoundUpgradeReveal,
        WaitRoundUpgradeReveal,
        StartReroll,
        SelectReroll,
        WaitReroll,
        StartUpgrade,
        SelectUpgrade,
        WaitUpgrade,
        UpgradeReveal,
        StartUnit,
        SelectUnit,
        WaitUnit,
        RoundResultReveal,
        EndRound,
        EndGame,
    }

    public static class MatchStateExtensions
    {
        public static bool IsWaiting(this MatchState matchState)
        {
            switch (matchState)
            {
                case MatchState.WaitReroll:
                case MatchState.WaitUnit:
                case MatchState.WaitUpgrade:
                case MatchState.WaitRoundUpgradeReveal:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsUpgradePhase(this MatchState matchState)
        {
            switch (matchState)
            {
                case MatchState.StartUpgrade:
                case MatchState.SelectUpgrade:
                case MatchState.WaitUpgrade:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsUnitPhase(this MatchState matchState)
        {
            switch (matchState)
            {
                case MatchState.StartUnit:
                case MatchState.SelectUnit:
                case MatchState.WaitUnit:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsRerollPhase(this MatchState matchState)
        {
            switch (matchState)
            {
                case MatchState.StartReroll:
                case MatchState.SelectReroll:
                case MatchState.WaitReroll:
                    return true;
                default:
                    return false;
            }
        }
    }
}