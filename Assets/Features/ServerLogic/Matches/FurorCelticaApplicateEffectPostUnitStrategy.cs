using System.Linq;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Cards.Domain.Upgrades
{
    public class FurorCelticaApplicateEffectPostUnitStrategy : IApplicateEffectPostUnitStrategy
    {
        public bool IsValid(UpgradeCard card)
        {
            return card.CardName.ToLowerInvariant() == "furor celtica";
        }

        public void Execute(ServerMatch serverMatch, string userId, UnitCard unitCardPlayed)
        {
            if (unitCardPlayed.Archetypes.Any(upArchetype => upArchetype == Archetype.SiegeUnit))
                serverMatch.Board.PlayersHands[userId].UnitsCards.Add(unitCardPlayed);
        }
    }
}