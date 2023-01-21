﻿using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Extensions;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Matches
{
    public class TeutonsFaithPreCalculusCardStrategy : IPreCalculusCardStrategy
    {
        public bool IsValid(UpgradeCard card) => card.CardName.ToLowerInvariant() == "teutons faith";

        public void Execute(UpgradeCard card, UnitCard unitCardPlayed, UnitCard rivalUnitCard, ServerMatch serverMatch, Round round, string userId)
        {
            if (unitCardPlayed.Archetypes.All(a => a != Archetype.Cavalry))
                return;

            if (!RivalPlayedMonk())
                return;
            
            card.BasePower= 1000; // do not use because of overflow: int.MaxValue;

            bool RivalPlayedMonk() => rivalUnitCard.Archetypes.ContainsAnyArchetype(new List<Archetype> {Archetype.Monk});
        }
    }
}