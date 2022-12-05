using System.Collections.Generic;
using ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Matches.Domain
{
    public class Board
    {
        public IDictionary<string, Hand> PlayersHands;
        public Deck Deck;
        public IList<Round> RoundsPlayed;
        public Round CurrentRound; //TODO: separate logic of current round with previous rounds
        //
    }
}

