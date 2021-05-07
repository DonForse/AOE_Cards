using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Services;
using UniRx;
using UnityEngine;

namespace Game
{
    public class GamePresenter : IGamePresenter
    {
        private int currentRound;
        private readonly IGameView _view;
        private readonly IPlayService _playService;
        private readonly ITokenService _tokenService;
        private Hand _hand;

        public GamePresenter(IGameView view, IPlayService playService, ITokenService tokenService)
        {
            _view = view;
            _playService = playService;
            _tokenService = tokenService;
        }

        public void SetMatch(Match match)
        {
            _hand = match.Hand;
            currentRound = match.Board.Rounds.Count() - 1;
            PlayerPrefs.SetString(PlayerPrefsHelper.MatchId, match.Id);
            PlayerPrefs.Save();
        }

        public void StartNewRound()
        {
            currentRound++;
        }

        public void PlayUpgradeCard(string cardName)
        {
            _hand.TakeUpgradeCard(cardName);
            _playService.PlayUpgradeCard(cardName)
                .DoOnError(err => OnError((PlayServiceException)err))
                .Subscribe(OnUpgradeCardPostComplete);
        }

        public void PlayUnitCard(string cardName)
        {
            _hand.TakeUnitCard(cardName);
            _playService.PlayUnitCard(cardName)
                .DoOnError(err => OnError((PlayServiceException)err))
                .Subscribe(OnUnitCardPostComplete);
        }

        public void GetRound()
        {
            _playService.GetRound(currentRound)
                 .DoOnError(err => OnError((PlayServiceException)err))
                 .Subscribe(OnGetRoundComplete);
        }

        public void SendReroll(IList<string> upgradeCards, IList<string> unitCards)
        {
            _playService.RerollCards(unitCards, upgradeCards)
                 .DoOnError(err => OnError((PlayServiceException)err))
                 .Subscribe(OnRerollComplete);
        }

        private void OnGetRoundComplete(Round round)
        {
            _view.OnGetRoundInfo(round);
        }

        private void OnError(PlayServiceException error)
        {
            if (error.Code == 401)
            {
                _tokenService.RefreshToken()
                    .DoOnError(err => OnRefreshTokenError(err.Message))
                    .Subscribe(OnRefreshTokenComplete);
                return;
            }
            _view.ShowError(error.Message);
        }

        private void OnRefreshTokenError(string error)
        {
            GameManager.SessionExpired();
        }

        private void OnRefreshTokenComplete(UserResponseDto response)
        {
            PlayerPrefs.SetString(PlayerPrefsHelper.UserId, response.guid);
            PlayerPrefs.SetString(PlayerPrefsHelper.UserName, response.username);
            PlayerPrefs.SetString(PlayerPrefsHelper.FriendCode, response.friendCode);
            PlayerPrefs.SetString(PlayerPrefsHelper.AccessToken, response.accessToken);
            PlayerPrefs.SetString(PlayerPrefsHelper.RefreshToken, response.refreshToken);
            PlayerPrefs.Save();
        }

        internal Hand GetHand()
        {
            return _hand;
        }

        private void OnRerollComplete(Hand hand)
        {
            _hand = hand;
            _view.OnRerollComplete(hand);
        }

        private void OnUnitCardPostComplete(Hand hand)
        {
            _hand = hand;
            _view.UnitCardSentPlay();
        }

        private void OnUpgradeCardPostComplete(Hand hand)
        {
            _hand = hand;
            _view.UpgradeCardSentPlay();
        }

        internal void RemoveCard(string cardName, bool upgrade)
        {
            if (upgrade)
                _hand.TakeUpgradeCard(cardName);
            else
                _hand.TakeUnitCard(cardName);
        }

    }
}