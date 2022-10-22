using Infrastructure.Services;
using UnityEngine;
using UniRx;
using System;
using Infrastructure.DTOs;
using Infrastructure.Services.Exceptions;
using Match.Domain;
using Token;

namespace Home
{
    public class HomePresenter
    {
        private readonly IMatchService _matchService;
        private readonly ITokenService _tokenService;
        private bool previousPlayVsBot = false;
        private bool previousPlayVsFriend = false;
        private string previousFriendCode = "";
        private CompositeDisposable _disposables = new CompositeDisposable();

        private ISubject<Match.Domain.Match> _onMatchFound = new Subject<Match.Domain.Match>();
        public IObservable<Match.Domain.Match> OnMatchFound => _onMatchFound;

        private ISubject<string> _onError = new Subject<string>();
        public IObservable<string> OnError => _onError;

        public HomePresenter(IMatchService matchService, ITokenService tokenService)
        {
            _matchService = matchService;
            _tokenService = tokenService;
        }

        public void Unload()
        {
            _disposables.Clear();
        }

        public void StartSearchingMatch(bool vsBot, bool vsFriend, string friendCode, int botDifficulty = 0)
        {
            previousPlayVsBot = vsBot;
            previousPlayVsFriend = vsFriend;
            previousFriendCode = friendCode;
            PlayerPrefs.SetString(PlayerPrefsHelper.MatchId, string.Empty);
            PlayerPrefs.Save();
            StartMatch(vsBot, vsFriend, friendCode, botDifficulty);
        }

        private void StartMatch(bool vsBot, bool vsFriend, string friendCode, int botDifficulty)
        {
            _matchService.StartMatch(vsBot, vsFriend, friendCode, botDifficulty)
                .DoOnError(error => HandleError((MatchServiceException)error))
                .Subscribe(startMatch =>
                {
                    if (startMatch != null)
                    {
                        _onMatchFound.OnNext(startMatch);
                        return;
                    }

                    Observable.Interval(TimeSpan.FromSeconds(3))
                    .Subscribe(_ =>
                    {
                        _matchService.GetMatch()
                            .DoOnError(err => HandleError((MatchServiceException)err))
                            .Subscribe(match =>
                            {
                                if (match == null) return;
                                
                                _onMatchFound.OnNext(match);

                            }).AddTo(_disposables);
                    })
                    .AddTo(_disposables);
                })
                .AddTo(_disposables);
        }

        private void HandleError(MatchServiceException exception)
        {
            if (exception.Code != 401)
            {
                _onError.OnNext(exception.Error);
                return;
            }

            _tokenService.RefreshToken()
                .DoOnError(err => OnRefreshTokenError(err.Message))
                .Subscribe(OnRefreshTokenComplete).AddTo(_disposables);
            return;


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
            StartSearchingMatch(previousPlayVsBot, previousPlayVsFriend, previousFriendCode);
        }

        internal void LeaveQueue()
        {
            _matchService.RemoveMatch()
                .DoOnError(err => HandleError((MatchServiceException)err))
                .Subscribe()
                .AddTo(_disposables);
        }
    }
}