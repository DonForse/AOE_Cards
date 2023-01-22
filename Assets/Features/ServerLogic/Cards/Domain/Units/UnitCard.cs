using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Cards.Domain.Units
{
    public class UnitCard
    {
        public readonly string cardName;
        public readonly int basePower;
        public readonly int powerEffect;
        public readonly IEnumerable<Archetype> bonusVs;
        public readonly IEnumerable<Archetype> archetypes;

        public UnitCard(string cardName, int basePower, int powerEffect, IEnumerable<Archetype> bonusVs, IEnumerable<Archetype> archetypes)
        {
            this.cardName = cardName;
            this.basePower = basePower;
            this.powerEffect = powerEffect;
            this.bonusVs = bonusVs;
            this.archetypes = archetypes;
        }
    }
}

