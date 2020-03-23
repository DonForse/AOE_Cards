using System;
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
        [SerializeField] private Button buttonHandUnits;
        [SerializeField] private Button buttonHandUpgrades;

        private GamePresenter _presenter;

        //private IList<UnitCardView> unitCards;
        //private IList<UpgradeCardView> upgradeCards;

        private UnitCardView _unitCardPlayed;
        private UpgradeCardView _upgradeCardPlayed;

        public void OnOpening()
        {
            _presenter = new GamePresenter(this, servicesProvider.GetMatchService());
            buttonHandUnits.onClick.AddListener(ShowHandUnits);
            buttonHandUpgrades.onClick.AddListener(ShowHandUpgrades);
            this.gameObject.SetActive(true);
        }

        private void ShowHandUnits()
        {
            buttonHandUnits.interactable = false;
            buttonHandUpgrades.interactable = true;
            //animations (control toggle from animation?)
            upgradeCardsContainer.SetActive(false);
            unitCardsContainer.SetActive(true);
        }

        private void ShowHandUpgrades()
        {
            buttonHandUpgrades.interactable = false;
            buttonHandUnits.interactable = true;
            //animations (control toggle from animation?)
            upgradeCardsContainer.SetActive(true);
            unitCardsContainer.SetActive(false);
        }

        public void OnClosing()
        {
            this.gameObject.SetActive(false);
        }

        public void SetGame(MatchStatus matchStatus)
        {
            _presenter.GameSetup(matchStatus);
        }

        public void StartRound() {
            _unitCardPlayed = null;
            _upgradeCardPlayed = null;
            foreach (var unitButton in unitCardsContainer.GetComponentsInChildren<Button>())
                unitButton.interactable = false;

            foreach (var upgradeButton in upgradeCardsContainer.GetComponentsInChildren<Button>())
                upgradeButton.interactable = false;
        }

        public void ShowRoundUpgradeCard(UpgradeCardData upgradeCardData)
        {
            var go = GameObject.Instantiate(upgradeCardGo, roundCardContainer.transform);
            var upgradeCard = go.GetComponent<UpgradeCardView>();
            upgradeCard.SetCard(upgradeCardData);
            //animation stuff.  
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

        public void ShowPlayerHand(Hand hand)
        {
            ShowPlayerUnitsCard(hand.GetUnitCards());
            ShowPlayerUpgradeCards(hand.GetUpgradeCards());
        }

        private void ShowPlayerUpgradeCards(IList<UpgradeCardData> list)
        {
            unitCardsContainer.SetActive(false);
            upgradeCardsContainer.SetActive(true);
            //set active and do animation
            //maybe add reroll chances
        }

        private void ShowPlayerUnitsCard(IList<UnitCardData> cards)
        {
            upgradeCardsContainer.SetActive(false);
            unitCardsContainer.SetActive(true);
            //set active and do animation
            //maybe add reroll chances
        }

        public void ShowUnitCard()
        {
            //animation of making the card bigger when hover
        }

        public void ShowRoundCard()
        {
            //animation of making the card bigger when hover
        }

        public void ShowUpgradeCardsPlayedByPlayer(string player)
        {
            throw new System.NotImplementedException();
        }

        public void ShowError()
        {
            throw new System.NotImplementedException();
        }

        public void UpgradeCardSentPlay()
        {
            //animation stuff
            _upgradeCardPlayed.transform.SetParent(upgradeCardShowDownContainer.transform);
            Canvas.ForceUpdateCanvases();
        }

        public void UnitCardSentPlay()
        {
            //animation stuff
            _unitCardPlayed.transform.SetParent(unitCardShowDownContainer.transform);
            Canvas.ForceUpdateCanvases();
        }

        public void CardReveal(RoundResult roundResult)
        {
            //clear round data.
            //unitCardShowDownContainer;
            //upgradeCardShowDownContainer();
        }

        public void ShowUpgradeCardsPlayedRound(Round round)
        {
            var cards = round.CardsPlayed.Where(cp => cp.Player != "you");
            foreach (var card in cards) {
                var go = GameObject.Instantiate(upgradeCardGo, upgradeCardsContainer.transform);
                var upgradeCard = go.GetComponent<UpgradeCardView>();
                upgradeCard.SetCard(card.UpgradeCardData);
                go.transform.SetParent(upgradeCardShowDownContainer.transform);
            }
        }
    }
}