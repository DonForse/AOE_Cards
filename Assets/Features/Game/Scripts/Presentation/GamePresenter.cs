using System;
using System.Collections.Generic;
using System.Linq;
using Features.Game.Scripts.Domain;
using Infrastructure.Data;
using Infrastructure.DTOs;
using Infrastructure.Services;
using Infrastructure.Services.Exceptions;
using Token;
using UniRx;
using UnityEngine;

namespace Game
{
    public class GamePresenter
    {
        // private int currentRound;
        //private readonly IGameView _view;
        private readonly IGameView _view;
        private readonly IPlayService _playService;
        private readonly ITokenService _tokenService;
        private Hand _hand;

        private ISubject<string> _onError = new Subject<string>();
        public IObservable<string> OnError=> _onError;

        // private ISubject<Hand> _onReroll = new Subject<Hand>();
        // public IObservable<Hand> OnReroll=> _onReroll;

        private ISubject<Unit> _onUnitCardPlayed = new Subject<Unit>();
        public IObservable<Unit> OnUnitCardPlayed => _onUnitCardPlayed;
        private ISubject<Unit> _onUpgradeCardPlayed = new Subject<Unit>();
        private CompositeDisposable _disposables = new CompositeDisposable();
        private readonly IGetRoundEvery3Seconds _getRoundEvery3Seconds;
        private readonly ICurrentMatchRepository _matchRepository;

        public IObservable<Unit> OnUpgradeCardPlayed => _onUpgradeCardPlayed;

        public GamePresenter(IGameView view, IPlayService playService, ITokenService tokenService, IGetRoundEvery3Seconds getRoundEvery3Seconds, ICurrentMatchRepository currentMatchRepository
        )
        {
            _view = view;
            _playService = playService;
            _tokenService = tokenService;
            _getRoundEvery3Seconds = getRoundEvery3Seconds;
            _matchRepository = currentMatchRepository;
        }

        public void Initialize()
        {
            _getRoundEvery3Seconds.Execute()
                .Subscribe(OnGetRoundComplete)
                .AddTo(_disposables);
            
            _view.Reroll()
                .Subscribe(rerollInfo => SendReroll(rerollInfo.upgrades, rerollInfo.units))
                .AddTo(_disposables);
        }

        public void SetMatch(Match.Domain.Match match)
        {
            _hand = match.Hand;
            _matchRepository.Set(match);
            // currentRound = match.Board.Rounds.Count() - 1;
            PlayerPrefs.SetString(PlayerPrefsHelper.MatchId, match.Id);
            PlayerPrefs.Save();
        }

        public void StartNewRound()
        {
            var match = _matchRepository.Get();
            match.Board.Rounds.Add(new Round());
            _matchRepository.Set(match);
            // currentRound++;
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
        private void SendReroll(IList<string> upgradeCards, IList<string> unitCards)
        {
            _playService.RerollCards(unitCards, upgradeCards)
                 .DoOnError(err => HandleError((PlayServiceException)err))
                 .Subscribe(OnRerollComplete);
        }

        internal void Unload()
        {
            _disposables.Clear();
        }

        private void OnGetRoundComplete(Round round) => _view.OnGetRoundInfo(round);

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
            _view.OnRerollComplete(hand);
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