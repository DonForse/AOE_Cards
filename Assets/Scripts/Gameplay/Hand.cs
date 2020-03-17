using System.Collections.Generic;
using System.Linq;

public class Hand
{
    public IList<UnitCardData> UnitCards;
    public IList<EventCardData> EventCards;

    public UnitCardData TakeUnitCard(string cardName)
    {
        var card = UnitCards.FirstOrDefault(c => c.cardName == cardName);
        UnitCards.Remove(card);
        return card;
    }

    public EventCardData TakeEventCard(string cardName)
    {
        var card = EventCards.FirstOrDefault(c => c.cardName == cardName);
        EventCards.Remove(card);
        return card;
    }
}