using System;

namespace Features.ServerLogic.Matches.Service
{
    public class ServerConfiguration : IServerConfiguration
    {
        public int GetAmountUnitCardsForPlayers()
        {
            return 7;
        }

        public int GetAmountUpgradeCardForPlayers()
        {
            return 7;
        }

        public int GetRoundTimerDurationInSeconds()
        {
            return 40;
        }

        public int GetMaxBotWaitForPlayRoundTimeInSeconds()
        {
            return 38;
        }

        public int GetAmountOfPlayersInMatch() => 2;

        public TimeSpan GetDurationInTimerForBotMatch()
        {
            return TimeSpan.FromSeconds(30);
        }
    }
}