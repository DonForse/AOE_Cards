using System;
using Infrastructure.Services;
using UnityEngine;

internal class LoginPresenter
{
    private ILoginView _view;
    private ILoginService _loginService;

    public LoginPresenter(ILoginView loginView, ILoginService loginService)
    {
        _view = loginView;
        _loginService = loginService;
    }

    public void Login(string text)
    {
        _loginService.Login(text, "a", OnLoginComplete);
        
    }

    public void OnLoginComplete(string response) {
        _view.OnLoginCompleteLogin();
    }

    internal void Register(string text)
    {
        _loginService.Register(text, "a", OnLoginComplete);
    }
}