using System.Collections.Generic;

namespace Infrastructure.Services
{
    public class Round
    {
        public int RoundNumber;
        public IList<PlayerCard> CardsPlayed;
        public IList<string> WinnerPlayers;
        public UpgradeCardData UpgradeCardRound;
        public bool Finished;
        public bool RivalReady;
    }
}