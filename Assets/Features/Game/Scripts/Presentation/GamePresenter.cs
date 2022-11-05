using System.Collections.Generic;
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
        private readonly IGameView _view;
        private readonly IPlayService _playService;
        private readonly ITokenService _tokenService;
        
        private CompositeDisposable _disposables = new CompositeDisposable();
        private readonly IGetRoundEvery3Seconds _getRoundEvery3Seconds;
        private readonly ICurrentMatchRepository _matchRepository;
        public void Unload() => _disposables.Clear();


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
            
            _view.ReRoll()
                .Subscribe(rerollInfo => SendReRoll(rerollInfo.upgrades, rerollInfo.units))
                .AddTo(_disposables);

            _view.UnitCardPlayed().Subscribe(PlayUnitCard).AddTo(_disposables);
            _view.UpgradeCardPlayed().Subscribe(PlayUpgradeCard).AddTo(_disposables);
        }

        public void SetMatch(Match.Domain.Match match)
        {
            _matchRepository.Set(match);
            PlayerPrefs.SetString(PlayerPrefsHelper.MatchId, match.Id);
            PlayerPrefs.Save();
        }

        public void StartNewRound()
        {
            var match = _matchRepository.Get();
            match.Board.Rounds.Add(new Round());
            _matchRepository.Set(match);
        }

        private void PlayUpgradeCard(string cardName)
        {
            var hand = _matchRepository.Get().Hand;
            hand.TakeUpgradeCard(cardName);
            _playService.PlayUpgradeCard(cardName)
                .DoOnError(err => HandleError((PlayServiceException)err))
                .Subscribe(OnUpgradeCardPostComplete);
        }

        private void PlayUnitCard(string cardName)
        {
            var hand = _matchRepository.Get().Hand;
            hand.TakeUnitCard(cardName);
            _playService.PlayUnitCard(cardName)
                .DoOnError(err => HandleError((PlayServiceException)err))
                .Subscribe(OnUnitCardPostComplete);
        }

        private void SendReRoll(IList<string> upgradeCards, IList<string> unitCards)
        {
            _playService.ReRollCards(unitCards, upgradeCards)
                 .DoOnError(err => HandleError((PlayServiceException)err))
                 .Subscribe(OnRerollComplete);
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

        internal Hand GetHand() => _matchRepository.Get().Hand;

        private void OnRerollComplete(Hand hand)
        {
            _matchRepository.Set(hand);
            _view.OnRerollComplete(hand);
        }

        private void OnUnitCardPostComplete(Hand hand)
        {
            _matchRepository.Set(hand);
            _view.OnUnitCardPlayed();
        }

        private void OnUpgradeCardPostComplete(Hand hand)
        {
            _matchRepository.Set(hand);
            _view.OnUpgradeCardPlayed();
        }

        internal void RemoveCard(string cardName, bool upgrade)
        {
            if (upgrade)
                _matchRepository.Get().Hand.TakeUpgradeCard(cardName);
            else
                _matchRepository.Get().Hand.TakeUnitCard(cardName);
        }

    }
}