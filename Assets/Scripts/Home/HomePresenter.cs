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
        private bool previousPlayVsFriend = false;
        private string previousFriendCode= "";
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
            _matchService.StartMatch(vsBot, vsFriend, friendCode, botDifficulty, OnMatchStatusComplete, OnError);
           _view.OnStartLookingForMatch(vsBot);
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
            _matchService.RemoveMatch(OnLeaveQueueComplete, OnError);
        }

        private void OnLeaveQueueComplete()
        {
            _view.OnQueueLeft();
        }
    }
}