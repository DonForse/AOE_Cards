using Infrastructure.Services;
using UnityEngine;
using UniRx;
using System;

namespace Home
{
    public class HomePresenter
    {
        private readonly IMatchService _matchService;
        private readonly ITokenService _tokenService;
        private readonly IHomeView _view;
        private bool previousPlayVsBot = false;
        private bool previousPlayVsFriend = false;
        private string previousFriendCode = "";
        private CompositeDisposable _disposables = new CompositeDisposable();

        public HomePresenter(IHomeView view, IMatchService matchService, ITokenService tokenService)
        {
            _view = view;
            _matchService = matchService;
            _tokenService = tokenService;
        }

        public void StartSearchingMatch(bool vsBot, bool vsFriend, string friendCode, int botDifficulty = 0)
        {
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
                .Subscribe(startMatch =>
                {
                    if (startMatch != null)
                    {
                        OnMatchStatusComplete(startMatch);
                    }
                    else
                    {
                        Observable.Interval(TimeSpan.FromSeconds(3))
                        .Subscribe(_ =>
                        {
                            _matchService.GetMatch()
                                .DoOnError(err => OnError((MatchServiceException)err))
                                .Subscribe(match =>
                                {
                                    if (match != null)
                                    {
                                        _matchService.StopSearch();
                                        _disposables.Dispose();
                                        OnMatchStatusComplete(match);
                                    }
                                }).AddTo(_disposables);
                        });
                    }
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
                .Subscribe(_ => OnLeaveQueueComplete());
        }

        private void OnLeaveQueueComplete()
        {
            _view.OnQueueLeft();
        }
    }
}