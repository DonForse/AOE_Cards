using Infrastructure.Services;
using Login.Scripts.Infrastructure;
using UnityEngine;
using UniRx;

namespace Login.Scripts.Domain
{
    internal class LoginPresenter
    {
        private readonly ILoginView _view;
        private readonly ILoginService _loginService;
        private CompositeDisposable _disposables = new CompositeDisposable();

        public LoginPresenter(ILoginView loginView, ILoginService loginService)
        {
            _view = loginView;
            _loginService = loginService;
        }

        public void Login(string username, string password)
        {
            _loginService.Login(username, password)
                .DoOnError(err => OnLoginFailed(err.Message))
                .Subscribe(user => OnLoginComplete(user))
                .AddTo(_disposables);
        }

        public void Register(string username, string password)
        {
            _loginService.Register(username, password)
                .DoOnError(err => OnLoginFailed(err.Message))
                .Subscribe(user => OnLoginComplete(user)).AddTo(_disposables);
        }

        private void OnLoginFailed(string errorMessage)
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

        public void Dispose()
        {
            _disposables.Clear();
        }
    }
}