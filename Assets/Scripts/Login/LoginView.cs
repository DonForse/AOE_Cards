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
        [SerializeField] private Button guestButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private GameObject actionsContainer;
        [SerializeField] private GameObject loginContainer;
        [SerializeField] private TMP_InputField username;
        [SerializeField] private TMP_InputField password;
        [SerializeField] private TextMeshProUGUI errorMessage;
        [SerializeField] private ServicesProvider servicesProvider;
        [SerializeField] private Navigator navigator;
        [SerializeField] private AudioClip mainThemeClip;

        private LoginPresenter _presenter;
        private string action;
        public void OnOpening()
        {
            SoundManager.Instance.PlayBackground(mainThemeClip, new AudioClipOptions { loop = true }, true);
            _presenter = new LoginPresenter(this, servicesProvider.GetLoginService());

            ActivateButtons();

            loginButton.onClick.AddListener(Login);
            registerButton.onClick.AddListener(Register);
            continueButton.onClick.AddListener(SendLogin);
            guestButton.onClick.AddListener(GuestLogin);
            backButton.onClick.AddListener(Back);
            this.gameObject.SetActive(true);
        }

        private void Back()
        {
            ActivateButtons();

            action = "";
            errorMessage.gameObject.SetActive(false);
            errorMessage.text = "";
            loginContainer.SetActive(false);
            actionsContainer.SetActive(true);
        }

        private void SendLogin()
        {
            DeactivateButtons();
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

        private void GuestLogin()
        {
            DeactivateButtons();
            _presenter.Register("GUEST", Guid.NewGuid().ToString());
        }

        public void OnLoginComplete() {
            navigator.OpenHomeView();
        }
    
        public void OnLoginFail(string message)
        {
            Toast.Instance.ShowToast(message, "Error");
            errorMessage.text = message;
            errorMessage.gameObject.SetActive(true);

            ActivateButtons();
        }

        public void OnClosing()
        {
            loginButton.onClick.RemoveAllListeners();
            this.gameObject.SetActive(false);
        }

        public void ShowError(string error)
        {
            Toast.Instance.ShowToast(error, "Warning");
            errorMessage.text = error;
            errorMessage.gameObject.SetActive(true);
            ActivateButtons();
        }

        private void DeactivateButtons()
        {
            loginButton.interactable = false;
            registerButton.interactable = false;
            continueButton.interactable = false;
            guestButton.interactable = false;
            backButton.interactable = false;
        }

        private void ActivateButtons()
        {
            loginButton.interactable = true;
            registerButton.interactable = true;
            continueButton.interactable = true;
            guestButton.interactable = true;
            backButton.interactable = true;
        }
    }
}
