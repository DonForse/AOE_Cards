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
        [SerializeField] private RerollView _rerollView;
        [SerializeField] private UpgradesView _upgradesView;
        [SerializeField] private GameInfoView _gameInfoView;
        [SerializeField] private HandView _handView;
        [SerializeField] private ShowdownView _showdownView;
        [SerializeField] private TimerView _timerView;

        private MatchState matchState;
        private GamePresenter _presenter;

        private UnitCardView _unitCardPlayed;
        private UpgradeCardView _upgradeCardPlayed;

        public void OnOpening()
        {
            ClearView();
            _presenter = new GamePresenter(this, servicesProvider.GetPlayService(), servicesProvider.GetTokenService());
            this.gameObject.SetActive(true);
        }
        public void OnClosing()
        {
            CancelInvoke("GetRound");
            ClearView();
            this.gameObject.SetActive(false);
        }

        public void SetGame(Match match)
        {
            _presenter.GameSetup(match);
        }

        private void UnexpectedError()
        {
            throw new NotImplementedException("Unexpected");
        }

        public void InitializeGame(Match match)
        {
            ChangeMatchState(MatchState.InitializeGame);
            var lastRound = match.Board.Rounds.Last();

            _gameInfoView.SetGame(match);
            _upgradesView.SetGame(match);
            _showdownView.SetGame(lastRound);
            InstantiateHandCards(match.Hand);

            InitializeRound(lastRound);
            InvokeRepeating("GetRound", 3f, 3f);
        }

        public void GetRound()
        {
            if (matchState == MatchState.EndGame)
                return;
            _presenter.GetRound();
        }

        public void OnGetRoundInfo(Round round)
        {
            if (matchState == MatchState.Reroll || matchState == MatchState.WaitReroll)
            {
                if (round.RoundState != RoundState.Reroll)
                {
                    OnRerollComplete(_presenter.GetHand());
                    InitializeRound(round);
                    //ChangeMatchState(MatchState.SelectUpgrade);
                }
                return;
            }
            if (matchState == MatchState.SelectUpgrade)
            {
                ShowRoundUpdatesUpgrade(round);
                return;
            }
            if (matchState == MatchState.SelectUnit)
            {
                ShowRoundUpdatesUnit(round);
                return;
            }
            if (matchState == MatchState.EndRound)
            {
                InitializeRound(round);
                return;
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

        private void GetGameState(Round lastRound)
        {
            var playedCards = lastRound.CardsPlayed.First(f => f.Player == PlayerPrefs.GetString(PlayerPrefsHelper.UserName));
            switch (lastRound.RoundState)
            {
                case RoundState.Reroll:
                    if (lastRound.HasReroll) ChangeMatchState(MatchState.WaitReroll);
                    else ChangeMatchState(MatchState.Reroll);
                    break;
                case RoundState.Upgrade:
                    if (playedCards.UpgradeCardData == null)
                        ChangeMatchState(MatchState.SelectUpgrade);
                    else
                        ChangeMatchState(MatchState.WaitUpgrade);
                    break;
                case RoundState.Unit:
                    if (playedCards.UnitCardData == null)
                        ChangeMatchState(MatchState.SelectUpgrade);
                    else
                        ChangeMatchState(MatchState.WaitUpgrade);
                    break;
                case RoundState.Finished:
                    ChangeMatchState(MatchState.EndRound);
                    break;
                case RoundState.GameFinished:
                    ChangeMatchState(MatchState.EndGame);
                    break;
                default:
                    ChangeMatchState(MatchState.StartRound);
                    break;
            }
        }

        public void InitializeRound(Round round)
        {
            ChangeMatchState(MatchState.StartRound);
            ClearGameObjectData();
            if (round.RoundState == RoundState.Reroll && round.HasReroll)
            {
                ShowReroll();
                return;
            }

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
                    var go = Instantiator.Instance.CreateUnitCardGO(card.First());
                    go.GetComponent<Draggable>()
                          .WithCallback(PlayUnitCard)
                          .WithDragAction(dragging => { _showdownView.CardDrag(dragging); })
                          .WithDropArea(_showdownView.GetComponent<RectTransform>())
                          .enabled = true;

                    _handView.SetUnitCard(go.gameObject);
                    missingCards--;
                }
            }

            var upgradeHandCards = _handView.GetUpgradeCards();
            foreach (var card in hand.GetUpgradeCards().GroupBy(card => card.cardName))
            {
                var missingCards = card.Count() - upgradeHandCards.Count(cuc => cuc.name == card.Key);
                while (missingCards > 0)
                {
                    var go = Instantiator.Instance.CreateUpgradeCardGO(card.First()); //GameObject.Instantiate(upgradeCardGo);
                    go.GetComponent<Draggable>()
                       .WithCallback(PlayUpgradeCard)
                       .WithDragAction(dragging => { _showdownView.CardDrag(dragging); })
                       .WithDropArea(_showdownView.GetComponent<RectTransform>())
                       .enabled = true;

                    _handView.SetUpgradeCard(go.gameObject);
                    missingCards--;
                }
            }
        }

        public void OnRerollComplete(Hand hand)
        {
            InstantiateHandCards(hand);
            //_rerollView.SwapCards(hand, RerollComplete);
            //some animation
            RerollComplete();
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

        private void ShowRoundUpdatesUpgrade(Round round)
        {
            if (round.RivalReady)
            {
                _showdownView.ShowRivalWaitUpgrade();
            }
        }

        private void ShowRoundUpdatesUnit(Round round)
        {
            if (round.RivalReady)
            {
                _showdownView.ShowRivalWaitUnit();
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
            if (round.RoundState != RoundState.Unit)
                return;
            var rivalCards = round.CardsPlayed.Where(cp => cp.Player != PlayerPrefs.GetString(PlayerPrefsHelper.UserName));

            foreach (var card in rivalCards)
            {
                if (card.UpgradeCardData == null)
                    return;
                var upgradeCard = Instantiator.Instance.CreateUpgradeCardGO(card.UpgradeCardData);
                _showdownView.PlayUpgradeCard(upgradeCard, PlayerType.Rival);
            }
            StartCoroutine(_showdownView.RevealCards(() =>
            {
                ChangeMatchState(MatchState.SelectUnit);
                _handView.ShowHandUnits();
            }));

        }

        private void ShowUnitCardsPlayedRound(Round round)
        {
            if (round.RoundState != RoundState.Finished && round.RoundState != RoundState.GameFinished)
                return;
            ChangeMatchState(MatchState.RoundResultReveal);
            var cards = round.CardsPlayed.Where(cp => cp.Player != PlayerPrefs.GetString(PlayerPrefsHelper.UserName));
            foreach (var card in cards)
            {
                if (card.UnitCardData == null) //some player didnt play yet
                    return;
                var unitCard = Instantiator.Instance.CreateUnitCardGO(card.UnitCardData);
                _showdownView.PlayUnitCard(unitCard, PlayerType.Rival);
            }
            StartCoroutine(_showdownView.RevealCards(() => { StartCoroutine(RoundPresentation(round)); }));
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

            if (round.RoundState == RoundState.GameFinished)
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
            _gameInfoView.Clear();
            _showdownView.Clear();
            _upgradesView.Clear();
            _rerollView.Clear();
            //_timerView.Clear();
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
            var upgradeCard = Instantiator.Instance.CreateUpgradeCardGO(upgradeCardData);
            _upgradesView.SetRoundUpgradeCard(upgradeCard.gameObject);
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
            if (state == MatchState.SelectUnit || state == MatchState.SelectUpgrade || state == MatchState.Reroll)
                _timerView.StartCountdown();
            matchState = state;
        }

        private void OnTimerComplete()
        {
            if (matchState == MatchState.SelectUnit)
                PlayUnitCard(_handView.GetUnitCards().FirstOrDefault().GetComponent<Draggable>());
            if (matchState == MatchState.SelectUpgrade)
                PlayUpgradeCard(_handView.GetUpgradeCards().FirstOrDefault().GetComponent<Draggable>());
            if (matchState == MatchState.Reroll)
                _rerollView.SendReroll();
            //throw new NotImplementedException();
        }

        private void ShowReroll()
        {
            ChangeMatchState(MatchState.Reroll);
            var units = _handView.GetUnitCards();
            var upgrades = _handView.GetUpgradeCards();

            _rerollView.WithGamePresenter(_presenter);
            _rerollView.AddUnitCards(units);
            _rerollView.AddUpgradeCards(upgrades);

            _timerView
                .WithTimer(30, 5)
                .WithTimerCompleteCallback(OnTimerComplete);
            _timerView.gameObject.SetActive(true);
            _timerView.ResetTimer();

            _rerollView.gameObject.SetActive(true);
        }

        private void RerollComplete()
        {
            _timerView.StopTimer();
            _timerView.WithTimer(Configuration.TurnTimer, Configuration.LowTimer);

            _rerollView.gameObject.SetActive(false);
            ChangeMatchState(MatchState.WaitReroll);
            _timerView.gameObject.SetActive(true);
        }
    }
}