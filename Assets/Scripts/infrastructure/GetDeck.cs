using System;
using System.Collections.Generic;
using System.Linq;

public class GetDeck
{
    private ICardProvider _provider;

    public GetDeck(ICardProvider provider) {
        _provider = provider;
    }
    public Deck Execute() {
        return new Deck()
        {
            EventCards = _provider.GetUpgradeCards(),
            UnitCards = _provider.GetUnitCards()
        };
    }
}