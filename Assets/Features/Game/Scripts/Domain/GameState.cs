namespace Features.Game.Scripts.Domain
{
    public enum GameState
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
        public static bool IsWaiting(this GameState gameState)
        {
            switch (gameState)
            {
                case GameState.WaitReroll:
                case GameState.WaitUnit:
                case GameState.WaitUpgrade:
                case GameState.WaitRoundUpgradeReveal:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsUpgradePhase(this GameState gameState)
        {
            switch (gameState)
            {
                case GameState.StartUpgrade:
                case GameState.SelectUpgrade:
                case GameState.WaitUpgrade:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsUnitPhase(this GameState gameState)
        {
            switch (gameState)
            {
                case GameState.StartUnit:
                case GameState.SelectUnit:
                case GameState.WaitUnit:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsRerollPhase(this GameState gameState)
        {
            switch (gameState)
            {
                case GameState.StartReroll:
                case GameState.SelectReroll:
                case GameState.WaitReroll:
                    return true;
                default:
                    return false;
            }
        }
    }
}