using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Entities;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Editor.Tests.Mothers
{
    public static class UnitCardMother
    {
        public static UnitCard Create(string withCardName = "unit-card",int withBasePower = 0, List<Archetype> withArchetypes = null, List<Archetype>withBonusVs =  null, int withPowerEffect = 0)
        {
            withArchetypes ??= new List<Archetype>() {Archetype.Monk};
            withBonusVs ??= new List<Archetype>();
            return new UnitCard(withCardName, withBasePower, withPowerEffect, withBonusVs,withArchetypes);
        }
    }
}