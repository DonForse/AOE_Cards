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

        public virtual int ApplyAlternativeEffects(Matches.Domain.Match match)
        {
            return 0;
        }

        public virtual void Play(Matches.Domain.Match match, string userId)
        {
            var currentRound = match.Board.RoundsPlayed.Last();
            var hand = match.Board.PlayersHands[userId];
            var upgradeCard = hand.UpgradeCards.FirstOrDefault(u => u.CardName == CardName);
            if (upgradeCard == null || !hand.UpgradeCards.Remove(upgradeCard))
                throw new ApplicationException("Upgrade card is not in hand");
            currentRound.PlayerCards[userId].UpgradeCard = this;
        }

        public virtual void ApplicateEffectPreUnit(Matches.Domain.Match match, string userId)
        {

        }

        public virtual void ApplicateEffectPostUnit(Matches.Domain.Match match, string userId)
        {

        }

        public virtual void ApplicateEffectPreCalculus(Matches.Domain.Match match, string userId)
        {

        }

        public virtual void ApplicateEffectPostCalculus(Matches.Domain.Match match, string userId)
        {
        }
    }
}

