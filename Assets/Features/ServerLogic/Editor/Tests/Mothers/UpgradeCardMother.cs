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

        public static TeutonsFaithUpgradeCard CreateFakeTeutonsFaithCard() => new TeutonsFaithUpgradeCard()
        {
            Archetypes =null,
            BasePower = 0,
            BonusVs = null,
            CardName = "teutons faith",
        };
        
        public static PersianTCUpgradeCard CreateFakePersianTC()
        {
            return new PersianTCUpgradeCard()
            {
                Archetypes =new[] {Archetype.Villager},
                BasePower = 0,
                BonusVs = null,
                CardName = "persian town center",
            };
        }

        public static UpgradeCardStub CreateStub(string withCardName = "unit-card", int withBasePower = 0,
            List<Archetype> withArchetypes = null, List<Archetype> withBonusVs = null)
        {
            withArchetypes ??= new List<Archetype>() {Archetype.Monk};

            return new UpgradeCardStub()
            {
                Archetypes = withArchetypes,
                BasePower = withBasePower,
                BonusVs = withBonusVs,
                CardName = withCardName,
            };
        }
        
        public class UpgradeCardStub : UpgradeCard
        {
            public bool CalledApplicateEffectPreUnit = false;
            public bool CalledApplicateEffectPostUnit = false;

            public override void ApplicateEffectPreUnit(ServerMatch serverMatch, string userId)
            {
                CalledApplicateEffectPreUnit = true;
                base.ApplicateEffectPreUnit(serverMatch, userId);
            }
            
            public override void ApplicateEffectPostUnit(ServerMatch serverMatch, string userId)
            {
                CalledApplicateEffectPostUnit = true;
                base.ApplicateEffectPostUnit(serverMatch, userId);
            }
        }
    }
}