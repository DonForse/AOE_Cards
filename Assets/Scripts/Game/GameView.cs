using System;
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
        [SerializeField] private GameObject _showDrawnHandContainer;
        [SerializeField] private UpgradesView _upgradesView;
        [SerializeField] private GameInfoView _gameInfoView;
        [SerializeField] private HandView _handView;
        [SerializeField] private ShowdownView _showdownView;

        private MatchState matchState;
        private GamePresenter _presenter;

        private UnitCardView _unitCardPlayed;
        private UpgradeCardView _upgradeCardPlayed;



        public void OnOpening()
        {
            _presenter = new GamePresenter(this, servicesProvider.GetPlayService());
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


        public void InitializeRound(Round round)
        {
            matchState = MatchState.StartRound;
            ClearGameObjectData();

            matchState = MatchState.RoundUpgradeReveal;
            ShowRoundUpgradeCard(round.UpgradeCardRound);

            _handView.ShowHandUpgrades();
            matchState = MatchState.SelectUpgrade;
        }

        public void InitializeHand(Hand hand)
        {
            //unitCards = new List<UnitCardView>();
            //upgradeCards = new List<UpgradeCardView>();
            var units = new List<GameObject>();
            foreach (var card in hand.GetUnitCards())
            {
                var go = Instantiate(unitCardGo);
                var unitCard = go.GetComponent<UnitCardView>();
                var button = go.GetComponent<Button>();
                button.onClick.AddListener(() => PlayUnitCard(unitCard));
                unitCard.SetCard(card);
                //_handView.SetUnitCard(go);
                units.Add(go);
                //unitCards.Add(unitCard);
            }
            var upgrades = new List<GameObject>();
            foreach (var card in hand.GetUpgradeCards())
            {
                var go = GameObject.Instantiate(upgradeCardGo);
                var upgradeCard = go.GetComponent<UpgradeCardView>();
                var button = go.GetComponent<Button>();
                button.onClick.AddListener(() => PlayUpgradeCard(upgradeCard));
                upgradeCard.SetCard(card);
                //_handView.SetUpgradeCard(go);
                upgrades.Add(go);
            }
            StartCoroutine(ShowDrawnCards(units, upgrades));
        }

        private IEnumerator ShowDrawnCards(IList<GameObject> units, IList<GameObject> upgrades)
        {
            //IList<GameObject> units = _handView.GetUnitCards();
            //IList<GameObject> upgrades = _handView.GetUpgradeCards();
            foreach (var unit in units) {
                unit.transform.SetParent(_showDrawnHandContainer.transform);
            }
            foreach (var upgrade in upgrades)
            {
                upgrade.transform.SetParent(_showDrawnHandContainer.transform);
            }
            _showDrawnHandContainer.SetActive(true);
            yield return new WaitForSeconds(2f);
            _showDrawnHandContainer.SetActive(false);
            foreach (var unit in units)
            {
                _handView.SetUnitCard(unit);
            }
            foreach (var upgrade in upgrades)
            {
                _handView.SetUpgradeCard(upgrade);
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
            _showdownView.PlayUpgradeCard(_upgradeCardPlayed, PlayerType.Player);
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
                _handView.ShowHandUnits();
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
            _showdownView.PlayUnitCard(_unitCardPlayed, PlayerType.Player);
        }

        private void ShowUpgradeCardsPlayedRound(Round round)
        {
            var rivalCards = round.CardsPlayed.Where(cp => cp.Player != PlayerPrefs.GetString(PlayerPrefsHelper.UserName));
            foreach (var card in rivalCards)
            {
                if (card.UpgradeCardData == null)
                    continue;
                var go = GameObject.Instantiate(upgradeCardGo);
                var upgradeCard = go.GetComponent<UpgradeCardView>();
                upgradeCard.SetCard(card.UpgradeCardData);

                _showdownView.PlayUpgradeCard(upgradeCard, PlayerType.Rival);
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
                var go = GameObject.Instantiate(unitCardGo);
                var unitCard = go.GetComponent<UnitCardView>();
                unitCard.SetCard(card.UnitCardData);

                _showdownView.PlayUnitCard(unitCard, PlayerType.Rival);
            }
            matchState = MatchState.RoundResultReveal;
            StartCoroutine(StartNewRound());
        }

        private IEnumerator StartNewRound()
        {
            //do some animation stuff
            yield return new WaitForSeconds(2f);

            _presenter.StartNewRound();
        }

        private void ClearGameObjectData()
        {
            _unitCardPlayed = null;
            _upgradeCardPlayed = null;
        }

        private void CardReveal(Round roundResult)
        {
            //clear round data.
            //unitCardShowDownContainer;
            //upgradeCardShowDownContainer();
        }

        private void ShowRoundUpgradeCard(UpgradeCardData upgradeCardData)
        {
            var go = GameObject.Instantiate(upgradeCardGo);
            var upgradeCard = go.GetComponent<UpgradeCardView>();
            upgradeCard.SetCard(upgradeCardData);
            _upgradesView.SetRoundUpgradeCard(go);
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