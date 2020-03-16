using System;
using System.Collections.Generic;
using System.Linq;

public class Deck
{
    public IList<UnitCardData> UnitCards;
    public IList<EventCardData> EventCards;

    public IList<UnitCardData> TakeUnitCards(int amount)
    {
        var cards = UnitCards.Take(amount);
        cards.Select(c => UnitCards.Remove(c));
        return cards.ToList();
    }

    public void Shuffle()
    {
        UnitCards = Randomize(UnitCards);
        EventCards = Randomize(EventCards);
    }

    private IList<T> Randomize<T>(IList<T> list)
    {
        Random rnd = new Random();
        return list.OrderBy((item) => rnd.Next()).ToList();
    }
}