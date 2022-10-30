using System.Collections.Generic;

namespace ServerLogic.Matches.Domain
{
    public class Board
    {
        public IDictionary<string, Hand> PlayersHands;
        public Deck Deck;
        public IList<Round> RoundsPlayed;
    }
}

