﻿using System;
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
        [SerializeField] private Button _optionsButton;
        [SerializeField] private Button _rulesButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private GameObject _matchMakingContainer;
        [SerializeField] private GameObject _matchFoundContainer;
        [SerializeField] private TextMeshProUGUI _matchMakingTimerLabel;
        private float timerStartTime;
        private bool timerRunning = false;
        private HomePresenter _presenter;
        
        public void OnOpening()
        {
            _matchFoundContainer.SetActive(false);
            _presenter = new HomePresenter(this, _servicesProvider.GetMatchService(), _servicesProvider.GetTokenService());

            ActivateButtons();

            _playButton.onClick.AddListener(PlayMatch);
            _rulesButton.onClick.AddListener(OpenRules);
            _exitButton.onClick.AddListener(Application.Quit);
            this.gameObject.SetActive(true);
        }

        public void OnClosing()
        {
            _rulesButton.onClick.RemoveAllListeners();
            _playButton.onClick.RemoveAllListeners();
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
            _presenter.StartSearchingMatch();
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
            StopTimer();
            // _matchMakingContainer.SetActive(false);
            _matchFoundContainer.SetActive(true);
            _navigator.OpenGameView(matchStatus);
        }

        public void OnStartLookingForMatch()
        {
            StartTimer();
        }

        public void OnError(string message)
        {
            throw new NotImplementedException();
        }

        private void DeactivateButtons() {
            //_optionsButton.interactable = false;
            _playButton.interactable = false;
            _rulesButton.interactable = false;
            _exitButton.interactable = false;
        }

        private void ActivateButtons()
        {
            _playButton.interactable = true;
            _rulesButton.interactable = true;
            _exitButton.interactable = true;
        }
    }
}