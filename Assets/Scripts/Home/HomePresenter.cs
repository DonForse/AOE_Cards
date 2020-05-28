using System;
using System.Threading.Tasks;
using Infrastructure.Services;
using UnityEngine;

namespace Home
{
    public class HomePresenter
    {
        private readonly IMatchService _matchService;
        private readonly ITokenService _tokenService;
        private readonly IHomeView _view;
        private bool previousPlayVsBot = false;
        public HomePresenter(IHomeView view, IMatchService matchService, ITokenService tokenService)
        {
            _view = view;
            _matchService = matchService;
            _tokenService = tokenService;
        }

        public void StartSearchingMatch(bool vsBot)
        {
            previousPlayVsBot = vsBot;
            PlayerPrefs.SetString(PlayerPrefsHelper.MatchId, string.Empty);
            _matchService.StartMatch(vsBot, OnMatchStatusComplete, OnError);
           _view.OnStartLookingForMatch();
        }

        private void OnError(long responseCode, string message)
        {
            if (responseCode == 401)
            {
                _tokenService.RefreshToken(onRefreshTokenComplete, onRefreshTokenError);
                return;
            }
            _view.OnError(message);
        }

        private void onRefreshTokenError(string error)
        {
            GameManager.SessionExpired();
        }

        private void onRefreshTokenComplete(UserResponseDto response)
        {
            PlayerPrefs.SetString(PlayerPrefsHelper.UserId, response.guid);
            PlayerPrefs.SetString(PlayerPrefsHelper.UserName, response.username);
            PlayerPrefs.SetString(PlayerPrefsHelper.AccessToken, response.accessToken);
            PlayerPrefs.SetString(PlayerPrefsHelper.RefreshToken, response.refreshToken);

            StartSearchingMatch(previousPlayVsBot);
        }

        private void OnMatchStatusComplete(Match matchStatus)
        {
            _view.OnMatchFound(matchStatus);
        }
    }
}