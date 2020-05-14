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

        //private bool isRerolling = false;
        //private bool hasInitializedRound = false;
        //private bool hasShownRoundUpgrade = false;
        //private bool hasShownUpgrades = false;
        //private bool hasShownUnits = false;
        private bool isWorking = false;

        MatchState matchState = MatchState.InitializeGame;

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
            Toast.Instance.ShowToast("Unexpected","Error");
            RevertLastAction();
            isWorking = false;
            //throw new NotImplementedException("Unexpected");
        }

        private void RevertLastAction()
        {
            
        }

        public void InitializeGame(Match match)
        {
            ChangeMatchState(MatchState.InitializeGame);
            _gameInfoView.SetGame(match);
            _upgradesView.SetGame(match);
            var lastRound = match.Board.Rounds.Last();
            _showdownView.SetRound(lastRound);
            _timerView.WithLowTimer(5f);
            _timerView.StartTimer();
            GetOrInstantiateHandCards(_presenter.GetHand());

            ChangeMatchState(MatchState.StartRound);
            InvokeRepeating("GetRound", 0.5f, 2f);
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
            _timerView.Update(round);
            if (isWorking)
                return;
            if (matchState == MatchState.StartRound)
            {
                StartRound(round);
            }
            
            if (matchState == MatchState.StartRoundUpgradeReveal)
            {
                ShowRoundUpgrade(round);
                return;
            }
            if (matchState == MatchState.StartReroll)
            {
                ShowReroll();
                return;
            }
            if (matchState == MatchState.StartUpgrade)
            {
                if (round.RoundState == RoundState.Upgrade)
                    ChangeMatchState(MatchState.SelectUpgrade);
                GetOrInstantiateHandCards(_presenter.GetHand());
                PutCardsInHand();
                _handView.ShowHandUpgrades();
                return;
            }
            if (matchState == MatchState.StartUnit)
            {
                //GetOrInstantiateHandCards(_presenter.GetHand());
                //PutCardsInHand();
                if (round.RoundState == RoundState.Unit)
                    ChangeMatchState(MatchState.SelectUnit);
                _handView.ShowHandUnits();
                return;
            }
            if (round.RoundState == RoundState.Upgrade)
            {
                ShowUpgradeTurn(round);
                return;
            }

            if (round.RoundState == RoundState.Unit)
            {
                ShowUnitTurn(round);
                return;
            }

            if (round.RoundState == RoundState.Finished || round.RoundState == RoundState.GameFinished)
            {
                ShowRoundEnd(round);
            }
        }

        private void StartRound(Round round)
        {
            isWorking = true;
            GetOrInstantiateHandCards(_presenter.GetHand());
            ChangeMatchState(MatchState.StartRoundUpgradeReveal);
            isWorking = false;
        }
        private void ShowUpgradeTurn(Round round)
        {
            isWorking = true;
            if (matchState == MatchState.Reroll)
            {
                HideReroll();
            };

            if (round.RivalReady)
            {
                _showdownView.ShowRivalWaitUpgrade();
            }

            isWorking = false;
        }

        private void ShowUnitTurn(Round round)
        {
            isWorking = true;
            if (matchState.IsUpgradePhase())
            {
                ChangeMatchState(MatchState.RoundUpgradeReveal);
                ShowUpgradeCardsPlayedRound(round, () =>
                {
                    ChangeMatchState(MatchState.StartUnit);
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
            if (matchState.IsUnitPhase())
            {
                ChangeMatchState(MatchState.RoundResultReveal);
                ShowUnitCardsPlayedRound(round, () =>
                {
                    ChangeMatchState(MatchState.EndRound);
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
            RevertLastAction();
            isWorking = false;
            //_presenter.GetRound();
            Toast.Instance.ShowToast(message, "Error");
            Debug.LogError(message);
        }

        public void UpgradeCardSentPlay()
        {
            MoveUpgradeCardToShowdown();
            isWorking = false;
        }

        private void MoveUpgradeCardToShowdown()
        {
            _showdownView.PlayUpgradeCard(_upgradeCardPlayed, PlayerType.Player);
            GetOrInstantiateHandCards(_presenter.GetHand());
            PutCardsInHand();
        }

        public void UnitCardSentPlay()
        {
            MoveUnitCardToShowdown();
            isWorking = false;
        }

        private void MoveUnitCardToShowdown()
        {
            _showdownView.PlayUnitCard(_unitCardPlayed, PlayerType.Player);
            GetOrInstantiateHandCards(_presenter.GetHand());
            PutCardsInHand();
        }

        private void ShowUpgradeCardsPlayedRound(Round round, Action callbackComplete)
        {
            var rivalCards = round.CardsPlayed.Where(cp => cp.Player != UserName);
            var ownPlayedCard = round.CardsPlayed.FirstOrDefault(cp => cp.Player == UserName);
            var upgradePlayed = _playableCards.FirstOrDefault(c => c.CardName == ownPlayedCard.UpgradeCardData.cardName);
            if (_upgradeCardPlayed == null && upgradePlayed != null) {
                _playableCards.Remove(upgradePlayed);
                _presenter.RemoveCard(upgradePlayed.CardName, true);
                _upgradeCardPlayed = (UpgradeCardView)upgradePlayed;

                MoveUpgradeCardToShowdown();
            }
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

            var ownPlayedCard = round.CardsPlayed.FirstOrDefault(cp => cp.Player == UserName);
            var unitPlayed = _playableCards.FirstOrDefault(c => c.CardName == ownPlayedCard.UnitCardData.cardName);
            if (_unitCardPlayed == null & unitPlayed != null)
            {
                _playableCards.Remove(unitPlayed);
                _presenter.RemoveCard(unitPlayed.CardName, false);
                _unitCardPlayed = (UnitCardView)unitPlayed;
                MoveUnitCardToShowdown();
            }

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
        
        private void ShowRoundUpgrade(Round round)
        {
            ClearGameObjectData();
            isWorking = true;

            var upgradeCard = Instantiator.Instance.CreateUpgradeCardGO(round.UpgradeCardRound);
            StartCoroutine(_upgradesView.SetRoundUpgradeCard(upgradeCard.gameObject, () => 
            {
                ChangeMatchState(round.RoundState == RoundState.Reroll && round.HasReroll ? MatchState.StartReroll : MatchState.StartUpgrade);
                isWorking = false;
            }));
        }

        private void EndRound(Round round)
        {
            isWorking = true;
            _showdownView.MoveCards(_upgradesView);
            _gameInfoView.WinRound(round.WinnerPlayers);

            if (round.RoundState == RoundState.GameFinished)
            {
                EndGame();
                return;
            }
            PrepareNewRound();
            isWorking = false;
        }

        private void PrepareNewRound()
        {
            _presenter.StartNewRound();
            ChangeMatchState(MatchState.StartRound);
        }


        private void EndGame()
        {
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
            isWorking = false;
            _timerView.StopTimer();
            PlayerPrefs.SetString(PlayerPrefsHelper.MatchId, string.Empty);
        }

        private void ClearGameObjectData()
        {
            _unitCardPlayed = null;
            _upgradeCardPlayed = null;
        }

        private void PlayUnitCard(Draggable draggable)
        {
            if (matchState != MatchState.SelectUnit)
                return;
            if (_unitCardPlayed != null)
                return;
            isWorking = true;
            var unitCard = draggable.GetComponent<UnitCardView>();
            _unitCardPlayed = unitCard;
            _playableCards.Remove(unitCard);
            _presenter.PlayUnitCard(unitCard.CardName);
        }

        private void PlayUpgradeCard(Draggable draggable)
        {
            if (matchState != MatchState.SelectUpgrade)
                return;
            if (_upgradeCardPlayed != null)
                return;

            isWorking = true;
            var upgradeCard = draggable.GetComponent<UpgradeCardView>();
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
            isWorking = true;

            var cards = _playableCards.Where(c => c.CardName.ToLower() != "villager");
            _rerollView.WithRerollAction((upgrades, units) => 
            {
                isWorking = true;
                _presenter.SendReroll(upgrades, units);
            });
            _rerollView.PutCards(cards);
            _rerollView.gameObject.SetActive(true);
            ChangeMatchState(MatchState.Reroll);
            isWorking = false;
        }

        private void HideReroll()
        {
            _rerollView.gameObject.SetActive(false);
            ChangeMatchState(MatchState.StartUpgrade);
        }

        private IEnumerator RerollComplete(Hand hand)
        {
            isWorking = true;
            /*IEnumerable<CardView> */
            var cardsBefore = _playableCards.Select(c => c.CardName).ToList();
            GetOrInstantiateHandCards(hand);
            var newCards = _playableCards.Where(c => cardsBefore.All(cn => cn != c.CardName)).ToList();

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
            HideReroll();
            isWorking = false; 
        }

        private void ChangeMatchState(MatchState state)
        {
            matchState = state;
            _timerView.ShowState(state);
        }
    }
}