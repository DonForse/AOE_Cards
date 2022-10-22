using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;

namespace Infrastructure
{
    public class InMemoryCardProvider : MonoBehaviour, ICardProvider
    {
        [SerializeField] List<UnitCardData> units;
        [SerializeField] List<UpgradeCardData> upgrades;

        private IList<UnitCardData> _unitCards = null;
        private IList<UpgradeCardData> _upgradeCards = null;

        public IList<UpgradeCardData> GetUpgradeCards()
        {
            if (_upgradeCards == null)
                _upgradeCards = upgrades;
        
            return _upgradeCards.ToList();
        }

        public IList<UnitCardData> GetUnitCards()
        {
            if (_unitCards == null)
                _unitCards = units;

            return _unitCards.ToList();
        }

        public UnitCardData GetUnitCard(string unitcard)
        {
            if (string.IsNullOrWhiteSpace(unitcard))
                return null;
            var card = GetUnitCards().FirstOrDefault(f => f.cardName == unitcard);
            if (card == null)
                Debug.LogError(unitcard);
            return card;
        }
        public UpgradeCardData GetUpgradeCard(string upgradeCard)
        {
            if (string.IsNullOrWhiteSpace(upgradeCard))
                return null;
            var card = GetUpgradeCards().FirstOrDefault(f => f.cardName == upgradeCard);
            if (card == null)
                Debug.LogError(upgradeCard);
            return card;
        }
    }
}