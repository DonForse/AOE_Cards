using Infrastructure.Services;
using UnityEngine;

namespace Login
{
    internal class LoginPresenter
    {
        private readonly ILoginView _view;
        private readonly ILoginService _loginService;

        public LoginPresenter(ILoginView loginView, ILoginService loginService)
        {
            _view = loginView;
            _loginService = loginService;
        }

        public void Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                _view.ShowError("Username cannot be empty");
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                _view.ShowError("Password cannot be empty");
                return;
            }
            _loginService.Login(username, password, OnLoginComplete,OnLoginFailed);  
        }

        public void Register(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                _view.ShowError("Username cannot be empty");
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                _view.ShowError("Password cannot be empty");
                return;
            }
            _loginService.Register(username, password, OnLoginComplete, OnRegisterFailed);
        }

        private void OnLoginFailed(string errorMessage)
        {
            _view.OnLoginFail(errorMessage);
        }

        private void OnRegisterFailed(string errorMessage)
        {
            _view.OnLoginFail(errorMessage);
        }

        private void OnLoginComplete(UserResponseDto response)
        {
            PlayerPrefs.SetString(PlayerPrefsHelper.UserId, response.guid);
            PlayerPrefs.SetString(PlayerPrefsHelper.UserName, response.username);
            PlayerPrefs.SetString(PlayerPrefsHelper.FriendCode, response.friendCode);
            PlayerPrefs.SetString(PlayerPrefsHelper.AccessToken, response.accessToken);
            PlayerPrefs.SetString(PlayerPrefsHelper.RefreshToken, response.refreshToken);
            PlayerPrefs.Save();
            _view.OnLoginComplete();
        }
    }
}