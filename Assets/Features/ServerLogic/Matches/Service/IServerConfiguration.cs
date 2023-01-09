using System;

namespace Features.ServerLogic.Matches.Service
{
    public interface IServerConfiguration
    {
        int GetAmountUnitCardsForPlayers();
        int GetAmountUpgradeCardForPlayers();
        int GetRoundTimerDurationInSeconds();
        int GetMaxBotWaitForPlayRoundTimeInSeconds();
        TimeSpan GetDurationInTimerForBotMatch();
        int GetAmountOfPlayersInMatch();
    }
}