using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace Game
{
    public class Hand
    {
        private readonly IList<UnitCardData> _unitCards;
        private readonly IList<UpgradeCardData> _upgradeCards;

        public Hand(IList<UnitCardData> unitCards, IList<UpgradeCardData> upgradeCards)
        {
            _unitCards = unitCards;
            _upgradeCards = upgradeCards;
        }

        public IList<UnitCardData> GetUnitCards()
        {
            return _unitCards.ToList();
        }
        public IList<UpgradeCardData> GetUpgradeCards()
        {
            return _upgradeCards.ToList();
        }

        public UnitCardData TakeUnitCard(string cardName)
        {
            var card = _unitCards.FirstOrDefault(c => c.cardName == cardName);
            _unitCards.Remove(card);
            return card;
        }

        public UpgradeCardData TakeUpgradeCard(string cardName)
        {
            var card = _upgradeCards.FirstOrDefault(c => c.cardName == cardName);
            _upgradeCards.Remove(card);
            return card;
        }
    }
}