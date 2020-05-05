namespace Game
{
    public enum PlayerType
    {
        Player,Rival
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
        RoundUpgradeReveal,
        SelectUpgrade,
        WaitUpgrade,
        UpgradeReveal,
        SelectUnit,
        WaitUnit,
        RoundResultReveal,
        EndRound,
        EndGame,
    }
}