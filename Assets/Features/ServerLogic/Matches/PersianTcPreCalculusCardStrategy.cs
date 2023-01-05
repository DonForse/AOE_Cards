using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Extensions;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Cards.Domain.Upgrades
{
    public class PersianTcPreCalculusCardStrategy : IPreCalculusCardStrategy
    {
        public bool IsValid(UpgradeCard card) => card.CardName.ToLowerInvariant() == "persian town center";

        public void Execute(UpgradeCard card, UnitCard unitCardPlayed, UnitCard rivalUnitCard, ServerMatch serverMatch, Round round, string userId)
        {
            if (!unitCardPlayed.Archetypes.ContainsAnyArchetype(Archetype.Villager))
                return;
            var upgrades = serverMatch.GetUpgradeCardsByPlayer(userId);
            var power = 0;
            foreach (var upgrade in upgrades) {
                if (upgrade.CardName == card.CardName)
                    continue;
                power += upgrade.CalculateValue(round.PlayerCards[userId].UnitCard, serverMatch.GetVsUnits(round, userId));
            }

            card.BasePower = power + unitCardPlayed.BasePower;
        }
    }
}