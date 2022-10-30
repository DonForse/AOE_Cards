using System.Collections.Generic;
using System.Linq;
using ServerLogic.Cards.Domain.Units;
using ServerLogic.Cards.Infrastructure;

namespace ServerLogic.Cards.Actions
{
    public class GetUnitCard
    {
        private readonly ICardRepository _cardRepository;
        public GetUnitCard(ICardRepository cardRepository) {
            _cardRepository = cardRepository;
        }
        public IList<UnitCard> Execute(bool withVillager)
        {
            var result = _cardRepository.GetUnitCards();
            if (!withVillager)
                result = result.Where(uc => uc.CardName.ToLower() != "villager").ToList();
            return result;
        }
        public UnitCard Execute(string cardName)
        {
            return _cardRepository.GetUnitCard(cardName);
        }
    }
}