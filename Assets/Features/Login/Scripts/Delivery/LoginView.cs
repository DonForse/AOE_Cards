using System;
using Features.Common.Utilities;
using Features.Home;
using Features.Login.Scripts.Domain;
using Features.Login.Scripts.Presentation;
using Features.Sound;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Login.Scripts.Delivery
{
    public class LoginView : MonoBehaviour, IView, ILoginView
    {
        [SerializeField] private Button openLoginButton;
        [SerializeField] private Button openRegisterButton;
        
        [SerializeField] private Button guestButton;
        [SerializeField] private Button closeLoginButton;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button registerButton;
        [SerializeField] private Button closeRegisterButton;
        [SerializeField] private GameObject actionsContainer;
        [SerializeField] private GameObject loginContainer;
        [SerializeField] private GameObject registerContainer;
        [SerializeField] private TMP_InputField username;
        [SerializeField] private TMP_InputField password;
        [SerializeField] private TextMeshProUGUI errorMessage;
        [SerializeField] private ServicesProvider servicesProvider;
        [SerializeField] private Navigator navigator;
        [SerializeField] private AudioClip mainThemeClip;
        private CompositeDisposable _disposables = new CompositeDisposable();

        private LoginPresenter _presenter;
        
        public IObservable<(string username, string password)> OnLoginButtonPressed() => loginButton
            .OnClickAsObservable()
            .ThrottleFirst(TimeSpan.FromSeconds(1))
            .Select(_ => (username.text, password.text));

        public IObservable<Unit> OnGuestButtonPressed() => guestButton
            .OnClickAsObservable()
            .ThrottleFirst(TimeSpan.FromSeconds(1));

        public IObservable<(string username, string password)> OnRegisterButtonPressed() => registerButton
            .OnClickAsObservable()
            .ThrottleFirst(TimeSpan.FromSeconds(1))
            .Select(_ => (username.text, password.text));

        public void NavigateToHomeView() => navigator.OpenHomeView();

        public void OnOpening()
        {
            SoundManager.Instance.PlayBackground(mainThemeClip, new AudioClipOptions {loop = true}, true);
            _presenter = new LoginPresenter(this, servicesProvider.GetLoginService(), new PlayerPrefsWrapper());
            _presenter.Initialize();

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
            openRegisterButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1)).Subscribe(_ => OpenRegister())
                .AddTo(_disposables);
            closeLoginButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1)).Subscribe(_ => CloseLogin())
                .AddTo(_disposables);
            closeRegisterButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1)).Subscribe(_ => CloseRegister())
                .AddTo(_disposables);
        }

        private void CloseLogin()
        {
            EnableButtons();

            errorMessage.gameObject.SetActive(false);
            errorMessage.text = "";
            loginContainer.SetActive(false);
            actionsContainer.SetActive(true);
        }
        
        private void CloseRegister()
        {
            EnableButtons();

            errorMessage.gameObject.SetActive(false);
            errorMessage.text = "";
            registerContainer.SetActive(false);
            actionsContainer.SetActive(true);
        }

        private void OpenLoginMenu()
        {
            actionsContainer.SetActive(false);
            loginContainer.SetActive(true);
        }

        private void OpenRegister()
        {
            actionsContainer.SetActive(false);
            registerContainer.SetActive(true);
        }

        public void ShowError(string message)
        {
            Toast.Instance.ShowToast(message, "Error");
            errorMessage.text = message;
            errorMessage.gameObject.SetActive(true);

            EnableButtons();
        }

        public void ShowWarning(string message)
        {
            Toast.Instance.ShowToast(message, "Warning");
            errorMessage.text = message;
            errorMessage.gameObject.SetActive(true);
            EnableButtons();
        }

        public void DisableButtons()
        {
            openLoginButton.interactable = false;
            openRegisterButton.interactable = false;
            registerButton.interactable = false;
            closeRegisterButton.interactable = false;
            loginButton.interactable = false;
            closeLoginButton.interactable = false;
            guestButton.interactable = false;
        }

        public void EnableButtons()
        {
            openLoginButton.interactable = true;
            openRegisterButton.interactable = true;
            registerButton.interactable = true;
            closeRegisterButton.interactable = true;
            loginButton.interactable = true;
            guestButton.interactable = true;
            closeLoginButton.interactable = true;
        }
    }
}