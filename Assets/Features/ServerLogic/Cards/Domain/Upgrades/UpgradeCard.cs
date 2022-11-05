using System;
using System.Collections.Generic;
using System.Linq;
using ServerLogic.Cards.Domain.Units;
using ServerLogic.Matches.Domain;

namespace ServerLogic.Cards.Domain.Upgrades
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

        public virtual int ApplyAlternativeEffects(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch)
        {
            return 0;
        }

        public virtual void Play(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch, string userId)
        {
            var currentRound = serverMatch.Board.RoundsPlayed.Last();
            var hand = serverMatch.Board.PlayersHands[userId];
            var upgradeCard = hand.UpgradeCards.FirstOrDefault(u => u.CardName == CardName);
            if (upgradeCard == null || !hand.UpgradeCards.Remove(upgradeCard))
                throw new ApplicationException("Upgrade card is not in hand");
            currentRound.PlayerCards[userId].UpgradeCard = this;
        }

        public virtual void ApplicateEffectPreUnit(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch, string userId)
        {

        }

        public virtual void ApplicateEffectPostUnit(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch, string userId)
        {

        }

        public virtual void ApplicateEffectPreCalculus(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch, string userId)
        {

        }

        public virtual void ApplicateEffectPostCalculus(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch, string userId)
        {
        }
    }
}

