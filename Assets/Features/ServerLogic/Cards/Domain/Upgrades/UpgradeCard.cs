﻿using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Cards.Domain.Upgrades
{
    public class UpgradeCard
    {
        public readonly string cardName;
        public int basePower;
        public readonly IEnumerable<Archetype> bonusVs;
        public readonly IEnumerable<Archetype> archetypes;

        public UpgradeCard(string cardName, int basePower, IEnumerable<Archetype> bonusVs, IEnumerable<Archetype> archetypes)
        {
            this.cardName = cardName;
            this.basePower = basePower;
            this.bonusVs = bonusVs;
            this.archetypes = archetypes;
        }
    }
}

