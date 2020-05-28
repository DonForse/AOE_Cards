namespace Game
{
    public enum PlayerType
    {
        Player,Rival
    }

    public enum CardType {
        Unit,Upgrade
    }

    public enum Archetype {
        Villager,
        Eagle,
        Camel,
        Elephant,
        Cavalry,
        Archer,
        CavalryArcher,
        Militia,
        Infantry,
        CounterUnit,
        Siege,
        Monk
    }

    public enum MatchResult {
        Win,Lose,Tie,NotFinished
    }
    
    public enum MatchState
    {
        InitializeGame,
        StartRound,
        StartRoundUpgradeReveal,
        RoundUpgradeReveal,
        StartReroll,
        Reroll,
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

    public static class MatchStateExtensions {
        public static bool IsUpgradePhase(this MatchState matchState) {
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
                case MatchState.Reroll:
                case MatchState.WaitReroll:
                    return true;
                default:
                    return false;
            }
        }
    }
}