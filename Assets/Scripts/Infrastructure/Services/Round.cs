using System.Collections.Generic;

namespace Infrastructure.Services
{
    public class Round
    {
        public IList<PlayerCard> CardsPlayed;
        public string WinnerPlayer;
        public UpgradeCardData UpgradeCardRound;
    }
}