using System;
using Common;
using Common.Utilities;
using Home;
using Login.Scripts.Presentation;
using Login.UnityDelivery;
using Sound;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Login.Scripts.Delivery
{
    public class LoginView : MonoBehaviour, IView, ILoginView
    {
        [SerializeField] private Button openLoginButton;
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

        public IObservable<(string username, string password)> OnLoginButtonPressed() => continueButton
            .OnClickAsObservable()
            .ThrottleFirst(TimeSpan.FromSeconds(1))
            .Select(_ => (username.text, password.text));

        public IObservable<Unit> OnGuestButtonPressed() => guestButton
            .OnClickAsObservable()
            .ThrottleFirst(TimeSpan.FromSeconds(1));

        public IObservable<(string username, string password)> OnRegisterButtonPressed()
        {
            throw new NotImplementedException();
        }

        public void OnOpening()
        {
            SoundManager.Instance.PlayBackground(mainThemeClip, new AudioClipOptions {loop = true}, true);
            _presenter = new LoginPresenter(this, servicesProvider.GetLoginService(), new PlayerPrefsWrapper());
            _presenter.Initialize();
            // _presenter.OnLoginComplete.Subscribe(_ => OnLoginComplete()).AddTo(_disposables);
            // _presenter.OnLoginError.Subscribe(OnLoginFail).AddTo(_disposables);

            RegisterButtons();
            EnableButtons();
            this.gameObject.SetActive(true);
        }

        public void OnClosing()
        {
            _disposables.Clear();
            _presenter.Dispose();
            this.gameObject.SetActive(false);
        }

        private void RegisterButtons()
        {
            openLoginButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1)).Subscribe(_ => OpenLoginMenu())
                .AddTo(_disposables);
            registerButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1)).Subscribe(_ => Register())
                .AddTo(_disposables);
            backButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1)).Subscribe(_ => Back())
                .AddTo(_disposables);
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

        private void OpenLoginMenu()
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

        public void NavigateToHomeView()
        {
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

        public void DisableButtons()
        {
            openLoginButton.interactable = false;
            registerButton.interactable = false;
            continueButton.interactable = false;
            guestButton.interactable = false;
            backButton.interactable = false;
        }

        public void EnableButtons()
        {
            openLoginButton.interactable = true;
            registerButton.interactable = true;
            continueButton.interactable = true;
            guestButton.interactable = true;
            backButton.interactable = true;
        }
    }
}