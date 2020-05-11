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

        private GamePresenter _presenter;

        private UnitCardView _unitCardPlayed;
        private UpgradeCardView _upgradeCardPlayed;

        private IList<CardView> _playableCards;

        private string UserName => PlayerPrefs.GetString(PlayerPrefsHelper.UserName);

        private bool isRerolling = false;
        private bool hasStartedRound = false;
        private bool hasShownUpgrades = false;
        private bool hasShownUnits = false;
        private bool isWorking = false;

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
            _gameInfoView.SetGame(match);
            _upgradesView.SetGame(match);

            var lastRound = match.Board.Rounds.Last();
            _showdownView.SetRound(lastRound);

            GetOrInstantiateHandCards(_presenter.GetHand());

            InvokeRepeating("GetRound", 0.5f, 3f);
        }

        private void GetRound()
        {
            _presenter.GetRound();
        }

        private void PutCardsInHand()
        {
            _handView.PutCards(_playableCards);
        }

        public void OnGetRoundInfo(Round round)
        {
            if (isWorking)
                return;
            if (!hasStartedRound)
            {
                StartRound(round);
                return;
            }

            if (round.RoundState == RoundState.Reroll && !isRerolling)
            {
                ShowReroll();
            }

            if (round.RoundState == RoundState.Upgrade)
            {
                ShowUpgradeTurn(round);
            }

            if (round.RoundState == RoundState.Unit)
            {
                ShowUnitTurn(round);
            }

            if (round.RoundState == RoundState.Finished || round.RoundState == RoundState.GameFinished)
            {
                ShowRoundEnd(round);
            }
        }

        private void ShowUpgradeTurn(Round round)
        {
            isWorking = true;
            if (round.RivalReady)
            {
                _showdownView.ShowRivalWaitUpgrade();
            }

            isWorking = false;
        }

        private void ShowUnitTurn(Round round)
        {
            isWorking = true;
            if (!hasShownUpgrades)
            {
                ShowUpgradeCardsPlayedRound(round, () =>
                {
                    hasShownUpgrades = true;
                    isWorking = false;
                });
                return;
            }

            if (round.RivalReady)
            {
                _showdownView.ShowRivalWaitUnit();
            }

            isWorking = false;
        }
        
        private void ShowRoundEnd(Round round)
        {
            isWorking = true;
            if (!hasShownUnits)
            {
                ShowUnitCardsPlayedRound(round, () =>
                {
                    hasShownUnits = true;
                    isWorking = false;
                });
                return;
            }
            EndRound(round);
            isWorking = false;
        }
        

        private IList<CardView> GetOrInstantiateHandCards(Hand hand)
        {
            var inPlayCards = _playableCards.ToList();
            foreach (var card in hand.GetUnitCards().GroupBy(card => card.cardName))
            {
                var missingCards = card.Count() - inPlayCards.Count(cuc => cuc.name == card.Key);
                while (missingCards > 0)
                {
                    var go = Instantiator.Instance.CreateUnitCardGO(card.First());

                    go.transform.position = Vector3.down * 2000f;
                    go.transform.localScale = Vector3.one;
                    _playableCards.Add(go);
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
                    var go = Instantiator.Instance.CreateUpgradeCardGO(card.First());
                    _playableCards.Add(go);
                    go.transform.position = Vector3.down * 2000f;
                    go.transform.localScale = Vector3.one;
                    go.GetComponent<Draggable>()
                        .WithCallback(PlayUpgradeCard)
                        .WithDragAction(dragging => { _showdownView.CardDrag(dragging); })
                        .WithDropArea(_showdownView.GetComponent<RectTransform>())
                        .enabled = true;

                    missingCards--;
                }
            }

            return _playableCards;
        }

        public void ShowError(string message)
        {
            //_presenter.GetRound();
            Debug.LogError(message);
        }

        public void UpgradeCardSentPlay()
        {
            _showdownView.PlayUpgradeCard(_upgradeCardPlayed, PlayerType.Player);
            GetOrInstantiateHandCards(_presenter.GetHand());
            PutCardsInHand();
        }

        public void UnitCardSentPlay()
        {
            _showdownView.PlayUnitCard(_unitCardPlayed, PlayerType.Player);
            GetOrInstantiateHandCards(_presenter.GetHand());
            PutCardsInHand();
        }

        private void ShowUpgradeCardsPlayedRound(Round round, Action callbackComplete)
        {
            var rivalCards = round.CardsPlayed.Where(cp => cp.Player != UserName);

            foreach (var card in rivalCards)
            {
                if (card.UpgradeCardData == null)
                    return;
                
                var upgradeCard = Instantiator.Instance.CreateUpgradeCardGO(card.UpgradeCardData);
                _showdownView.PlayUpgradeCard(upgradeCard, PlayerType.Rival);
            }

            StartCoroutine(_showdownView.RevealCards(() =>
            {
                _handView.ShowHandUnits();
                callbackComplete?.Invoke();
            }));
        }

        private void ShowUnitCardsPlayedRound(Round round, Action callbackComplete)
        {
            var rivalCards = round.CardsPlayed.Where(cp => cp.Player != UserName);
            
            foreach (var card in rivalCards)
            {
                if (card.UnitCardData == null) //some player didnt play yet
                    return;
                
                var unitCard = Instantiator.Instance.CreateUnitCardGO(card.UnitCardData);
                _showdownView.PlayUnitCard(unitCard, PlayerType.Rival);
            }

            StartCoroutine(_showdownView.UnitShowdown(round, () =>
            {
                callbackComplete?.Invoke();
            }));
        }
        
        private void StartRound(Round round)
        {
            ClearGameObjectData();
            isWorking = true;

            var upgradeCard = Instantiator.Instance.CreateUpgradeCardGO(round.UpgradeCardRound);
            StartCoroutine(_upgradesView.SetRoundUpgradeCard(upgradeCard.gameObject, () => { isWorking = false; }));
        }

        private void EndRound(Round round)
        {
            _showdownView.MoveCards(_upgradesView);
            _gameInfoView.WinRound(round.WinnerPlayers);

            if (round.RoundState == RoundState.GameFinished)
            {
                EndGame();
            }

            return;
            
            PrepareNewRound();
        }

        private void PrepareNewRound()
        {
             bool isRerolling = false;
             bool hasStartedRound = false;
             bool hasShownUpgrades = false;
             bool hasShownUnits = false;
             bool isWorking = false;
             
             _presenter.StartNewRound();
        }

        private void EndGame()
        {
            var result = _gameInfoView.GetWinnerPlayer();
            _navigator.OpenResultView(result);
        }

        private void ClearView()
        {
            isRerolling = false;
            hasStartedRound = false;
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

        private void PlayUnitCard(Draggable draggable)
        {
            var unitCard = draggable.GetComponent<UnitCardView>();
            if (_unitCardPlayed != null)
                return;
            _unitCardPlayed = unitCard;
            _playableCards.Remove(unitCard);
            _presenter.PlayUnitCard(unitCard.CardName);
        }

        private void PlayUpgradeCard(Draggable draggable)
        {
            var upgradeCard = draggable.GetComponent<UpgradeCardView>();
            if (_upgradeCardPlayed != null)
                return;
            _upgradeCardPlayed = upgradeCard;
            _playableCards.Remove(upgradeCard);
            _presenter.PlayUpgradeCard(upgradeCard.CardName);
        }

        public void OnRerollComplete(Hand hand)
        {
            StartCoroutine(RerollComplete(hand));
        }

        private void ShowReroll()
        {
            isRerolling = true;
            var cards = _playableCards.Where(c => c.CardName.ToLower() != "villager");

            _rerollView.WithRerollAction((upgrades, units) => { _presenter.SendReroll(upgrades, units); });
            _rerollView.PutCards(cards);

            _timerView
                .WithTimer(30, 5)
                .WithTimerCompleteCallback(HideReroll);
            _timerView.gameObject.SetActive(true);
            _timerView.ResetTimer();

            _rerollView.gameObject.SetActive(true);
        }

        private void HideReroll()
        {
            _rerollView.gameObject.SetActive(false);
        }

        private IEnumerator RerollComplete(Hand hand)
        {
            /*IEnumerable<CardView> */
            var cardsBefore = _playableCards.Select(c => c.CardName);
            GetOrInstantiateHandCards(hand);
            var newCards = _playableCards.Where(c => cardsBefore.All(cn => cn != c.CardName));

            var handCards = new List<CardData>();
            handCards.AddRange(hand.GetUnitCards());
            handCards.AddRange(hand.GetUpgradeCards());
            var changedCards = cardsBefore.Where(c => handCards.All(cn => cn.cardName != c)).ToList();
            foreach (var card in changedCards)
            {
                var cardToRemove = _playableCards.First(c => c.CardName == card);
                _playableCards.Remove(cardToRemove);
            }

            yield return _rerollView.SwapCards(newCards);
            _timerView.StopTimer();
            _timerView.WithTimer(Configuration.TurnTimer, Configuration.LowTimer);

            _rerollView.gameObject.SetActive(false);
            _timerView.gameObject.SetActive(true);
        }
    }
}