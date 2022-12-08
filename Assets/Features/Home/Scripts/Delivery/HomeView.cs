using System;
using Features.Common.Utilities;
using Features.Home.Scripts.Domain;
using Features.Home.Scripts.Presentation;
using Features.Match.Domain;
using Features.Sound;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Home.Scripts.Delivery
{
    public class HomeView : MonoBehaviour, IView, IHomeView
    {
        [SerializeField] private ServicesProvider _servicesProvider;
        [SerializeField] private Navigator _navigator;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _playFriendButton;
        [SerializeField] private Button _openPlayFriendButton;
        [SerializeField] private Button _closePlayFriendButton;
        [SerializeField] private Button _playBotButton;
        [SerializeField] private Button _openBotMenuButton;
        [SerializeField] private Button _closeBotMenuButton;
        [SerializeField] private Button _playhardBotButton;
        [SerializeField] private Button _optionsButton;
        [SerializeField] private Button _rulesButton;
        [SerializeField] private Button _exitButton;

        [SerializeField] private Button _leaveQueueButton;

        [SerializeField] private GameObject _matchMakingContainer;
        [SerializeField] private GameObject _matchFoundContainer;
        [SerializeField] private GameObject _playFriendContainer;
        [SerializeField] private GameObject _playBotContainer;
        [SerializeField] private TextMeshProUGUI _matchMakingTimerLabel;
        [SerializeField] private TextMeshProUGUI _userCodeLabel;
        [SerializeField] private TMP_InputField _friendCode;
        [SerializeField] private AudioClip mainThemeClip;

        private CompositeDisposable _disposables = new CompositeDisposable();

        private float timerStartTime;
        private bool timerRunning = false;
        private HomePresenter _presenter;

        public IObservable<Unit> OnPlayMatch() =>
            _playButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1));

        public IObservable<Unit> OnPlayVersusHardBot() =>
            _playhardBotButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1))
                .DoOnSubscribe(CloseBotMenu);

        public IObservable<Unit> OnPlayVersusEasyBot() =>
            _playBotButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1))
                .DoOnSubscribe(CloseBotMenu);

        public IObservable<Unit> OnLeaveQueue() =>
            _leaveQueueButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1));

        public IObservable<string> OnPlayVersusFriend() =>
            _playFriendButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1))
                .DoOnSubscribe(CloseVersusFriend)
                .Select(_ => _friendCode.text);

        public void OnOpening()
        {
            SoundManager.Instance.PlayBackground(mainThemeClip, new AudioClipOptions { loop = true }, false);
            _matchFoundContainer.SetActive(false);
            _presenter = new HomePresenter(this, _servicesProvider.GetMatchService(),
                _servicesProvider.GetTokenService(),
                new PlayerPrefsWrapper(), new FindMatchInQueue(_servicesProvider.GetMatchService()));
            _presenter.Initialize();

            EnableButtons();

            _userCodeLabel.text = PlayerPrefs.GetString(PlayerPrefsHelper.FriendCode);

            _openBotMenuButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1))
                .Subscribe(_ => OpenBotMenu());
            _closeBotMenuButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1))
                .Subscribe(_ => CloseBotMenu()).AddTo(_disposables);
            _rulesButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1)).Subscribe(_ => OpenRules())
                .AddTo(_disposables);
            _openPlayFriendButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1))
                .Subscribe(_ => OpenVersusFriend()).AddTo(_disposables);
            _closePlayFriendButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1))
                .Subscribe(_ => CloseVersusFriend()).AddTo(_disposables);
           
            if (_exitButton != null)
                _exitButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1))
                    .Subscribe(_ => Application.Quit()).AddTo(_disposables);
            this.gameObject.SetActive(true);
        }

        public void OnClosing()
        {
            _presenter.Unload();
            _disposables.Clear();
            this.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (!timerRunning)
                return;
            _matchMakingTimerLabel.text = TimeSpan.FromSeconds(Time.time - timerStartTime).ToString(@"mm\:ss");
        }

        private void OpenRules()
        {
            DisableButtons();
            _navigator.OpenTutorialView();
        }

        private void OpenBotMenu() => _playBotContainer.SetActive(true);
        private void CloseBotMenu() => _playBotContainer.SetActive(false);
        private void OpenVersusFriend() => _playFriendContainer.SetActive(true);
        private void CloseVersusFriend() => _playFriendContainer.SetActive(false);

        public void LeftQueue()
        {
            StopTimer();
            EnableButtons();
            _matchMakingContainer.SetActive(false);
        }
        
        private void StartTimer()
        {
            timerStartTime = Time.time;
            timerRunning = true;
            _matchMakingContainer.SetActive(true);
        }

        private void StopTimer()
        {
            _matchMakingContainer.SetActive(false);
            timerRunning = false;
        }

        public void ShowMatchFound(GameMatch gameMatchStatus)
        {
            _matchFoundContainer.SetActive(true);
            StopTimer();
            _navigator.OpenGameView(gameMatchStatus);
        }

        public void ShowError(string message)
        {
            Toast.Instance.ShowToast("An Error Ocurred, please log in again", "Error");
            _navigator.OpenLoginView();
        }

        public void StartSearchingForMatch()
        {
            StartTimer();

            DisableButtons();
        }

        private void DisableButtons()
        {
            _playBotButton.interactable = false;
            _playhardBotButton.interactable = false;
            _playButton.interactable = false;
            _rulesButton.interactable = false;
            _openPlayFriendButton.interactable = false;
            if (_exitButton != null)
                _exitButton.interactable = false;
        }

        private void EnableButtons()
        {
            _playBotButton.interactable = true;
            _playButton.interactable = true;
            _playhardBotButton.interactable = true;
            _rulesButton.interactable = true;
            _openPlayFriendButton.interactable = true;
            if (_exitButton != null)
                _exitButton.interactable = true;
        }
    }
}