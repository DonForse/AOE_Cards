using System.Linq;
using Infrastructure.Services;

namespace Game
{
    public class GamePresenter
    {
        private readonly GetDeck _getDeck;
        private Deck _deck;
        private Hand _hand;
        private readonly IGameView _view;
        private readonly IMatchService _matchService;

        public GamePresenter(IGameView view, IMatchService matchService ,GetDeck getDeck)
        {
            _view = view;
            _getDeck = getDeck;
            _matchService = matchService;
        }

        public Hand GetHand()
        {
            return _hand;
        }

        public void GameSetup()
        {
            _deck = _getDeck.Execute();
            _deck.Shuffle();
            _hand = new Hand(_deck.TakeUnitCards(5), _deck.TakeEventCards(5));

        }

        public void RoundSetup()
        {
            var card = _deck.TakeEventCards(1).FirstOrDefault();
        
            _view.ShowRoundEventCard(card);
        }

        public void PlayEventCard(string cardName)
        {
            var card = _hand.TakeEventCard(cardName);
            _matchService.PlayEventCard(card.cardName);
            _view.EventCardSentPlay();
        }

        public void PlayUnitCard(string cardName)
        {
            var card = _hand.TakeUnitCard(cardName);
            _matchService.PlayUnitCard(card.cardName);
            _view.UnitCardSentPlay();

        }
    }
}