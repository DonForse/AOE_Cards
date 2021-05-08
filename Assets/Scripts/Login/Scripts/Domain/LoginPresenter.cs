using Infrastructure.Services;
using Login.Scripts.Infrastructure;
using UnityEngine;
using UniRx;
using System;

namespace Login.Scripts.Domain
{
    internal class LoginPresenter
    {
        
        private readonly ILoginService _loginService;
        private CompositeDisposable _disposables = new CompositeDisposable();

        private ISubject<Unit> _onLoginComplete = new Subject<Unit>();
        public IObservable<Unit> OnLoginComplete => _onLoginComplete;


        private ISubject<string> _onLoginError = new Subject<string>();
        public IObservable<string> OnLoginError => _onLoginError;

        public LoginPresenter(ILoginService loginService)
        {
            _loginService = loginService;
        }

        public void Login(string username, string password)
        {
            _loginService.Login(username, password)
                .DoOnError(err => OnLoginFailed(err.Message))
                .Subscribe(user => HandleLoginComplete(user))
                .AddTo(_disposables);
        }

        public void Register(string username, string password)
        {
            _loginService.Register(username, password)
                .DoOnError(err => OnLoginFailed(err.Message))
                .Subscribe(user => HandleLoginComplete(user)).AddTo(_disposables);
        }

        private void OnLoginFailed(string errorMessage)
        {
            _onLoginError.OnNext(errorMessage);
        }

        private void HandleLoginComplete(UserResponseDto response)
        {
            PlayerPrefs.SetString(PlayerPrefsHelper.UserId, response.guid);
            PlayerPrefs.SetString(PlayerPrefsHelper.UserName, response.username);
            PlayerPrefs.SetString(PlayerPrefsHelper.FriendCode, response.friendCode);
            PlayerPrefs.SetString(PlayerPrefsHelper.AccessToken, response.accessToken);
            PlayerPrefs.SetString(PlayerPrefsHelper.RefreshToken, response.refreshToken);
            PlayerPrefs.Save();
            _onLoginComplete.OnNext(Unit.Default);
        }

        public void Unload()
        {
            _disposables.Clear();
        }
    }
}