using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InMemoryDeckProvider : ICardProvider
{
    public IList<EventCardData> GetEventCards()
    {
        throw new System.NotImplementedException();
    }

    public IList<UnitCardData> GetUnitCards()
    {
        throw new System.NotImplementedException();
    }
}
