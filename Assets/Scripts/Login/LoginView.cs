using System;
using Home;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Login
{
    public class LoginView : MonoBehaviour, IView, ILoginView
    {
        [SerializeField] private Button loginButton;
        [SerializeField] private Button registerButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private GameObject actionsContainer;
        [SerializeField] private GameObject loginContainer;
        [SerializeField] private TextMeshProUGUI username;
        [SerializeField] private TMP_InputField password;
        [SerializeField] private TextMeshProUGUI errorMessage;
        [SerializeField] private ServicesProvider servicesProvider;
        [SerializeField] private Navigator navigator;
        private LoginPresenter _presenter;
        private string action;
        public void OnOpening()
        {
            _presenter = new LoginPresenter(this, servicesProvider.GetLoginService());
            loginButton.onClick.AddListener(Login);
            registerButton.onClick.AddListener(Register);
            continueButton.onClick.AddListener(SendLogin);
            backButton.onClick.AddListener(Back);
            this.gameObject.SetActive(true);
        }

        private void Back()
        {
            action = "";
            errorMessage.gameObject.SetActive(false);
            errorMessage.text = "";
            loginContainer.SetActive(false);
            actionsContainer.SetActive(true);
        }

        private void SendLogin()
        {
            if (action == "login")
                _presenter.Login(username.text, password.text);
            if (action == "register")
                _presenter.Register(username.text, password.text);
        }

        private void Login()
        {
            action = "login";
            actionsContainer.SetActive(false);
            loginContainer.SetActive(true);

        }

        private void Register()
        {
            action = "register";
            actionsContainer.SetActive(false);
            loginContainer.SetActive(true);
        }

        public void OnLoginComplete() {
            navigator.OpenHomeView();
        }
    
        public void OnLoginFail(string message)
        {
            errorMessage.text = message;
            errorMessage.gameObject.SetActive(true);
        }

        public void OnClosing()
        {
            loginButton.onClick.RemoveAllListeners();
            this.gameObject.SetActive(false);
        }
    }
}
