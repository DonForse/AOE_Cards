using System.Linq;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Cards.Domain.Upgrades
{
    public class MadrasahApplicateEffectPostUnitStrategy : IApplicateEffectPostUnitStrategy
    {
        public bool IsValid(UpgradeCard card)
        {
            return card.cardName.ToLowerInvariant() == "madrasah";
        }

        public void Execute(ServerMatch serverMatch, string userId, UnitCard unitCardPlayed)
        {
            if (unitCardPlayed.Archetypes.Any(upArchetype => upArchetype == Archetype.Monk))
                serverMatch.Board.PlayersHands[userId].UnitsCards.Add(unitCardPlayed);
        }
    }
}