using System.Collections.Generic;
using System.Linq;
using Game;

namespace Infrastructure.Services
{
    public class MatchStatus
    {
        public int round;
        public Hand hand;
        public Board board;
    }

    public class Board
    {
        public List<Round> Rounds;
    }

    public class Round
    {
        public IList<PlayerCard> CardsPlayed;
        public string WinnerPlayer;
        public UpgradeCardData UpgradeCardRound;
    }

    public class PlayerCard
    {
        public string Player;
        public UpgradeCardData UpgradeCardData;
        public UnitCardData UnitCardData;
    }
}