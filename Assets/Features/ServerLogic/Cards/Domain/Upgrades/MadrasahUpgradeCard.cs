using System.Linq;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Cards.Domain.Upgrades
{
    public class MadrasahUpgradeCard : UpgradeCard
    {
        public override void ApplicateEffectPostUnit(ServerMatch serverMatch, string userId)
        {
            var currentRound = serverMatch.Board.CurrentRound;
            var unitPlayed = currentRound.PlayerCards[userId].UnitCard;
            if (unitPlayed.Archetypes.Any(upArchetype => upArchetype == Archetype.Monk))
                serverMatch.Board.PlayersHands[userId].UnitsCards.Add(unitPlayed);
        }
    }
}

