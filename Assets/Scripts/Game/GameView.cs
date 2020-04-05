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

        private int playersCount = 0;


        public void OnOpening()
        {
            _presenter = new GamePresenter(this, servicesProvider.GetPlayService());
            this.gameObject.SetActive(true);
        }

        public void SetGame(Match match)
        {
            matchState = MatchState.InitializeGame;

            _presenter.GameSetup(match);
            playersCount = match.users.Count();
            _gameInfoView.SetGame(match);

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
                unitCard.SetCard(card, PlayUnitCard, _showdownView.GetComponent<RectTransform>(), true);
                //_handView.SetUnitCard(go);
                units.Add(go);
                //unitCards.Add(unitCard);
            }
            var upgrades = new List<GameObject>();
            foreach (var card in hand.GetUpgradeCards())
            {
                var go = GameObject.Instantiate(upgradeCardGo);
                var upgradeCard = go.GetComponent<UpgradeCardView>();
                upgradeCard.SetCard(card, PlayUpgradeCard, _showdownView.GetComponent<RectTransform>(), true);
                //_handView.SetUpgradeCard(go);
                upgrades.Add(go);
            }
            StartCoroutine(ShowDrawnCards(units, upgrades));
        }

        private IEnumerator ShowDrawnCards(IList<GameObject> units, IList<GameObject> upgrades)
        {
            //IList<GameObject> units = _handView.GetUnitCards();
            //IList<GameObject> upgrades = _handView.GetUpgradeCards();
            foreach (var unit in units)
            {
                unit.transform.SetParent(_showDrawnHandContainer.transform);
            }
            foreach (var upgrade in upgrades)
            {
                upgrade.transform.SetParent(_showDrawnHandContainer.transform);
            }
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
            LayoutRebuilder.MarkLayoutForRebuild(_handView.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(_handView.GetComponent<RectTransform>());
            Canvas.ForceUpdateCanvases();
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
            if (round.CardsPlayed.Count(c => c.Player != PlayerPrefs.GetString(PlayerPrefsHelper.UserName)) < playersCount - 1)
                return;
            var rivalCards = round.CardsPlayed.Where(cp => cp.Player != PlayerPrefs.GetString(PlayerPrefsHelper.UserName));

            foreach (var card in rivalCards)
            {
                if (card.UpgradeCardData == null)
                    return;
                var go = GameObject.Instantiate(upgradeCardGo);
                var upgradeCard = go.GetComponent<UpgradeCardView>();
                upgradeCard.SetCard(card.UpgradeCardData, null, null, false);

                _showdownView.PlayUpgradeCard(upgradeCard, PlayerType.Rival);
            }
            matchState = MatchState.SelectUnit;
        }

        private void ShowUnitCardsPlayedRound(Round round)
        {
            if (round.CardsPlayed.Count(c => c.Player != PlayerPrefs.GetString(PlayerPrefsHelper.UserName)) < playersCount - 1)
                return;
            var cards = round.CardsPlayed.Where(cp => cp.Player != PlayerPrefs.GetString(PlayerPrefsHelper.UserName));
            foreach (var card in cards)
            {
                if (card.UnitCardData == null) //some player didnt play yet
                    return;
                var go = GameObject.Instantiate(unitCardGo);
                var unitCard = go.GetComponent<UnitCardView>();
                unitCard.SetCard(card.UnitCardData, (_) => { }, null, false);

                _showdownView.PlayUnitCard(unitCard, PlayerType.Rival);
            }
            matchState = MatchState.RoundResultReveal;
            StartCoroutine(StartNewRound(round));
        }

        private IEnumerator StartNewRound(Round round)
        {
            //do some animation stuff
            yield return new WaitForSeconds(2f);

            _showdownView.Clear(_upgradesView);
            var pt = round.WinnerPlayer == PlayerPrefs.GetString(PlayerPrefsHelper.UserName) ? PlayerType.Player : PlayerType.Rival;
            _gameInfoView.WinRound(pt);
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
            upgradeCard.SetCard(upgradeCardData, null, null, false);
            _upgradesView.SetRoundUpgradeCard(go);
            //animation stuff.  
        }

        private void PlayUnitCard(Draggable draggable)
        {
            if (matchState != MatchState.SelectUnit)
                return;
            var unitCard = draggable.GetComponent<UnitCardView>();
            if (_unitCardPlayed != null)
                return;
            _unitCardPlayed = unitCard;
            _presenter.PlayUnitCard(unitCard.CardName);

        }

        private void PlayUpgradeCard(Draggable draggable)
        {
            if (matchState != MatchState.SelectUpgrade)
                return;
            var upgradeCard = draggable.GetComponent<UpgradeCardView>();
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