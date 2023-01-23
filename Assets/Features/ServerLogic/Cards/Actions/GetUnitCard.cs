using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Domain.Entities;
using Features.ServerLogic.Cards.Infrastructure;

namespace Features.ServerLogic.Cards.Actions
{
    public class GetUnitCard : IGetUnitCard
    {
        private readonly ICardRepository _cardRepository;
        public GetUnitCard(ICardRepository cardRepository) {
            _cardRepository = cardRepository;
        }
        public IList<UnitCard> Execute(bool withVillager)
        {
            var result = _cardRepository.GetUnitCards();
            if (!withVillager)
                result = result.Where(uc => uc.cardName.ToLower() != "villager").ToList();
            return result;
        }
        public UnitCard Execute(string cardName)
        {
            return _cardRepository.GetUnitCard(cardName);
        }
    }
}