using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Services;
using UnityEngine;

namespace Game
{
    public class GamePresenter
    {
        private Match _match;
        private int currentRound;
        private readonly IGameView _view;
        private readonly IPlayService _playService;

        public GamePresenter(IGameView view, IPlayService playService)
        {
            _view = view;
            _playService = playService;
        }

        public Hand GetHand()
        {
            return _match.hand;
        }

        public void GameSetup(Match match)
        {
            _match = match;
            currentRound = _match.board.Rounds.Count() -1;
            PlayerPrefs.SetString(PlayerPrefsHelper.MatchId, match.id);

            _view.InitializeHand(_match.hand);
            _view.InitializeRound(_match.board.Rounds.Last());
        }

        public void StartNewRound()
        {
            currentRound++;
            GetRound(currentRound);
        }

        public void PlayUpgradeCard(string cardName)
        {
            var card = _match.hand.TakeUpgradeCard(cardName);
            _playService.PlayUpgradeCard(card.cardName, OnUpgradeCardPostComplete, OnError);
        }

        public void PlayUnitCard(string cardName)
        {
            var card = _match.hand.TakeUnitCard(cardName);
            _playService.PlayUnitCard(card.cardName, OnUnitCardPostComplete, OnError);
        }

        public void GetRound(int roundNumber)
        {
            _playService.GetRound(roundNumber, OnGetRoundComplete, OnError);
        }

        private void OnGetRoundComplete(Round round)
        {
            _view.OnGetRoundInfo(round);
        }

        private void OnError(string obj)
        {
            throw new NotImplementedException();
        }

        private void OnUnitCardPostComplete()
        {
            _view.UnitCardSentPlay();
        }

        private void OnUpgradeCardPostComplete()
        {
            _view.UpgradeCardSentPlay();
        }
    }
}