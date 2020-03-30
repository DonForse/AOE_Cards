using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InMemoryCardProvider : ICardProvider
{
    private IList<UnitCardData> _unitCards = null;
    private IList<UpgradeCardData> _upgradeCards = null;

    public IList<UpgradeCardData> GetUpgradeCards()
    {
        if (_upgradeCards == null)
            _upgradeCards = Resources.LoadAll<UpgradeCardData>("Cards/UpgradeCards");
        
        return _upgradeCards.ToList();
    }

    public IList<UnitCardData> GetUnitCards()
    {
        if (_unitCards == null)
            _unitCards = Resources.LoadAll<UnitCardData>("Cards/UnitCards");

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