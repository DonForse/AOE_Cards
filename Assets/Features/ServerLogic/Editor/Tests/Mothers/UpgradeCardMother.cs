using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Entities;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Editor.Tests.Mothers
{
    public static class UpgradeCardMother
    {
        public static UpgradeCard Create(string withCardName = "round-upgrade-card", int withBasePower = 0,
            List<Archetype> withArchetypes = null, List<Archetype> withBonusVs = null)
        {
            withArchetypes ??= new List<Archetype>() {Archetype.Monk};

            return new UpgradeCard(withCardName, withBasePower, withBonusVs, withArchetypes);
        }

        public static UpgradeCard CreateFakeTeutonsFaithCard() =>
            new("teutons faith", 0, null, null);

        public static UpgradeCard CreateFakePersianTC() =>
            new("persian town center", 0, null, new[] { Archetype.Villager });

        public static UpgradeCard CreateFakeMadrasah() => 
            new("madrasah", 0, null, new[] { Archetype.Monk });

        public static UpgradeCard CreateFakeFurorCeltica() =>
            new("furor celtica", 0, null, new[] { Archetype.SiegeUnit });
    }
}