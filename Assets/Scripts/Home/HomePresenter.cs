using Infrastructure.Services;
using UnityEngine;
using UniRx;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Home
{
    public class HomePresenter
    {
        private readonly IMatchService _matchService;
        private readonly ITokenService _tokenService;
        private readonly IHomeView _view;
        private bool previousPlayVsBot = false;
        private bool previousPlayVsFriend = false;
        private string previousFriendCode= "";
        private CompositeDisposable _disposables = new CompositeDisposable();
        private Match _match = null;

        public HomePresenter(IHomeView view, IMatchService matchService, ITokenService tokenService)
        {
            _view = view;
            _matchService = matchService;
            _tokenService = tokenService;
        }

        public void StartSearchingMatch(bool vsBot, bool vsFriend, string friendCode, int botDifficulty = 0)
        {
            _match = null;
            previousPlayVsBot = vsBot;
            previousPlayVsFriend = vsFriend;
            previousFriendCode = friendCode;
            PlayerPrefs.SetString(PlayerPrefsHelper.MatchId, string.Empty);
            PlayerPrefs.Save();
            StartMatch(vsBot, vsFriend, friendCode, botDifficulty);
            _view.OnStartLookingForMatch(vsBot);
        }

        private void StartMatch(bool vsBot, bool vsFriend, string friendCode, int botDifficulty)
        {
            _matchService.StartMatch(vsBot, vsFriend, friendCode, botDifficulty)
                .DoOnError(error => OnError((MatchServiceException)error))
                .Do(match => _match = match)
                .Delay(TimeSpan.FromSeconds(3))
                .Subscribe(match =>
                {
                    if (_match != null)
                        OnMatchStatusComplete(match);
                    else _matchService.GetMatch(OnMatchStatusComplete, (code, error) => OnError(new MatchServiceException(error,code)));
                })
                .AddTo(_disposables);
        }

        private void OnError(MatchServiceException exception)
        {
            if (exception.Code == 401)
            {
                _tokenService.RefreshToken(onRefreshTokenComplete, onRefreshTokenError);
                return;
            }
            _view.OnError(exception.Error);
        }

        private void onRefreshTokenError(string error)
        {
            GameManager.SessionExpired();
        }

        private void onRefreshTokenComplete(UserResponseDto response)
        {
            PlayerPrefs.SetString(PlayerPrefsHelper.UserId, response.guid);
            PlayerPrefs.SetString(PlayerPrefsHelper.UserName, response.username);
            PlayerPrefs.SetString(PlayerPrefsHelper.FriendCode, response.friendCode);
            PlayerPrefs.SetString(PlayerPrefsHelper.AccessToken, response.accessToken);
            PlayerPrefs.SetString(PlayerPrefsHelper.RefreshToken, response.refreshToken);
            PlayerPrefs.Save();
            StartSearchingMatch(previousPlayVsBot, previousPlayVsFriend, previousFriendCode);
        }

        private void OnMatchStatusComplete(Match matchStatus)
        {
            _view.OnMatchFound(matchStatus);
        }

        internal void LeaveQueue()
        {
            _matchService.RemoveMatch()
                .DoOnError(err => OnError((MatchServiceException)err))
                .Subscribe(_=>OnLeaveQueueComplete());
        }

        private void OnLeaveQueueComplete()
        {
            _view.OnQueueLeft();
        }
    }
}