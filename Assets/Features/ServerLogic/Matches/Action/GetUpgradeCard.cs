using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Cards.Infrastructure;

namespace Features.ServerLogic.Matches.Action
{
    public class GetUpgradeCard : IGetUpgradeCard
    {
        private readonly ICardRepository _cardRepository;
        public GetUpgradeCard(ICardRepository cardRepository)
        {
            _cardRepository = cardRepository;
        }
        public IList<UpgradeCard> Execute()
        {
            return _cardRepository.GetUpgradeCards();
        }
        public UpgradeCard Execute(string cardName)
        {
            return _cardRepository.GetUpgradeCard(cardName);
        }
    }
}