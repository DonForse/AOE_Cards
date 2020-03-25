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
            _loginService.Login(username, password, OnLoginComplete,OnLoginFailed);
        
        }

        public void Register(string username, string password)
        {
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

        private void OnLoginComplete(UserResponseDto response) {
            PlayerPrefs.SetString(PlayerPrefsHelper.UserId, response.id);
            PlayerPrefs.SetString(PlayerPrefsHelper.UserName, response.username);
            _view.OnLoginComplete();
        }

    }
}