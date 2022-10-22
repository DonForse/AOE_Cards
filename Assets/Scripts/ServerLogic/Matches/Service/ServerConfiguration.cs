using System;

namespace ServerLogic.Matches.Service
{
    public static class ServerConfiguration
    {
        internal static int GetAmountUnitCardsForPlayers()
        {
            return 7;
        }

        internal static int GetAmountUpgradeCardForPlayers()
        {
            return 7;
        }

        internal static int GetRoundTimerDurationInSeconds()
        {
            return 40;
        }

        internal static int GetMaxBotWaitForPlayRoundTimeInSeconds()
        {
            return 38;
        }

        internal static TimeSpan GetDurationInTimerForBotMatch()
        {
            return TimeSpan.FromSeconds(30);
        }
    }
}