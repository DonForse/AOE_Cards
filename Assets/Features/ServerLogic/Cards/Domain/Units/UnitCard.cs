using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Cards.Domain.Units
{
    public class UnitCard
    {
        public string CardName;
        public int BasePower;
        public int PowerEffect;
        public IList<Archetype> BonusVs;
        public IList<Archetype> Archetypes;
    }
}

