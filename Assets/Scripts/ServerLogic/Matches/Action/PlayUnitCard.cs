using ServerLogic.Cards.Infrastructure;
using ServerLogic.Matches.Infrastructure;

namespace ServerLogic.Matches.Action
{
    public class PlayUnitCard
    {
        private readonly IMatchesRepository _matchesRepository;
        private readonly ICardRepository _cardRepository;

        public PlayUnitCard(IMatchesRepository matchesRepository, ICardRepository cardRepository)
        {
            _matchesRepository = matchesRepository;
            _cardRepository = cardRepository;
        }

        public void Execute(string matchId,string userId, string cardname)
        {
            var match = _matchesRepository.Get(matchId);
            //get type of card.
            var unitCard = _cardRepository.GetUnitCard(cardname);
            match.PlayUnitCard(userId, unitCard);
            _matchesRepository.Update(match);
        }
    }
}