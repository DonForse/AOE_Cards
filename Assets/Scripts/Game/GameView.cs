﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Home;
using Infrastructure.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class GameView : MonoBehaviour, IGameView, IView
    {
        [SerializeField] private ServicesProvider servicesProvider;
        [SerializeField] private GameObject unitCardGo;
        [SerializeField] private GameObject upgradeCardGo;

        [SerializeField] private GameObject unitCardsContainer;
        [SerializeField] private GameObject upgradeCardsContainer;
        [SerializeField] private GameObject roundCardContainer;
        [SerializeField] private GameObject upgradeCardShowDownContainer;
        [SerializeField] private GameObject unitCardShowDownContainer;
        [SerializeField] private Button buttonToggleHandCards;

        private MatchState matchState;
        private GamePresenter _presenter;

        private UnitCardView _unitCardPlayed;
        private UpgradeCardView _upgradeCardPlayed;

        private bool showingUpgrades;

        public void OnOpening()
        {
            _presenter = new GamePresenter(this, servicesProvider.GetPlayService());
            buttonToggleHandCards.onClick.AddListener(ToggleHandCards);
            this.gameObject.SetActive(true);
        }

        public void SetGame(Match matchStatus)
        {
            _presenter.GameSetup(matchStatus);
            matchState = MatchState.InitializeGame;

            InvokeRepeating("GetRound", 3f, 3f);
        }

        public void OnClosing()
        {
            this.gameObject.SetActive(false);
        }

        private void ToggleHandCards()
        {
            if(showingUpgrades)
                ShowHandUnits();
            else 
                ShowHandUpgrades();
        }
        
        private void ShowHandUnits()
        {
            showingUpgrades = false;
            //TODO: change image
            //animations (control toggle from animation?)
            upgradeCardsContainer.SetActive(false);
            unitCardsContainer.SetActive(true);

            foreach (var unitButton in unitCardsContainer.GetComponentsInChildren<Button>())
                unitButton.interactable = true;

            foreach (var upgradeButton in upgradeCardsContainer.GetComponentsInChildren<Button>())
                upgradeButton.interactable = false;
        }

        private void ShowHandUpgrades()
        {
            showingUpgrades = true;
            //TODO: change image
            //animations (control toggle from animation?)
            upgradeCardsContainer.SetActive(true);
            unitCardsContainer.SetActive(false);

            foreach (var unitButton in unitCardsContainer.GetComponentsInChildren<Button>())
                unitButton.interactable = false;

            foreach (var upgradeButton in upgradeCardsContainer.GetComponentsInChildren<Button>())
                upgradeButton.interactable = true;
        }

        public void InitializeRound(Round round)
        {
            matchState = MatchState.StartRound;
            ClearGameObjectData();

            matchState = MatchState.RoundUpgradeReveal;
            ShowRoundUpgradeCard(round.UpgradeCardRound);

            ShowHandUpgrades();
            matchState = MatchState.SelectUpgrade;
        }

        public void InitializeHand(Hand hand)
        {
            //unitCards = new List<UnitCardView>();
            //upgradeCards = new List<UpgradeCardView>();
            foreach (var card in hand.GetUnitCards())
            {
                var go = Instantiate(unitCardGo, unitCardsContainer.transform);
                var unitCard = go.GetComponent<UnitCardView>();
                var button = go.GetComponent<Button>();
                button.onClick.AddListener(() => PlayUnitCard(unitCard));
                unitCard.SetCard(card);
                //unitCards.Add(unitCard);
            }
            foreach (var card in hand.GetUpgradeCards())
            {
                var go = GameObject.Instantiate(upgradeCardGo, upgradeCardsContainer.transform);
                var upgradeCard = go.GetComponent<UpgradeCardView>();
                var button = go.GetComponent<Button>();
                button.onClick.AddListener(() => PlayUpgradeCard(upgradeCard));
                upgradeCard.SetCard(card);
                //upgradeCards.Add(upgradeCard);
            }
        }

        public void ShowUpgradeCardsPlayedByPlayer(string player)
        {
            throw new System.NotImplementedException();
        }

        public void ShowError(string message)
        {
            Debug.LogError(message);
        }

        public void UpgradeCardSentPlay()
        {
            matchState = MatchState.WaitUpgrade;
            //animation stuff
            _upgradeCardPlayed.transform.SetParent(upgradeCardShowDownContainer.transform);
            Canvas.ForceUpdateCanvases();
        }

        public void GetRound()
        {
            //TODO: BE SUPER CAREFUL TO CHECK ASYNCHRONOUS PROBLEMS.
            if (!(matchState == MatchState.WaitUpgrade || matchState == MatchState.WaitUnit))
                return;
            _presenter.GetRound();
        }

        public void OnGetRoundInfo(Round round)
        {
            if (matchState == MatchState.RoundResultReveal)
            {
                InitializeRound(round);
            }
            if (matchState == MatchState.WaitUpgrade)
            {
                ShowUpgradeCardsPlayedRound(round);
                ShowHandUnits();
                return;
            }
            if (matchState == MatchState.WaitUnit)
            {
                ShowUnitCardsPlayedRound(round);
                return;
            }
        }

        public void UnitCardSentPlay()
        {
            matchState = MatchState.WaitUnit;
            //animation stuff
            _unitCardPlayed.transform.SetParent(unitCardShowDownContainer.transform);
            Canvas.ForceUpdateCanvases();
        }

        private void ShowUpgradeCardsPlayedRound(Round round)
        {
            var cards = round.CardsPlayed.Where(cp => cp.Player != PlayerPrefs.GetString(PlayerPrefsHelper.UserName));
            foreach (var card in cards)
            {
                if (card.UpgradeCardData == null)
                    continue;
                var go = GameObject.Instantiate(upgradeCardGo, upgradeCardsContainer.transform);
                var upgradeCard = go.GetComponent<UpgradeCardView>();
                upgradeCard.SetCard(card.UpgradeCardData);
                go.transform.SetParent(upgradeCardShowDownContainer.transform);
            }
            matchState = MatchState.SelectUnit;
        }

        private void ShowUnitCardsPlayedRound(Round round)
        {
            var cards = round.CardsPlayed.Where(cp => cp.Player != PlayerPrefs.GetString(PlayerPrefsHelper.UserName));
            foreach (var card in cards)
            {
                if (card.UnitCardData == null)
                    continue;
                var go = GameObject.Instantiate(unitCardGo, unitCardsContainer.transform);
                var unitCard = go.GetComponent<UnitCardView>();
                unitCard.SetCard(card.UnitCardData);
                go.transform.SetParent(unitCardShowDownContainer.transform);
            }
            matchState = MatchState.RoundResultReveal;
            //do some animation
            StartCoroutine(StartNewRound());
        }

        private IEnumerator StartNewRound()
        {
            yield return new WaitForSeconds(2f);

            _presenter.StartNewRound();
        }

        private void ClearGameObjectData()
        {
            _unitCardPlayed = null;
            _upgradeCardPlayed = null;
            foreach (var unitButton in unitCardsContainer.GetComponentsInChildren<Button>())
                unitButton.interactable = false;

            foreach (var upgradeButton in upgradeCardsContainer.GetComponentsInChildren<Button>())
                upgradeButton.interactable = true;
        }

        private void CardReveal(Round roundResult)
        {
            //clear round data.
            //unitCardShowDownContainer;
            //upgradeCardShowDownContainer();
        }

        private void ShowRoundUpgradeCard(UpgradeCardData upgradeCardData)
        {
            var go = GameObject.Instantiate(upgradeCardGo, roundCardContainer.transform);
            var upgradeCard = go.GetComponent<UpgradeCardView>();
            upgradeCard.SetCard(upgradeCardData);
            //animation stuff.  
        }

        private void PlayUnitCard(UnitCardView unitCard)
        {
            if (_unitCardPlayed != null)
                return;
            _unitCardPlayed = unitCard;
            _presenter.PlayUnitCard(unitCard.CardName);

        }

        private void PlayUpgradeCard(UpgradeCardView upgradeCard)
        {
            if (_upgradeCardPlayed != null)
                return;
            _upgradeCardPlayed = upgradeCard;
            _presenter.PlayUpgradeCard(upgradeCard.CardName);
        }

        private void ShowUnitCard()
        {
            //animation of making the card bigger when hover
        }

        private void ShowRoundCard()
        {
            //animation of making the card bigger when hover
        }
    }
}