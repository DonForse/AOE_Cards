using System.Linq;
using Features.ServerLogic.Cards.Domain.Entities;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Matches
{
    public class MadrasahApplicateEffectPostUnitStrategy : IApplicateEffectPostUnitStrategy
    {
        public bool IsValid(UpgradeCard card)
        {
            return card.cardName.ToLowerInvariant() == "madrasah";
        }

        public void Execute(ServerMatch serverMatch, string userId, UnitCard unitCardPlayed)
        {
            if (unitCardPlayed.archetypes.Any(upArchetype => upArchetype == Archetype.Monk))
                serverMatch.Board.PlayersHands[userId].UnitsCards.Add(unitCardPlayed);
        }
    }
}