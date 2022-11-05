using System.Linq;
using ServerLogic.Matches.Domain;

namespace ServerLogic.Cards.Domain.Upgrades
{
    public class MadrasahUpgradeCard : UpgradeCard
    {
        public override void ApplicateEffectPostUnit(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch, string userId)
        {
            var currentRound = serverMatch.Board.RoundsPlayed.Last();
            var unitPlayed = currentRound.PlayerCards[userId].UnitCard;
            if (unitPlayed.Archetypes.Any(upArchetype => upArchetype == Archetype.Monk))
                serverMatch.Board.PlayersHands[userId].UnitsCards.Add(unitPlayed);
        }
    }
}

