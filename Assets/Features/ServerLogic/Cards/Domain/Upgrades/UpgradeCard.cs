using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Cards.Domain.Upgrades
{
    public class UpgradeCard
    {
        public string CardName;
        public int BasePower;
        public IList<Archetype> BonusVs;
        public IList<Archetype> Archetypes;

        public virtual int CalculateValue(UnitCard unitCard, IList<UnitCard> vsCards)
        {
            if (Archetypes != null && !unitCard.Archetypes.Any(uArch => Archetypes.Any(arch => arch == uArch)))
                return 0;

            if (BonusVs != null && BonusVs.Count == 0)
                return BasePower;

            if (BonusVs != null && !BonusVs.Any(bonusVs => vsCards.Any(card => card.Archetypes.Any(arq => arq == bonusVs))))
                return 0;
            return BasePower;
        }

        public virtual void ApplicateEffectPreUnit(ServerMatch serverMatch, string userId)
        {

        }

        public virtual void ApplicateEffectPostUnit(ServerMatch serverMatch, string userId)
        {

        }

        public virtual void ApplicateEffectPreCalculus(ServerMatch serverMatch, string userId)
        {

        }

        public virtual void ApplicateEffectPostCalculus(ServerMatch serverMatch, string userId)
        {
        }
    }
}

