using System;
using Infrastructure.Services;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Home
{
    public class HomeView : MonoBehaviour, IView
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

        public void OnOpening()
        {
            SoundManager.Instance.PlayBackground(mainThemeClip, new AudioClipOptions { loop = true }, false);
            _matchFoundContainer.SetActive(false);
            _presenter = new HomePresenter(_servicesProvider.GetMatchService(), _servicesProvider.GetTokenService());
            _presenter.OnError.Subscribe(error => OnError(error)).AddTo(_disposables);
            _presenter.OnMatchFound.Subscribe(match => OnMatchFound(match)).AddTo(_disposables);

            EnableButtons();

            _userCodeLabel.text = PlayerPrefs.GetString(PlayerPrefsHelper.FriendCode);

            _playButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1)).Subscribe(_ => PlayMatch()).AddTo(_disposables);
            _openBotMenuButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1)).Subscribe(_ => OpenBotMenu());
            _playhardBotButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1)).Subscribe(_ => PlayVersusBotHard());
            _playBotButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1)).Subscribe(_ => PlayVersusBot());
            _closeBotMenuButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1)).Subscribe(_ => CloseBotMenu()).AddTo(_disposables);
            _rulesButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1)).Subscribe(_ => OpenRules()).AddTo(_disposables);
            _playFriendButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1)).Subscribe(_ => PlayVersusFriend()).AddTo(_disposables);
            _openPlayFriendButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1)).Subscribe(_ => OpenVersusFriend()).AddTo(_disposables);
            _closePlayFriendButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1)).Subscribe(_ => CloseVersusFriend()).AddTo(_disposables);
            _leaveQueueButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1)).Subscribe(_ => LeaveQueue()).AddTo(_disposables);

            if (_exitButton != null)
                _exitButton.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(1)).Subscribe(_ => Application.Quit()).AddTo(_disposables);
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
        private void PlayMatch()
        {
            DisableButtons();
            _presenter.StartSearchingMatch(false, false, string.Empty);
            OnStartLookingForMatch(false);

            //navigator.
        }

        private void PlayVersusBot()
        {
            CloseBotMenu();
            DisableButtons();
            _presenter.StartSearchingMatch(true, false, string.Empty, 0);
            OnStartLookingForMatch(true);

            //navigator.
        }

        private void PlayVersusBotHard()
        {
            CloseBotMenu();
            DisableButtons();
            _presenter.StartSearchingMatch(true, false, string.Empty, 1);
            OnStartLookingForMatch(true);
        }

        private void OpenBotMenu()
        {
            _playBotContainer.SetActive(true);
        }

        private void CloseBotMenu()
        {
            _playBotContainer.SetActive(false);
        }


        private void OpenVersusFriend()
        {
            _playFriendContainer.SetActive(true);
        }

        private void CloseVersusFriend()
        {
            _playFriendContainer.SetActive(false);
        }

        private void LeaveQueue()
        {
            _presenter.LeaveQueue();
            OnQueueLeft();
        }

        private void OnQueueLeft()
        {
            StopTimer();
            EnableButtons();
            _matchMakingContainer.SetActive(false);
        }

        private void PlayVersusFriend()
        {
            CloseVersusFriend();
            DisableButtons();
            _presenter.StartSearchingMatch(false, true, _friendCode.text);
            //navigator.
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

        private  void OnMatchFound(Match matchStatus)
        {
            _matchFoundContainer.SetActive(true);
            StopTimer();
            // _matchMakingContainer.SetActive(false);
            _navigator.OpenGameView(matchStatus);

        }

        private void OnStartLookingForMatch(bool vsBot)
        {
            if (vsBot)
            {
                _matchFoundContainer.SetActive(true);
                return;
            }
            StartTimer();
        }

        private void OnError(string message)
        {
            Toast.Instance.ShowToast("An Error Ocurred, please log in again", "Error");
            _navigator.OpenLoginView();
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