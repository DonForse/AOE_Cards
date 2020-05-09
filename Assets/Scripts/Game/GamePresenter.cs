using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Services;
using UnityEngine;

namespace Game
{
    public class GamePresenter : IGamePresenter
    {
        private int currentRound;
        private readonly IGameView _view;
        private readonly IPlayService _playService;
        private readonly ITokenService _tokenService;

        public GamePresenter(IGameView view, IPlayService playService, ITokenService tokenService)
        {
            _view = view;
            _playService = playService;
            _tokenService = tokenService;
        }

        public void GameSetup(Match match)
        {
            currentRound = match.Board.Rounds.Count() -1;
            PlayerPrefs.SetString(PlayerPrefsHelper.MatchId, match.Id);
            _view.InitializeGame(match);
        }

        public void StartNewRound()
        {
            currentRound++;
            GetRound();
        }

        public void PlayUpgradeCard(string cardName)
        {
            _playService.PlayUpgradeCard(cardName, OnUpgradeCardPostComplete, OnError);
        }

        public void PlayUnitCard(string cardName)
        {
            _playService.PlayUnitCard(cardName, OnUnitCardPostComplete, OnError);
        }

        public void GetRound()
        {
            _playService.GetRound(currentRound, OnGetRoundComplete, OnError);
        }

        public void SendReroll(IList<string> upgradeCards, IList<string> unitCards) {
            _playService.RerollCards(unitCards, upgradeCards, OnRerollComplete, OnError);
        }

        private void OnRerollComplete(Hand hand)
        {
            _view.OnRerollComplete(hand);
        }

        private void OnGetRoundComplete(Round round)
        {
            _view.OnGetRoundInfo(round);
        }

        private void OnError(long responseCode, string message)
        {
            if (responseCode == 401)
            {
                _tokenService.RefreshToken(OnRefreshTokenComplete, OnRefreshTokenError);
                return;
            }
            _view.ShowError(message);
        }

        private void OnRefreshTokenError(string error)
        {
            GameManager.SessionExpired();
        }

        private void OnRefreshTokenComplete(UserResponseDto response)
        {
            PlayerPrefs.SetString(PlayerPrefsHelper.UserId, response.guid);
            PlayerPrefs.SetString(PlayerPrefsHelper.UserName, response.username);
            PlayerPrefs.SetString(PlayerPrefsHelper.AccessToken, response.accessToken);
            PlayerPrefs.SetString(PlayerPrefsHelper.RefreshToken, response.refreshToken);
        }

        private void OnUnitCardPostComplete(Hand hand)
        {
            _view.UnitCardSentPlay(hand);
        }

        private void OnUpgradeCardPostComplete(Hand hand)
        {
            _view.UpgradeCardSentPlay();
        }
    }
}