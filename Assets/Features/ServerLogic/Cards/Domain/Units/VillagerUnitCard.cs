using System;
using System.Linq;

namespace ServerLogic.Cards.Domain.Units
{
    public class VillagerUnitCard : UnitCard
    {
        public override void Play(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch, string userId)
        {
            var currentRound = serverMatch.Board.RoundsPlayed.Last();
            if (currentRound.PlayerCards.ContainsKey(userId) && currentRound.PlayerCards[userId].UnitCard != null)
                throw new ApplicationException("Unit card has already been played");

            currentRound.PlayerCards[userId].UnitCard = this;
        }
    }
}

