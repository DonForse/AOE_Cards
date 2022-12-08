using System.Collections.Generic;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Editor.Tests.Mothers
{
    public static class BoardMother
    {
        public static Board Create(Deck withDeck = null,
            Round withCurrentRound = null,
            IDictionary<string, Hand> withPlayerHands = null,
            IList<Round> withRoundsPlayed = null)
        {
            return new Board()
            {
                Deck = withDeck,
                CurrentRound = withCurrentRound,
                PlayersHands = withPlayerHands,
                RoundsPlayed = withRoundsPlayed
            };
        }
    }
}