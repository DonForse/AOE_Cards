using Home;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginView : MonoBehaviour, IView, ILoginView
{
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private TextMeshProUGUI username;
    [SerializeField] private ServicesProvider _servicesProvider;
    [SerializeField] private Navigator _navigator;
    private LoginPresenter _presenter;
    public void OnOpening()
    {
        _presenter = new LoginPresenter(this, _servicesProvider.GetLoginService());
        loginButton.onClick.AddListener(Login);
        registerButton.onClick.AddListener(Register);
        this.gameObject.SetActive(true);
    }

    private void Login()
    {
        _presenter.Login(username.text);
    }

    private void Register()
    {
        _presenter.Register(username.text);
    }

    public void OnLoginComplete() {
        _navigator.OpenHomeView();

    }
    public void OnClosing()
    {
        loginButton.onClick.RemoveAllListeners();
        this.gameObject.SetActive(false);
    }

    public void OnLoginCompleteLogin()
    {
        throw new NotImplementedException();
    }
}
