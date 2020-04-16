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
            return _match.Hand;
        }

        public void GameSetup(Match match)
        {
            _match = match;
            currentRound = _match.Board.Rounds.Count() -1;
            PlayerPrefs.SetString(PlayerPrefsHelper.MatchId, match.Id);

            _view.InitializeHand(_match.Hand);
            _view.InitializeRound(_match.Board.Rounds.Last());
        }

        public void StartNewRound()
        {
            currentRound++;
            GetRound();
        }

        public void PlayUpgradeCard(string cardName)
        {
            var card = _match.Hand.TakeUpgradeCard(cardName);
            _playService.PlayUpgradeCard(card.cardName, OnUpgradeCardPostComplete, OnError);
        }

        public void PlayUnitCard(string cardName)
        {
            var card = _match.Hand.TakeUnitCard(cardName);
            _playService.PlayUnitCard(card.cardName, OnUnitCardPostComplete, OnError);
        }

        public void GetRound()
        {
            _playService.GetRound(currentRound, OnGetRoundComplete, OnError);
        }

        private void OnGetRoundComplete(Round round)
        {
            if (round.WinnerPlayers.Count > 0)
            {
                if (!_match.Board.Rounds.Any(r => r.RoundNumber == round.RoundNumber))
                    _match.Board.Rounds.Add(round);
            }
            _view.OnGetRoundInfo(round);
        }

        private void OnError(string message)
        {
            _view.ShowError(message);
        }

        private void OnUnitCardPostComplete()
        {
            _view.UnitCardSentPlay();
        }

        private void OnUpgradeCardPostComplete()
        {
            _view.UpgradeCardSentPlay();
        }

        internal bool IsMatchOver()
        {
            return _match.Board.Rounds.SelectMany(r => r.WinnerPlayers).GroupBy(wp=>wp).Any(group=>group.Count() >= 4);
        }
    }
}