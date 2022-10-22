using System.Collections.Generic;
using ServerLogic.Cards.Domain.Upgrades;
using ServerLogic.Cards.Infrastructure;

namespace ServerLogic.Matches.Action
{
    internal class GetUpgradeCard
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