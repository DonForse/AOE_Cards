using System.Linq;
using Infrastructure.Services;

namespace Game
{
    public class GamePresenter
    {
        private Deck _deck;
        private Hand _hand;
        private readonly IGameView _view;
        private readonly IMatchService _matchService;

        public GamePresenter(IGameView view, IMatchService matchService)
        {
            _view = view;
            _matchService = matchService;
        }

        public Hand GetHand()
        {
            return _hand;
        }

        public void GameSetup(MatchStatus matchStatus)
        {
            _hand = matchStatus.hand; // new Hand(_deck.TakeUnitCards(5), _deck.TakeEventCards(5));
        }

        public void RoundSetup(UpgradeCardData upgradeCardData)
        {
            _view.ShowRoundUpgradeCard(upgradeCardData);
        }

        public void PlayUpgradeCard(string cardName)
        {
            var card = _hand.TakeUpgradeCard(cardName);
            _matchService.PlayUpgradeCard(card.cardName);
            _view.UpgradeCardSentPlay();
        }

        public void PlayUnitCard(string cardName)
        {
            var card = _hand.TakeUnitCard(cardName);
            _matchService.PlayUnitCard(card.cardName);
            _view.UnitCardSentPlay();
        }
    }
}