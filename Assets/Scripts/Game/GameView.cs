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

        private IList<CardView> _playableCards;

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
            //InitializeRound(lastRound);
            InvokeRepeating("GetRound",0.5f, 3f);
            ChangeMatchState(MatchState.EndRound);
        }

        private void PutCardsInHand()
        {
            _handView.PutCards(_playableCards);
        }

        public void GetRound()
        {
            if (matchState == MatchState.EndGame)
                return;
            _presenter.GetRound();
        }

        public void OnGetRoundInfo(Round round)
        {
            if (matchState == MatchState.EndRound)
            {
                InitializeRound(round);
                return;
            }
            switch (round.RoundState)
            {
                case RoundState.Reroll:
                    if (matchState == MatchState.WaitReroll)
                    {
                        //Wait Opponent
                    }
                        //InitializeRound(round);
                    break;
                case RoundState.Upgrade:
                    if (matchState == MatchState.WaitReroll)
                        InitializeRound(round);
                    if (round.RivalReady)
                        _showdownView.ShowRivalWaitUpgrade();
                    break;
                case RoundState.Unit:
                    if (matchState == MatchState.SelectUpgrade)
                    {
                        var ownCards = round.CardsPlayed.FirstOrDefault(cp => cp.Player == PlayerPrefs.GetString(PlayerPrefsHelper.UserName));
                        var card = _playableCards.FirstOrDefault(c => c.CardName == ownCards.UpgradeCardData.cardName);
                        if (card != null)
                            PlayUpgradeCard(card.GetComponent<Draggable>());
                    }
                    if (round.RivalReady)
                    {
                        if (matchState == MatchState.WaitUpgrade || matchState == MatchState.SelectUpgrade)
                            _showdownView.ShowRivalWaitUpgrade();
                        else
                            _showdownView.ShowRivalWaitUnit();
                    }
                    if (matchState ==MatchState.WaitUpgrade )
                        ShowUpgradeCardsPlayedRound(round);
                    break;
                case RoundState.Finished:
                    if (matchState == MatchState.SelectUnit)
                    {
                        var ownCards = round.CardsPlayed.FirstOrDefault(cp => cp.Player == PlayerPrefs.GetString(PlayerPrefsHelper.UserName));
                        var card = _playableCards.FirstOrDefault(c => c.CardName == ownCards.UnitCardData.cardName);
                        if (card != null)
                            PlayUnitCard(card.GetComponent<Draggable>());
                    }
                    if (matchState == MatchState.WaitUnit || matchState == MatchState.SelectUnit)
                    {
                        ShowUnitCardsPlayedRound(round);
                    }
                    break;
                case RoundState.GameFinished:
                    if (matchState == MatchState.SelectUnit)
                    {
                        var ownCards = round.CardsPlayed.FirstOrDefault(cp => cp.Player == PlayerPrefs.GetString(PlayerPrefsHelper.UserName));
                        var card = _playableCards.FirstOrDefault(c => c.CardName == ownCards.UnitCardData.cardName);
                        if (card != null)
                            PlayUnitCard(card.GetComponent<Draggable>());
                    }

                    if (matchState == MatchState.WaitUnit || matchState == MatchState.SelectUnit)
                        ShowUnitCardsPlayedRound(round);
                    break;
                default:
                    break;
            }
        }

        public void InitializeRound(Round round)
        {
            ChangeMatchState(MatchState.StartRound);
            ClearGameObjectData();

            InstantiateHandCards(_presenter.GetHand());
            if (round.RoundState == RoundState.Reroll && round.HasReroll)
            {
                ChangeMatchState(MatchState.Reroll);
                ShowReroll();
                return;
            }
            
            PutCardsInHand();
            ChangeMatchState(MatchState.RoundUpgradeReveal);
            ShowRoundUpgradeCard(round.UpgradeCardRound);

            _handView.ShowHandUpgrades();
            ChangeMatchState(MatchState.SelectUpgrade);
        }

        public void InstantiateHandCards(Hand hand)
        {
            var inPlayCards = _playableCards.ToList();
            foreach (var card in hand.GetUnitCards().GroupBy(card => card.cardName))
            {
                var missingCards = card.Count() - inPlayCards.Count(cuc => cuc.name == card.Key);
                while (missingCards > 0)
                {
                    var go = Instantiator.Instance.CreateUnitCardGO(card.First());
                    _playableCards.Add(go.GetComponent<CardView>());
                    go.GetComponent<Draggable>()
                          .WithCallback(PlayUnitCard)
                          .WithDragAction(dragging => { _showdownView.CardDrag(dragging); })
                          .WithDropArea(_showdownView.GetComponent<RectTransform>())
                          .enabled = true;
                    missingCards--;
                }
            }
            foreach (var card in hand.GetUpgradeCards().GroupBy(card => card.cardName))
            {
                var missingCards = card.Count() - inPlayCards.Count(cuc => cuc.name == card.Key);
                while (missingCards > 0)
                {
                    var go = Instantiator.Instance.CreateUpgradeCardGO(card.First()); //GameObject.Instantiate(upgradeCardGo);
                    _playableCards.Add(go.GetComponent<CardView>());
                    go.GetComponent<Draggable>()
                       .WithCallback(PlayUpgradeCard)
                       .WithDragAction(dragging => { _showdownView.CardDrag(dragging); })
                       .WithDropArea(_showdownView.GetComponent<RectTransform>())
                       .enabled = true;

                    missingCards--;
                }
            }
        }

        public void OnRerollComplete(Hand hand)
        {
            StartCoroutine(RerollComplete(hand));      
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
            InstantiateHandCards(_presenter.GetHand());
            PutCardsInHand();
        }      

        public void UnitCardSentPlay()
        {
            ChangeMatchState(MatchState.WaitUnit);
            _showdownView.PlayUnitCard(_unitCardPlayed, PlayerType.Player);
            InstantiateHandCards(_presenter.GetHand());
            PutCardsInHand();
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
            ChangeMatchState(MatchState.UpgradeReveal);
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
            _navigator.OpenResultView(result);
        }

        private void ClearView()
        {
            _playableCards = new List<CardView>();
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
            _playableCards.Remove(unitCard);
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
            _playableCards.Remove(upgradeCard);
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
            {
                //PlayUnitCard(_handView.GetUnitCards().FirstOrDefault().GetComponent<Draggable>());
            }
            if (matchState == MatchState.SelectUpgrade)
            {
                //PlayUpgradeCard(_handView.GetUpgradeCards().FirstOrDefault().GetComponent<Draggable>());
            }
            if (matchState == MatchState.Reroll)
            {
                //_rerollView.SendReroll();
            }
            //throw new NotImplementedException();
        }

        private void ShowReroll()
        {

            var cards = _playableCards.Where(c=>c.CardName.ToLower() != "villager");

            _rerollView.WithGamePresenter(_presenter);
            _rerollView.PutCards(cards);

            _timerView
                .WithTimer(30, 5)
                .WithTimerCompleteCallback(OnTimerComplete);
            _timerView.gameObject.SetActive(true);
            _timerView.ResetTimer();

            _rerollView.gameObject.SetActive(true);
        }

        private IEnumerator RerollComplete(Hand hand)
        {
            /*IEnumerable<CardView> */
            var cardsBefore = _playableCards.Select(c => c.CardName);
            InstantiateHandCards(hand);
            var newCards = _playableCards.Where(c => cardsBefore.All(cn => cn != c.CardName));

            var handCards = new List<CardData>();
            handCards.AddRange(hand.GetUnitCards());
            handCards.AddRange(hand.GetUpgradeCards());
            var changedCards = cardsBefore.Where(c =>handCards.All(cn => cn.cardName != c)).ToList();
            foreach (var card in changedCards) {
                var cardToRemove = _playableCards.First(c => c.CardName == card);
                _playableCards.Remove(cardToRemove);
            }

            yield return _rerollView.SwapCards(newCards);
            _timerView.StopTimer();
            _timerView.WithTimer(Configuration.TurnTimer, Configuration.LowTimer);

            _rerollView.gameObject.SetActive(false);
            ChangeMatchState(MatchState.WaitReroll);
            _timerView.gameObject.SetActive(true);
            
        }
    }
}