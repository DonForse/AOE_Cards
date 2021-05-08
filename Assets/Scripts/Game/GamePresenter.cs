﻿using System;
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
        //private readonly IGameView _view;
        private readonly IPlayService _playService;
        private readonly ITokenService _tokenService;
        private Hand _hand;

        private ISubject<Round> _onGetRoundInfo = new Subject<Round>();
        public IObservable<Round> OnGetRoundInfo => _onGetRoundInfo;
        private ISubject<string> _onError = new Subject<string>();
        public IObservable<string> OnError=> _onError;

        private ISubject<Hand> _onReroll = new Subject<Hand>();
        public IObservable<Hand> OnReroll=> _onReroll;

        private ISubject<Unit> _onUnitCardPlayed = new Subject<Unit>();
        public IObservable<Unit> OnUnitCardPlayed => _onUnitCardPlayed;
        private ISubject<Unit> _onUpgradeCardPlayed = new Subject<Unit>();
        private CompositeDisposable _disposables = new CompositeDisposable();

        public IObservable<Unit> OnUpgradeCardPlayed => _onUpgradeCardPlayed;

        public GamePresenter( IPlayService playService, ITokenService tokenService)
        {
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
                .DoOnError(err => HandleError((PlayServiceException)err))
                .Subscribe(OnUpgradeCardPostComplete);
        }

        public void PlayUnitCard(string cardName)
        {
            _hand.TakeUnitCard(cardName);
            _playService.PlayUnitCard(cardName)
                .DoOnError(err => HandleError((PlayServiceException)err))
                .Subscribe(OnUnitCardPostComplete);
        }

        public void GetRound()
        {
            _playService.GetRound(currentRound)
                 .DoOnError(err => HandleError((PlayServiceException)err))
                 .Subscribe(OnGetRoundComplete);
        }

        public void SendReroll(IList<string> upgradeCards, IList<string> unitCards)
        {
            _playService.RerollCards(unitCards, upgradeCards)
                 .DoOnError(err => HandleError((PlayServiceException)err))
                 .Subscribe(OnRerollComplete);
        }

        internal void Unload()
        {
            _disposables.Dispose();
        }

        private void OnGetRoundComplete(Round round)
        {
            _onGetRoundInfo.OnNext(round);
        }

        private void HandleError(PlayServiceException error)
        {
            if (error.Code == 401)
            {
                _tokenService.RefreshToken()
                    .DoOnError(err => OnRefreshTokenError(err.Message))
                    .Subscribe(OnRefreshTokenComplete);
                return;
            }
            _onError.OnNext(error.Message);
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
            _onReroll.OnNext(hand);
        }

        private void OnUnitCardPostComplete(Hand hand)
        {
            _hand = hand;
            _onUnitCardPlayed.OnNext(Unit.Default);
        }

        private void OnUpgradeCardPostComplete(Hand hand)
        {
            _hand = hand;
            _onUpgradeCardPlayed.OnNext(Unit.Default);
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