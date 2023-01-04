using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Extensions;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Cards.Domain.Upgrades
{
    public class TeutonsFaithUpgradeCardStrategy : IUpgradeCardStrategy
    {
        public bool IsValid(UpgradeCard card) => card.CardName.ToLowerInvariant() == "teutons faith";

        public int Execute(UpgradeCard card, UnitCard unitCardPlayed, UnitCard rivalUnitCard, ServerMatch serverMatch, Round round, string userId)
        {
            if (unitCardPlayed.Archetypes.All(a => a != Archetype.Cavalry))
                return 0;

            return RivalPlayedMonk() ? 1000 : // do not use because of overflow: int.MaxValue;
                0;

            bool RivalPlayedMonk() => rivalUnitCard.Archetypes.ContainsAnyArchetype(new List<Archetype> {Archetype.Monk});
        }
    }
}