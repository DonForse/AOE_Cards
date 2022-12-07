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

        public virtual int CalculatePower(ServerMatch serverMatch, string userId)
        {
            var round = serverMatch.Board.RoundsPlayed.Last();
            var upgradeCards = serverMatch.GetUpgradeCardsByPlayer(round, userId);
            var vsCards = serverMatch.GetVsUnits(round, userId);

            var upgradePowers = 0;
            foreach (var upgradeCard in upgradeCards)
            {
                upgradeCard.ApplicateEffectPreCalculus(serverMatch, userId);
            }
            foreach (var upgradeCard in upgradeCards)
            {
                upgradePowers += upgradeCard.CalculateValue(this, vsCards);
            }            
            if (vsCards.Any(vs => vs.Archetypes.Any(vsArchetype => BonusVs.Any(bonus => bonus == vsArchetype))))
                upgradePowers += PowerEffect;

            foreach (var upgradeCard in upgradeCards)
            {
                upgradeCard.ApplicateEffectPostCalculus(serverMatch, userId);
            }

            return BasePower + upgradePowers;
        }
    }
}

