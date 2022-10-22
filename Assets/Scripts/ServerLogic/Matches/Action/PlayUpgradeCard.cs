using ServerLogic.Cards.Infrastructure;
using ServerLogic.Matches.Infrastructure;

namespace ServerLogic.Matches.Action
{
    public class PlayUpgradeCard
    {
        private readonly IMatchesRepository _matchesRepository;
        private readonly ICardRepository _cardRepository;
        public PlayUpgradeCard(IMatchesRepository matchesRepository, ICardRepository cardRepository)
        {
            _matchesRepository = matchesRepository;
            _cardRepository = cardRepository;
        }
        public void Execute(string matchId,string userId, string cardname)
        {
            var match = _matchesRepository.Get(matchId);
            //get type of card.
            var upgradeCard = _cardRepository.GetUpgradeCard(cardname);
            match.PlayUpgradeCard(userId, upgradeCard);
            _matchesRepository.Update(match);
        }
    }
}