using System.Collections.Generic;
using System.Linq;

namespace Game
{
    public class Hand
    {
        private readonly IList<UnitCardData> _unitCards;
        private readonly IList<EventCardData> _eventCards;

        public Hand(IList<UnitCardData> unitCards, IList<EventCardData> eventCards)
        {
            _unitCards = unitCards;
            _eventCards = eventCards;
        }

        public IList<UnitCardData> GetUnitCards()
        {
            return _unitCards.ToList();
        }
        public IList<EventCardData> GetEventCards()
        {
            return _eventCards.ToList();
        }

        public UnitCardData TakeUnitCard(string cardName)
        {
            var card = _unitCards.FirstOrDefault(c => c.cardName == cardName);
            _unitCards.Remove(card);
            return card;
        }

        public EventCardData TakeEventCard(string cardName)
        {
            var card = _eventCards.FirstOrDefault(c => c.cardName == cardName);
            _eventCards.Remove(card);
            return card;
        }
    }
}