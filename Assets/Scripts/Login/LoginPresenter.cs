using System;
using Infrastructure.Services;
using UnityEngine;
using static Infrastructure.Services.LoginService;

internal class LoginPresenter
{
    private ILoginView _view;
    private ILoginService _loginService;

    public LoginPresenter(ILoginView loginView, ILoginService loginService)
    {
        _view = loginView;
        _loginService = loginService;
    }

    public void Login(string username, string password)
    {
        _loginService.Login(username, password, OnLoginComplete);
        
    }

    public void Register(string username, string password)
    {
        _loginService.Register(username, password, OnLoginComplete);
    }

    public void OnLoginComplete(UserResponseDto response) {
        PlayerPrefs.SetString(PlayerPrefsHelper.UserId, response.id);
        PlayerPrefs.SetString(PlayerPrefsHelper.UserName, response.username);
        _view.OnLoginCompleteLogin();
    }

}