using System.Collections.Generic;
using Features.Data;

namespace Features.Infrastructure.Data
{
    public class Round
    {
        public int RoundNumber;
        public IList<PlayerCard> CardsPlayed;
        public IList<string> WinnerPlayers;
        public UpgradeCardData UpgradeCardRound;
        public bool Finished;
        public bool RivalReady;
        public RoundState RoundState;
        public bool HasReroll;
        public int Timer;
    }
}