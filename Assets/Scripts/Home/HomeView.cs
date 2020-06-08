using System;
using Game;
using Infrastructure.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Home
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

        private float timerStartTime;
        private bool timerRunning = false;
        private HomePresenter _presenter;

        public void OnOpening()
        {
            SoundManager.Instance.PlayBackground(mainThemeClip, new AudioClipOptions { loop = true }, false);
            _matchFoundContainer.SetActive(false);
            _presenter = new HomePresenter(this, _servicesProvider.GetMatchService(), _servicesProvider.GetTokenService());

            ActivateButtons();

            _userCodeLabel.text = PlayerPrefs.GetString(PlayerPrefsHelper.FriendCode);

            _playButton.onClick.AddListener(PlayMatch);
            _openBotMenuButton.onClick.AddListener(OpenBotMenu);
            _playhardBotButton.onClick.AddListener(PlayVersusBotHard);
            _playBotButton.onClick.AddListener(PlayVersusBot);
            _closeBotMenuButton.onClick.AddListener(CloseBotMenu);
            _rulesButton.onClick.AddListener(OpenRules);
            _playFriendButton.onClick.AddListener(PlayVersusFriend);
            _openPlayFriendButton.onClick.AddListener(OpenVersusFriend);
            _closePlayFriendButton.onClick.AddListener(CloseVersusFriend);
            _leaveQueueButton.onClick.AddListener(LeaveQueue);
            if (_exitButton != null)
                _exitButton.onClick.AddListener(Application.Quit);
            this.gameObject.SetActive(true);
        }

        public void OnClosing()
        {
            _rulesButton.onClick.RemoveAllListeners();
            _playButton.onClick.RemoveAllListeners();
            _playBotButton.onClick.RemoveAllListeners();
            _rulesButton.onClick.RemoveAllListeners();
            _playFriendButton.onClick.RemoveAllListeners();
            _openPlayFriendButton.onClick.RemoveAllListeners();
            _closePlayFriendButton.onClick.RemoveAllListeners();
            _leaveQueueButton.onClick.RemoveAllListeners();
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
            DeactivateButtons();
            _navigator.OpenTutorialView();
        }
        private void PlayMatch()
        {
            DeactivateButtons();
            _presenter.StartSearchingMatch(false,false,string.Empty);
            //navigator.
        }

        private void PlayVersusBot()
        {
            CloseBotMenu();
            DeactivateButtons();
            _presenter.StartSearchingMatch(true, false, string.Empty, 0);
            //navigator.
        }

        private void PlayVersusBotHard()
        {
            CloseBotMenu();
            DeactivateButtons();
            _presenter.StartSearchingMatch(true, false, string.Empty, 1);
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
        }

        public void OnQueueLeft()
        {
            StopTimer();
            ActivateButtons();
            _matchMakingContainer.SetActive(false);
        }

        private void PlayVersusFriend()
        {
            CloseVersusFriend();
            DeactivateButtons();
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

        public void OnMatchFound(Match matchStatus)
        {
            _matchFoundContainer.SetActive(true);
            StopTimer();
            // _matchMakingContainer.SetActive(false);
            _navigator.OpenGameView(matchStatus);
        }

        public void OnStartLookingForMatch(bool vsBot)
        {
            if (vsBot)
            {
                _matchFoundContainer.SetActive(true);
                return;
            }
            StartTimer();
        }

        public void OnError(string message)
        {
            Toast.Instance.ShowToast("An Error Ocurred, please log in again", "Error");
            _navigator.OpenLoginView();
        }

        private void DeactivateButtons() {
            _playBotButton.interactable = false;
            _playhardBotButton.interactable = false;
            _playButton.interactable = false;
            _rulesButton.interactable = false;
            _openPlayFriendButton.interactable = false;
            if (_exitButton != null)
                _exitButton.interactable = false;
        }

        private void ActivateButtons()
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