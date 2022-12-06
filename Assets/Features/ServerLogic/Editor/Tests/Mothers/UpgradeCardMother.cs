using System.Collections.Generic;
using ServerLogic.Cards.Domain.Upgrades;
using ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Editor.Tests.Mothers
{
    public static class UpgradeCardMother
    {
        public static UpgradeCard Create(string withCardName = "unit-card", int withBasePower = 0,
            List<Archetype> withArchetypes = null, List<Archetype> withBonusVs = null)
        {
            withArchetypes ??= new List<Archetype>() {Archetype.Monk};

            return new UpgradeCard()
            {
                Archetypes = withArchetypes,
                BasePower = withBasePower,
                BonusVs = withBonusVs,
                CardName = withCardName,
            };
        }
    }
}