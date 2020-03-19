using System;
using System.Collections.Generic;
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
        private readonly IList<RoundResult> _match;

        public GamePresenter(IGameView view, IMatchService matchService)
        {
            _view = view;
            _matchService = matchService;
            _match = new List<RoundResult>();
        }

        public Hand GetHand()
        {
            return _hand;
        }

        public void GameSetup(MatchStatus matchStatus)
        {
            _hand = matchStatus.hand;
            _view.InitializeHand(_hand);

            _view.ShowPlayerHand(_hand);
        }

        public void RoundSetup(UpgradeCardData upgradeCardData)
        {
            _view.ShowRoundUpgradeCard(upgradeCardData);
        }

        public void PlayUpgradeCard(string cardName)
        {
            var card = _hand.TakeUpgradeCard(cardName);
            _matchService.PlayUpgradeCard(card.cardName, OnUpgradeCardsPlayed);
            _view.UpgradeCardSentPlay();
        }

        public void PlayUnitCard(string cardName)
        {
            var card = _hand.TakeUnitCard(cardName);
            _matchService.PlayUnitCard(card.cardName, OnRoundFinished);
            _view.UnitCardSentPlay();
        }

        private void OnUpgradeCardsPlayed(Round round)
        {
            //_view.Up;
        }
        
        private void OnRoundFinished(RoundResult roundResult)
        {
            _view.CardReveal(roundResult);
        }
    }
}