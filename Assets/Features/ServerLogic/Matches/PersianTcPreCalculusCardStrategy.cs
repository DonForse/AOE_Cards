using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Extensions;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Matches
{
    public class PersianTcPreCalculusCardStrategy : IPreCalculusCardStrategy
    {
        public bool IsValid(UpgradeCard card) => card.cardName.ToLowerInvariant() == "persian town center";

        public void Execute(UpgradeCard card, UnitCard unitCardPlayed, UnitCard rivalUnitCard, ServerMatch serverMatch, Round round, string userId)
        {
            if (!unitCardPlayed.Archetypes.ContainsAnyArchetype(Archetype.Villager))
                return;
            var upgrades = serverMatch.GetUpgradeCardsByPlayer(userId);
            var power = 0;
            foreach (var upgrade in upgrades) {
                if (upgrade.cardName == card.cardName)
                    continue;
                power += CalculateValue(upgrade,round.PlayerCards[userId].UnitCard, serverMatch.GetVsUnits(round, userId));
            }

            card.basePower = power + unitCardPlayed.BasePower;
        }
        
        private int CalculateValue(UpgradeCard card, UnitCard unitCard, IList<UnitCard> vsCards)
        {
            if (card.archetypes != null && !unitCard.Archetypes.Any(uArch => card.archetypes.Any(arch => arch == uArch)))
                return 0;

            if (card.bonusVs != null && !card.bonusVs.Any())
                return card.basePower;

            if (card.bonusVs != null && !card.bonusVs.Any(bonusVs => vsCards.Any(card => card.Archetypes.Any(arq => arq == bonusVs))))
                return 0;
            return card.basePower;
        }
    }
}