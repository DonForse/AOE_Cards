using System;
using System.Collections.Generic;
using System.Linq;
using ServerLogic.Matches.Domain;

namespace ServerLogic.Cards.Domain.Units
{
    public class UnitCard : IUnitCard
    {
        public string CardName;
        public int BasePower;
        public int PowerEffect;
        public IList<Archetype> BonusVs;
        public IList<Archetype> Archetypes;

        public virtual int CalculatePower(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch, string userId)
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

        public virtual void Play(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch, string userId)
        {
            var currentRound = serverMatch.Board.RoundsPlayed.Last();
            if (currentRound.PlayerCards.ContainsKey(userId) && currentRound.PlayerCards[userId].UnitCard != null)
                throw new ApplicationException("Unit card has already been played");

            var hand = serverMatch.Board.PlayersHands[userId];

            var unitCard = hand.UnitsCards.FirstOrDefault(u => u.CardName == CardName);
            if (unitCard == null || !hand.UnitsCards.Remove(unitCard))
                throw new ApplicationException("Unit card is not in hand");

            currentRound.PlayerCards[userId].UnitCard = this;
        }
    }
}

