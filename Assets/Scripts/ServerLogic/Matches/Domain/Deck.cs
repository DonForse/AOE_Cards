using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ServerLogic.Cards.Domain.Units;
using ServerLogic.Cards.Domain.Upgrades;

namespace ServerLogic.Matches.Domain
{
    public class Deck
    {
        private readonly Random _random;
        public ConcurrentStack<UnitCard> UnitCards;
        public ConcurrentStack<UpgradeCard> UpgradeCards;
        public Deck() {
            _random = new Random();
        }
        public IList<UnitCard> TakeUnitCards(int count)
        {
            UnitCard[] cards = new UnitCard[count];
            UnitCards.TryPopRange(cards, 0, count);
            return cards.ToList();
        }

        internal IList<UpgradeCard> TakeUpgradeCards(int count)
        {
            UpgradeCard[] cards = new UpgradeCard[count];
            UpgradeCards.TryPopRange(cards, 0, count);
            return cards.ToList();
        }

        internal void Shuffle()
        {
            ShuffleUnits();

            ShuffleUpgrades();
        }

        private void ShuffleUpgrades()
        {
            var upgrades = UpgradeCards.ToArray();
            UpgradeCards.Clear();
            foreach (var value in upgrades.OrderBy(x => _random.Next()))
                UpgradeCards.Push(value);
        }

        private void ShuffleUnits()
        {
            var units = UnitCards.ToArray();
            UnitCards.Clear();
            foreach (var value in units.OrderBy(x => _random.Next()))
                UnitCards.Push(value);
        }

        internal void AddUpgradeCards(List<UpgradeCard> upgradeCards)
        {
            if (upgradeCards == null || upgradeCards.Count == 0)
                return;
            UpgradeCards.PushRange(upgradeCards.ToArray());
            ShuffleUpgrades();
        }

        internal void AddUnitCards(List<UnitCard> unitCards)
        {
            if (unitCards == null || unitCards.Count == 0)
                return;
            UnitCards.PushRange(unitCards.ToArray());
            ShuffleUnits();
        }
    }
}

