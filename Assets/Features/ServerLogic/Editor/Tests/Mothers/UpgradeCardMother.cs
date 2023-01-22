using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Matches.Domain;

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

        public static UpgradeCard CreateFakeTeutonsFaithCard() => new UpgradeCard()
        {
            Archetypes =null,
            BasePower = 0,
            BonusVs = null,
            CardName = "teutons faith",
        };
        
        public static UpgradeCard CreateFakePersianTC()
        {
            return new UpgradeCard()
            {
                Archetypes =new[] {Archetype.Villager},
                BasePower = 0,
                BonusVs = null,
                CardName = "persian town center",
            };
        }
        
        public static UpgradeCard CreateFakeMadrasah()
        {
            return new UpgradeCard()
            {
                Archetypes =new[] {Archetype.Monk},
                BasePower = 0,
                BonusVs = null,
                CardName = "madrasah",
            };
        }
        
        public static UpgradeCard CreateFakeFurorCeltica()
        {
            return new UpgradeCard()
            {
                Archetypes =new[] {Archetype.SiegeUnit},
                BasePower = 0,
                BonusVs = null,
                CardName = "furor celtica",
            };
        }
    }
}