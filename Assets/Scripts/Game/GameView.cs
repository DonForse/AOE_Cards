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
        [SerializeField] private Navigator _navigator;
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
        public void OnClosing()
        {
            this.gameObject.SetActive(false);
        }

        public void SetGame(Match match)
        {
            matchState = MatchState.InitializeGame;

            _presenter.GameSetup(match);
            playersCount = match.Users.Count();
            _gameInfoView.SetGame(match);
            SetRounds(match);
            InvokeRepeating("GetRound", 3f, 3f);
        }

        private void SetRounds(Match match)
        {
            //SetAlreadyPlayedTurns();
            var currentRound = match.Board.Rounds.Last();
            var previousRounds = match.Board.Rounds.Where(r => r != currentRound);
            var ownCards = currentRound.CardsPlayed.FirstOrDefault(c => c.Player == PlayerPrefs.GetString(PlayerPrefsHelper.UserName));
            if (ownCards == null)
                UnexpectedError();

            if (currentRound.UpgradeCardRound != null)
                ShowRoundUpgradeCard(currentRound.UpgradeCardRound);

            if (ownCards.UpgradeCardData == null)
            {
                matchState = MatchState.SelectUpgrade;
                return;
            }
            foreach (var playerCard in currentRound.CardsPlayed)
            {
                var playerType = PlayerPrefs.GetString(PlayerPrefsHelper.UserName) == playerCard.Player ? PlayerType.Player : PlayerType.Rival;
                SetRoundUpgradeData(playerCard, playerType);
            }

            if (matchState == MatchState.WaitUpgrade)
                return;

            foreach (var playerCard in currentRound.CardsPlayed)
            {
                var playerType = PlayerPrefs.GetString(PlayerPrefsHelper.UserName) == playerCard.Player ? PlayerType.Player : PlayerType.Rival; 
                SetRoundUnitData(playerCard, playerType);
            }
        }

        private void SetRoundUnitData(PlayerCard playerCard, PlayerType playerType)
        {
            if (playerType == PlayerType.Player && playerCard == null)
            {
                matchState = MatchState.SelectUnit;
                return;
            }
            if (playerType == PlayerType.Rival && playerCard == null)
            {
                matchState = MatchState.WaitUnit;
                return;
            }
            if (playerCard.UnitCardData != null)
            {
                var go = Instantiate(unitCardGo);
                var unitCard = go.GetComponent<UnitCardView>();
                unitCard.SetCard(playerCard.UnitCardData, _ => { }, null, false, null);

                _showdownView.PlayUnitCard(unitCard, playerType);
            }
        }

        private void SetRoundUpgradeData(PlayerCard playerCard, PlayerType playerType)
        {
            if (playerType == PlayerType.Rival && playerCard.UpgradeCardData == null)
            {
                matchState = MatchState.WaitUpgrade;
            }
            if (playerCard.UpgradeCardData != null)
            {
                var go = Instantiate(upgradeCardGo);
                var upgradeCard = go.GetComponent<UpgradeCardView>();
                upgradeCard.SetCard(playerCard.UpgradeCardData, _ => { }, null, false, null );

                _showdownView.PlayUpgradeCard(upgradeCard, playerType);
            }
        }

        private void UnexpectedError()
        {
            throw new NotImplementedException("Unexpected");
        }

        public void InitializeGame(Match match) {

            InstantiateHandCards(match.Hand);
            InitializeRound(match.Board.Rounds.Last());
            if (match.Board.Rounds.Count == 1 && match.Board.Rounds.First().CardsPlayed.All(uc=>uc.UpgradeCardData == null))
                StartCoroutine(ShowDrawnCards());
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

        public void InstantiateHandCards(Hand hand)
        {
            var unitHandCards = _handView.GetUnitCards();
            foreach (var card in hand.GetUnitCards().GroupBy(card=>card.cardName))
            {
                var missingCards = card.Count() - unitHandCards.Count(cuc => cuc.name == card.Key);
                while (missingCards > 0)
                {
                    var go = Instantiate(unitCardGo);
                    var unitCard = go.GetComponent<UnitCardView>();
                    unitCard.SetCard(card.First(), PlayUnitCard, _showdownView.GetComponent<RectTransform>(), true, dragging => { _showdownView.CardDrag(dragging); });
                    _handView.SetUnitCard(go);
                    missingCards--;
                }
            }

            var upgradeHandCards = _handView.GetUpgradeCards();
            foreach (var card in hand.GetUpgradeCards().GroupBy(card => card.cardName))
            {
                var missingCards =  card.Count() - upgradeHandCards.Count(cuc => cuc.name == card.Key);
                while (missingCards > 0)
                {
                    var go = GameObject.Instantiate(upgradeCardGo);
                    var upgradeCard = go.GetComponent<UpgradeCardView>();
                    upgradeCard.SetCard(card.First(), PlayUpgradeCard, _showdownView.GetComponent<RectTransform>(), true, dragging => { _showdownView.CardDrag(dragging); } );
                    _handView.SetUpgradeCard(go);
                    missingCards--;
                }
            }
        }

        private IEnumerator ShowDrawnCards()
        {
            var units = _handView.GetUnitCards();
            var upgrades= _handView.GetUpgradeCards();
            foreach (var unit in units)
            {
                unit.transform.SetParent(_showDrawnHandContainer.transform);
                unit.transform.position = _showDrawnHandContainer.transform.position;
            }
            foreach (var upgrade in upgrades)
            {
                upgrade.transform.SetParent(_showDrawnHandContainer.transform);
                upgrade.transform.position = _showDrawnHandContainer.transform.position;
            }
            _showDrawnHandContainer.SetActive(true);
            yield return new WaitForSeconds(3f);
            foreach (var unit in units)
            {
                _handView.SetUnitCard(unit);
            }
            foreach (var upgrade in upgrades)
            {
                _handView.SetUpgradeCard(upgrade);
            }
            _showDrawnHandContainer.SetActive(false);
            LayoutRebuilder.MarkLayoutForRebuild(_handView.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(_handView.GetComponent<RectTransform>());
            Canvas.ForceUpdateCanvases();
        }

        public void ShowError(string message)
        {
            //_presenter.GetRound();
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
            if (matchState == MatchState.SelectUpgrade)
            {
                ShowRoundUpdatesUpgrade(round);
            }
            if (matchState == MatchState.SelectUnit)
            {
                ShowRoundUpdatesUnit(round);
            }

            if (matchState == MatchState.RoundResultReveal)
            {
                InitializeRound(round);
            }
            if (matchState == MatchState.WaitUpgrade)
            {
                ShowUpgradeCardsPlayedRound(round);
                return;
            }
            if (round.Finished && matchState == MatchState.WaitUnit)
            {
                ShowUnitCardsPlayedRound(round);
                return;
            }
        }

        private void ShowRoundUpdatesUpgrade(Round round)
        {
            if (round.RivalReady)
            {
                var go = GameObject.Instantiate(upgradeCardGo);
                _showdownView.ShowRivalWaitUpgrade(go);
            }
        }


        private void ShowRoundUpdatesUnit(Round round)
        {
            if (round.RivalReady)
            {
                var go = GameObject.Instantiate(unitCardGo);
                _showdownView.ShowRivalWaitUnit(unitCardGo);
            }
        }

        public void UnitCardSentPlay(Hand hand)
        {
            matchState = MatchState.WaitUnit;
            _showdownView.PlayUnitCard(_unitCardPlayed, PlayerType.Player);
            InstantiateHandCards(hand);
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
                upgradeCard.SetCard(card.UpgradeCardData, null, null, false, null);

                _showdownView.PlayUpgradeCard(upgradeCard, PlayerType.Rival);
            }
            matchState = MatchState.SelectUnit;
            _handView.ShowHandUnits();
        }

        private void ShowUnitCardsPlayedRound(Round round)
        {
            if (round.CardsPlayed.Count(c => c.Player != PlayerPrefs.GetString(PlayerPrefsHelper.UserName)) < playersCount - 1)
                return;
            matchState = MatchState.RoundResultReveal;
            var cards = round.CardsPlayed.Where(cp => cp.Player != PlayerPrefs.GetString(PlayerPrefsHelper.UserName));
            foreach (var card in cards)
            {
                if (card.UnitCardData == null) //some player didnt play yet
                    return;
                var go = GameObject.Instantiate(unitCardGo);
                var unitCard = go.GetComponent<UnitCardView>();
                unitCard.SetCard(card.UnitCardData, (_) => { }, null, false, null);
                _showdownView.PlayUnitCard(unitCard, PlayerType.Rival);
            }
            foreach (var cardView in _showdownView.GetUnitsCardsPlayed())
            {
                foreach (var card in round.CardsPlayed)
                {
                    if (cardView.CardName == card.UnitCardData.cardName)
                        cardView.IncreasePowerAnimation(_upgradesView, card.UnitCardPower, 1f);
                }
            }

            StartCoroutine(StartNewRound(round));
        }

        private IEnumerator StartNewRound(Round round)
        {
            //do some animation stuff
            yield return new WaitForSeconds(3f);

            _showdownView.MoveCards(_upgradesView);

            _gameInfoView.WinRound(round.WinnerPlayers);

            if (_presenter.IsMatchOver())
            {
                //yield return new WaitForSeconds(2f);
                EndGame();
            }
            else
            {
                _presenter.StartNewRound();
            }
        }

        private void EndGame()
        {
            matchState = MatchState.EndGame;
            var result = _gameInfoView.GetWinnerPlayer();
            ClearView();
            _navigator.OpenResultView(result);
        }

        private void ClearView()
        {
            ClearGameObjectData();
            _handView.Clear();
            //_showDrawnHandContainer.Clear();
            _gameInfoView.Clear();
            _showdownView.Clear();
            _upgradesView.Clear();
            PlayerPrefs.SetString(PlayerPrefsHelper.MatchId, string.Empty);
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
            upgradeCard.SetCard(upgradeCardData, null, null, false, null);
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