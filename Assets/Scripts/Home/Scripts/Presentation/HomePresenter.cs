using UnityEngine;
using UniRx;
using System;
using Common;
using Infrastructure.DTOs;
using Infrastructure.Services.Exceptions;
using Match.Domain;
using Token;

namespace Home
{
    public class HomePresenter
    {
        private readonly IHomeView _view;
        private readonly IMatchService _matchService;
        private readonly ITokenService _tokenService;
        private readonly IPlayerPrefs _playerPrefs;
        private CompositeDisposable _viewDisposables = new CompositeDisposable();
        private CompositeDisposable _serviceDisposables = new CompositeDisposable();
        private IFindMatchInQueue _findMatchInQueue;

        public HomePresenter(IHomeView view, IMatchService matchService, ITokenService tokenService,
            IPlayerPrefs playerPrefs, IFindMatchInQueue findMatchInQueue)
        {
            _view = view;
            _matchService = matchService;
            _tokenService = tokenService;
            _playerPrefs = playerPrefs;
            _findMatchInQueue = findMatchInQueue;
        }

        public void Initialize()
        {
            _view.OnPlayMatch().Subscribe(_ =>
                StartSearchingMatch(false, false, string.Empty)).AddTo(_viewDisposables);
            _view.OnPlayVersusHardBot().Subscribe(_ =>
                StartSearchingMatch(true, false, string.Empty, 1)).AddTo(_viewDisposables);
            _view.OnPlayVersusEasyBot().Subscribe(_ =>
                StartSearchingMatch(true, false, string.Empty, 0)).AddTo(_viewDisposables);
            _view.OnLeaveQueue().Subscribe(_ => LeaveQueue()).AddTo(_viewDisposables);
            _view.OnPlayVersusFriend().Subscribe(friendCode => StartSearchingMatch(false, true, friendCode))
                .AddTo(_viewDisposables);
            // _view.OnStartSearchingMatch();
        }


        public void Unload()
        {
            _viewDisposables.Clear();
        }

        private void StartSearchingMatch(bool vsBot, bool vsFriend, string friendCode, int botDifficulty = 0)
        {
            _playerPrefs.SetString(PlayerPrefsHelper.MatchId, string.Empty);
            _playerPrefs.Save();
            StartMatch(vsBot, vsFriend, friendCode, botDifficulty);
        }

        private void StartMatch(bool vsBot, bool vsFriend, string friendCode, int botDifficulty)
        {
            _view.StartSearchingForMatch();
            _matchService.StartMatch(vsBot, vsFriend, friendCode, botDifficulty)
                // .DoOnError(error => HandleError((MatchServiceException)error))
                .Subscribe(startMatch =>
                {
                    if (startMatch != null)
                    {
                        _view.ShowMatchFound(startMatch);
                        return;
                    }

                    _findMatchInQueue.Execute()
                        .Subscribe(match => _view.ShowMatchFound(match))
                        .AddTo(_serviceDisposables);
                })
                .AddTo(_serviceDisposables);
        }


        // private void OnRefreshTokenError(string error)
        // {
        //     GameManager.SessionExpired();
        // }

        // private void OnRefreshTokenComplete(UserResponseDto response)
        // {
        //     PlayerPrefs.SetString(PlayerPrefsHelper.UserId, response.guid);
        //     PlayerPrefs.SetString(PlayerPrefsHelper.UserName, response.username);
        //     PlayerPrefs.SetString(PlayerPrefsHelper.FriendCode, response.friendCode);
        //     PlayerPrefs.SetString(PlayerPrefsHelper.AccessToken, response.accessToken);
        //     PlayerPrefs.SetString(PlayerPrefsHelper.RefreshToken, response.refreshToken);
        //     PlayerPrefs.Save();
        //     StartSearchingMatch(previousPlayVsBot, previousPlayVsFriend, previousFriendCode);
        // }

        private void LeaveQueue()
        {
            _matchService.RemoveMatch()
                // .DoOnError(err => HandleError((MatchServiceException)err))
                .DoOnCompleted(()=>_serviceDisposables.Clear())
                .Subscribe(_ => _view.LeftQueue())
                
                .AddTo(_serviceDisposables);
        }
    }
}