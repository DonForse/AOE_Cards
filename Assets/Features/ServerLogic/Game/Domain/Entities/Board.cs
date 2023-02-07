using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Entities;

namespace Features.ServerLogic.Game.Domain.Entities
{
    public class Board
    {
        public IDictionary<string, Hand> PlayersHands;
        public Deck Deck;
        public IList<Round> RoundsPlayed;
        public Round CurrentRound ; //TODO: separate logic of current round with previous rounds
        //
    }
}

