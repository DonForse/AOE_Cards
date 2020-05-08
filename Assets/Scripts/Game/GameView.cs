using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
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
        [SerializeField] private GameObject _rerollView;
        [SerializeField] private UpgradesView _upgradesView;
        [SerializeField] private GameInfoView _gameInfoView;
        [SerializeField] private HandView _handView;
        [SerializeField] private ShowdownView _showdownView;
        [SerializeField] private TimerView _timerView;

        private MatchState matchState;
        private GamePresenter _presenter;

        private UnitCardView _unitCardPlayed;
        private UpgradeCardView _upgradeCardPlayed;

        private int playersCount = 0;


        public void OnOpening()
        {
            _presenter = new GamePresenter(this, servicesProvider.GetPlayService(), servicesProvider.GetTokenService());
            this.gameObject.SetActive(true);
        }
        public void OnClosing()
        {
            CancelInvoke("GetRound");
            this.gameObject.SetActive(false);
        }

        public void SetGame(Match match)
        {
            ChangeMatchState(MatchState.InitializeGame);

            _presenter.GameSetup(match);
            playersCount = match.Users.Count();
            _gameInfoView.SetGame(match);
            SetRounds(match);
            InvokeRepeating("GetRound", 3f, 3f);
            _timerView = _timerView
                .WithTimer(Configuration.TurnTimer, Configuration.LowTimer)
                .WithTimerCompleteCallback(AutoSelectFirstCard);
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
                ChangeMatchState(MatchState.SelectUpgrade);
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
                ChangeMatchState(MatchState.SelectUnit);
                return;
            }
            if (playerType == PlayerType.Rival && playerCard == null)
            {
                ChangeMatchState(MatchState.WaitUnit);
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
                ChangeMatchState(MatchState.WaitUpgrade);
            }
            if (playerCard.UpgradeCardData != null)
            {
                var go = Instantiate(upgradeCardGo);
                var upgradeCard = go.GetComponent<UpgradeCardView>();
                upgradeCard.SetCard(playerCard.UpgradeCardData, _ => { }, null, false, null);

                _showdownView.PlayUpgradeCard(upgradeCard, playerType);
            }
        }

        private void UnexpectedError()
        {
            throw new NotImplementedException("Unexpected");
        }

        public void InitializeGame(Match match)
        {
            var lastRound = match.Board.Rounds.Last();
            InstantiateHandCards(match.Hand);
            InitializeRound(lastRound);
            ChangeMatchState(MatchState.InitializeGame);
            _timerView.gameObject.SetActive(true);
            if (lastRound.HasReroll && lastRound.CardsPlayed.All(uc => uc.UpgradeCardData == null))
                ShowReroll();
        }

        public void InitializeRound(Round round)
        {
            ChangeMatchState(MatchState.StartRound);
            ClearGameObjectData();

            ChangeMatchState(MatchState.RoundUpgradeReveal);
            ShowRoundUpgradeCard(round.UpgradeCardRound);

            _handView.ShowHandUpgrades();
            ChangeMatchState(MatchState.SelectUpgrade);
        }

        public void InstantiateHandCards(Hand hand)
        {
            var unitHandCards = _handView.GetUnitCards();
            foreach (var card in hand.GetUnitCards().GroupBy(card => card.cardName))
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
                var missingCards = card.Count() - upgradeHandCards.Count(cuc => cuc.name == card.Key);
                while (missingCards > 0)
                {
                    var go = GameObject.Instantiate(upgradeCardGo);
                    var upgradeCard = go.GetComponent<UpgradeCardView>();
                    upgradeCard.SetCard(card.First(), PlayUpgradeCard, _showdownView.GetComponent<RectTransform>(), true, dragging => { _showdownView.CardDrag(dragging); });
                    _handView.SetUpgradeCard(go);
                    missingCards--;
                }
            }
        }

        private void ShowReroll()
        {
            _timerView.gameObject.SetActive(true);
            var units = _handView.GetUnitCards();
            var upgrades = _handView.GetUpgradeCards();
            _timerView.WithTimer(30, 5).WithTimerCompleteCallback(()=>RerollComplete(units, upgrades));
            

            foreach (var unit in units)
            {
                unit.transform.SetParent(_rerollView.transform);
                unit.transform.localScale = Vector3.one;
                unit.transform.position = _rerollView.transform.position;
            }
            foreach (var upgrade in upgrades)
            {
                upgrade.transform.SetParent(_rerollView.transform);
                upgrade.transform.localScale = Vector3.one;
                upgrade.transform.position = _rerollView.transform.position;
            }
            _rerollView.SetActive(true);
        }

        private void RerollComplete(IList<GameObject> units, IList<GameObject> upgrades)
        {
            foreach (var unit in units)
            {
                _handView.SetUnitCard(unit);
            }
            foreach (var upgrade in upgrades)
            {
                _handView.SetUpgradeCard(upgrade);
            }
            _rerollView.SetActive(false);
            LayoutRebuilder.MarkLayoutForRebuild(_handView.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(_handView.GetComponent<RectTransform>());
            Canvas.ForceUpdateCanvases();

            ChangeMatchState(MatchState.SelectUpgrade);
            _timerView.gameObject.SetActive(true);
        }

        public void ShowError(string message)
        {
            //_presenter.GetRound();
            Debug.LogError(message);
        }

        public void UpgradeCardSentPlay()
        {
            ChangeMatchState(MatchState.WaitUpgrade);
            _showdownView.PlayUpgradeCard(_upgradeCardPlayed, PlayerType.Player);
        }

        public void GetRound()
        {
            if (matchState == MatchState.EndGame)
                return;
            //TODO: BE SUPER CAREFUL TO CHECK ASYNCHRONOUS PROBLEMS.
            //if (!(matchState == MatchState.WaitUpgrade || matchState == MatchState.WaitUnit))
            //    return;
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
            if (matchState == MatchState.EndRound)
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
                _showdownView.ShowRivalWaitUpgrade(upgradeCardGo);
            }
        }


        private void ShowRoundUpdatesUnit(Round round)
        {
            if (round.RivalReady)
            {

                _showdownView.ShowRivalWaitUnit(unitCardGo);
            }
        }

        public void UnitCardSentPlay(Hand hand)
        {
            ChangeMatchState(MatchState.WaitUnit);
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
            StartCoroutine(_showdownView.RevealCards(()=> {
                ChangeMatchState(MatchState.SelectUnit);
                _handView.ShowHandUnits();
            }));

        }

        private void ShowUnitCardsPlayedRound(Round round)
        {
            if (round.CardsPlayed.Count(c => c.Player != PlayerPrefs.GetString(PlayerPrefsHelper.UserName)) < playersCount - 1)
                return;
            ChangeMatchState(MatchState.RoundResultReveal);
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
            StartCoroutine(_showdownView.RevealCards(()=> { StartCoroutine(RoundPresentation(round)); }));
        }

        private IEnumerator RoundPresentation(Round round)
        {
            foreach (var cardView in _showdownView.GetUnitsCardsPlayed())
            {
                var card = round.CardsPlayed.FirstOrDefault(c => c.UnitCardData.cardName == cardView.CardName);
                cardView.IncreasePowerAnimation(_upgradesView, card.UnitCardPower, 1f);
            }
            yield return new WaitForSeconds(2f);
            StartCoroutine(StartNewRound(round));
        }

        private IEnumerator StartNewRound(Round round)
        {
            // //do some animation stuff
            yield return new WaitForSeconds(1f);

            _showdownView.MoveCards(_upgradesView);
            _gameInfoView.WinRound(round.WinnerPlayers);

            if (_presenter.IsMatchOver())
            {
                yield return new WaitForSeconds(2f);
                EndGame();
            }
            else
            {
                ChangeMatchState(MatchState.EndRound);
                _presenter.StartNewRound();
            }
        }

        private void EndGame()
        {
            ChangeMatchState(MatchState.EndGame);
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

        private void ChangeMatchState(MatchState state)
        {
            _timerView.ShowState(state);
            if (state == MatchState.SelectUnit || state == MatchState.SelectUpgrade)
                _timerView.StartCountdown();
            matchState = state;
        }

        private void AutoSelectFirstCard()
        {
            if (matchState == MatchState.SelectUnit)
                PlayUnitCard(_handView.GetUnitCards().FirstOrDefault().GetComponent<Draggable>());
            if (matchState == MatchState.SelectUpgrade)
                PlayUpgradeCard(_handView.GetUpgradeCards().FirstOrDefault().GetComponent<Draggable>());
            //throw new NotImplementedException();
        }
    }
}