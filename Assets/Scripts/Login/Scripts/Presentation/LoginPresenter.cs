using System;
using Common;
using Infrastructure.DTOs;
using Login.Scripts.Domain;
using Login.UnityDelivery;
using UniRx;
using UnityEngine;

namespace Login.Scripts.Presentation
{
    public class LoginPresenter : IDisposable
    {
        private readonly ILoginView _loginView;
        private readonly ILoginService _loginService;
        private readonly IPlayerPrefs _playerPrefs;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        private ISubject<string> _onLoginError = new Subject<string>();
        public IObservable<string> OnLoginError => _onLoginError;

        public LoginPresenter(ILoginView loginView, ILoginService loginService, IPlayerPrefs playerPrefs)
        {
            _loginView = loginView;
            _loginService = loginService;
            _playerPrefs = playerPrefs;
        }

        public void Initialize()
        {
            _loginView.OnLoginButtonPressed()
                .Subscribe(loginInfo=> Login(loginInfo.username, loginInfo.password))
                .AddTo(_disposables);
            
            _loginView.OnGuestButtonPressed()
                .Subscribe(loginInfo => GuestLogin())
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Clear();
        }

        private void Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username)){
                _loginView.ShowError("Username cannot be empty");
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                _loginView.ShowError("Password cannot be empty");
                return;
            }

            _loginView.DisableButtons();
            _loginService.Login(username, password)
                .DoOnError(err => OnLoginFailed(err.Message))
                .Subscribe(HandleLoginComplete)
                .AddTo(_disposables);
        }

        private void GuestLogin()
        {
            _loginView.DisableButtons();

            _loginService.Register("GUEST", string.Empty)
                .DoOnError(err => OnLoginFailed(err.Message))
                .Subscribe(HandleLoginComplete).AddTo(_disposables);
        }
        
        private void Register(string username, string password)
        {
            _loginView.DisableButtons();

            _loginService.Register(username, password)
                .DoOnError(err => OnLoginFailed(err.Message))
                .Subscribe(HandleLoginComplete).AddTo(_disposables);
        }

        private void OnLoginFailed(string errorMessage)
        {
            _onLoginError.OnNext(errorMessage);
        }

        private void HandleLoginComplete(UserResponseDto response)
        {
            _playerPrefs.SetString(PlayerPrefsHelper.UserId, response.guid);
            _playerPrefs.SetString(PlayerPrefsHelper.UserName, response.username);
            _playerPrefs.SetString(PlayerPrefsHelper.FriendCode, response.friendCode);
            _playerPrefs.SetString(PlayerPrefsHelper.AccessToken, response.accessToken);
            _playerPrefs.SetString(PlayerPrefsHelper.RefreshToken, response.refreshToken);
            _playerPrefs.Save();
            _loginView.NavigateToHomeView();
        }
    }
}