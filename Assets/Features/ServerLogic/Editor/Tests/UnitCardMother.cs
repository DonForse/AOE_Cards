using System.Collections.Generic;
using ServerLogic.Cards.Domain.Units;
using ServerLogic.Cards.Domain.Upgrades;
using ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Editor.Tests
{
    public static class UnitCardMother
    {
        public static UnitCard Create(string withCardName = "unit-card",int withBasePower = 0, List<Archetype> withArchetypes = null, List<Archetype>withBonusVs =  null, int withPowerEffect = 0)
        {
            withArchetypes ??= new List<Archetype>() {Archetype.Monk};

            return new UnitCard()
            {
                Archetypes = withArchetypes,
                BasePower = withBasePower,
                BonusVs = withBonusVs,
                CardName = withCardName,
                PowerEffect = withPowerEffect
            };
        }
        
        // public static UnitCard Create(string withCardName = "unit-card",int withBasePower = 0, Archetype withArchetype = Archetype.Monk, Archetype? withBonusVs = null, int withPowerEffect = 0)
        // {
        //     return new UnitCard()
        //     {
        //         Archetypes = new List<Archetype>(){withArchetype},
        //         BasePower = withBasePower,
        //         BonusVs = withBonusVs.HasValue ? new List<Archetype>(){withBonusVs.Value} : null,
        //         CardName = withCardName,
        //         PowerEffect = withPowerEffect
        //     };
        // }
    }
}

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