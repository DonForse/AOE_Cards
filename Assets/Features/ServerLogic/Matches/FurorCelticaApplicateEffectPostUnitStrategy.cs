using System.Linq;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Matches
{
    public class FurorCelticaApplicateEffectPostUnitStrategy : IApplicateEffectPostUnitStrategy
    {
        public bool IsValid(UpgradeCard card)
        {
            return card.cardName.ToLowerInvariant() == "furor celtica";
        }

        public void Execute(ServerMatch serverMatch, string userId, UnitCard unitCardPlayed)
        {
            if (unitCardPlayed.archetypes.Any(upArchetype => upArchetype == Archetype.SiegeUnit))
                serverMatch.Board.PlayersHands[userId].UnitsCards.Add(unitCardPlayed);
        }
    }
}