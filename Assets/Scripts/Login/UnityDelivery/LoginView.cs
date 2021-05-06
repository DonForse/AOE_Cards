using System;
using Home;
using Login.Scripts.Domain;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Login.UnityDelivery
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
        private CompositeDisposable _disposables = new CompositeDisposable();

        private LoginPresenter _presenter;
        private string action;
        public void OnOpening()
        {
            SoundManager.Instance.PlayBackground(mainThemeClip, new AudioClipOptions { loop = true }, true);
            _presenter = new LoginPresenter(this, servicesProvider.GetLoginService());
            
            RegisterButtons();
            EnableButtons();
            this.gameObject.SetActive(true);
        }

        public void OnClosing()
        {
            _disposables.Dispose();
            _presenter.Dispose();
            this.gameObject.SetActive(false);
        }
        
        private void RegisterButtons()
        {
            loginButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1)).Subscribe(_ => Login()).AddTo(_disposables);
            registerButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1)).Subscribe(_ => Register()).AddTo(_disposables);
            continueButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1)).Subscribe(_ => SendLogin()).AddTo(_disposables);
            guestButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1)).Subscribe(_ => GuestLogin()).AddTo(_disposables);
            backButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1)).Subscribe(_ => Back()).AddTo(_disposables);
        }

        private void Back()
        {
            EnableButtons();

            action = "";
            errorMessage.gameObject.SetActive(false);
            errorMessage.text = "";
            loginContainer.SetActive(false);
            actionsContainer.SetActive(true);
        }

        private void SendLogin()
        {
            if (string.IsNullOrWhiteSpace(username.text))
            {
                ShowError("Username cannot be empty");
                return;
            }
            if (string.IsNullOrWhiteSpace(password.text))
            {
                ShowError("Password cannot be empty");
                return;
            }

            DisableButtons();
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
            DisableButtons();
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

            EnableButtons();
        }

        public void ShowError(string error)
        {
            Toast.Instance.ShowToast(error, "Warning");
            errorMessage.text = error;
            errorMessage.gameObject.SetActive(true);
            EnableButtons();
        }

        private void DisableButtons()
        {
            loginButton.interactable = false;
            registerButton.interactable = false;
            continueButton.interactable = false;
            guestButton.interactable = false;
            backButton.interactable = false;
        }

        private void EnableButtons()
        {
            loginButton.interactable = true;
            registerButton.interactable = true;
            continueButton.interactable = true;
            guestButton.interactable = true;
            backButton.interactable = true;
        }
    }
}
