using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InMemoryCardProvider : ICardProvider
{
    public IList<UpgradeCardData> GetUpgradeCards()
    {
        throw new System.NotImplementedException();
    }

    public IList<UnitCardData> GetUnitCards()
    {
        throw new System.NotImplementedException();
    }
}
