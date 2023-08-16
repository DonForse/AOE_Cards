using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Features.Common.Utilities;
using Features.Data;
using Features.Game.Scripts.Domain;
using Features.Game.Scripts.Domain.Actions;
using Features.Game.Scripts.Infrastructure;
using Features.Game.Scripts.Presentation;
using Features.Home;
using Features.Infrastructure.Data;
using Features.Match.Domain;
using Features.Sound;
using UniRx;
using UnityEngine;

namespace Features.Game.Scripts.Delivery
{
    public class GameView : MonoBehaviour, IView, IGameView
    {
        public void Log(string m) => Debug.Log($"<color='red'>{m}</color>");
        
        [SerializeField] private Navigator _navigator;
        [SerializeField] private ServicesProvider servicesProvider;
        [SerializeField] private RerollView _rerollView;
        [SerializeField] private UpgradesView _upgradesView;
        [SerializeField] private GameInfoView _gameInfoView;
        [SerializeField] private HandView _handView;
        [SerializeField] private ShowdownView _showdownView;
        [SerializeField] private TimerView _timerView;
        [SerializeField] private ActionView _actionView;
        [SerializeField] private GameObject _focusOutGameObject;
        [SerializeField] private AudioClip[] clips;

        private GamePresenter _presenter;
        private UnitCardView _unitCardPlayed;
        private UpgradeCardView _upgradeCardPlayed;

        private IList<CardView> _playableCards;
        // MatchState matchState = MatchState.InitializeGame;
        private CompositeDisposable _disposables = new CompositeDisposable();
        private readonly ISubject<(List<string> units, List<string> upgrades)> _rerollSubject = new Subject<(List<string> units, List<string> upgrades)>();
        private readonly ISubject<string> _unitCardPlayedSubject = new Subject<string>();
        private readonly ISubject<string> _upgradeCardPlayedSubject = new Subject<string>();
        private readonly ISubject<Unit> _applicationRestoreFocusSubject = new Subject<Unit>();
        private readonly ISubject<Unit> _showRoundUpgradeCompletedSubject = new Subject<Unit>();
        private readonly ISubject<Unit> _unitShowDownCompleteSubject = new Subject<Unit>();
        private readonly ISubject<Unit> _upgradeShowDownCompleteSubject = new Subject<Unit>();
        public IObservable<string> UnitCardPlayed() => _unitCardPlayedSubject;
        public IObservable<string> UpgradeCardPlayed() => _upgradeCardPlayedSubject;
        public IObservable<Unit> ApplicationRestoreFocus() => _applicationRestoreFocusSubject;
        public IObservable<Unit> ShowRoundUpgradeCompleted() => _showRoundUpgradeCompletedSubject;
        public IObservable<Unit> UnitShowDownCompleted() => _unitShowDownCompleteSubject;
        public IObservable<Unit> UpgradeShowDownCompleted() => _upgradeShowDownCompleteSubject;

        private string UserName => PlayerPrefs.GetString(PlayerPrefsHelper.UserName);

        public void OnOpening()
        {
            Clear();
            LoadAudio();
            this.gameObject.SetActive(true);
        }

        private void InitializePresenter(GameMatch gameMatch)
        {
            var repo = new InMemoryCurrentMatchRepository();
            _presenter = new GamePresenter(this, servicesProvider.GetPlayService(), servicesProvider.GetTokenService(),
                servicesProvider.GetMatchService(),
                new GetRoundEvery3Seconds(servicesProvider.GetPlayService(), repo),
                new GetMatchEvery3Seconds(servicesProvider.GetMatchService()), repo,
                new InMemoryMatchStateRepository(),
                new PlayerPrefsWrapper());
            _presenter.Initialize(gameMatch);
        }

        private void LoadAudio()
        {
            if (clips == null || clips.Length == 0)
                clips = Resources.LoadAll<AudioClip>("Sounds/GameBackground");

            var index = UnityEngine.Random.Range(0, clips.Length);
            SoundManager.Instance.PlayBackground(clips[index], new AudioClipOptions { loop = true }, true);
        }

        public void OnClosing()
        {
            _disposables.Clear();
            _presenter.Unload();    
            Clear();
            this.gameObject.SetActive(false);
        }

        public void SetGame(GameMatch gameMatch)
        {
            InitializePresenter(gameMatch);
            InitializeGame(gameMatch);
        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                Debug.Log("Focus");
                _applicationRestoreFocusSubject.OnNext(Unit.Default);
            }
            else {
                Debug.Log("Pause");
                _focusOutGameObject.SetActive(true);
            }
        }
        
        public void UpdateTimer(Round round) => _timerView.Update(round);
        private void InitializeGame(GameMatch gameMatch)
        {
            SetupViews(gameMatch);
            ShowMatchState(GameState.StartRound);
        }

        public void SetupViews(GameMatch gameMatch)
        {
            ShowMatchState(GameState.InitializeGame);
            _gameInfoView.SetGame(gameMatch);
            _upgradesView.WithShowDownView(_showdownView).SetGame(gameMatch);
            _showdownView.SetRound(gameMatch.Board.CurrentRound);
            _timerView.WithLowTimer(5f);
            _timerView.StartTimer();
            
            _upgradesView.OnShowRoundUpgradeCompletes().Subscribe(_ =>
            {
                Debug.Log("ShowRoundUpgrade-Completed");
                // ShowMatchState(round.RoundState == RoundState.Reroll && round.HasReroll
                //     ? GameState.StartReroll
                //     : GameState.StartUpgrade);
                _showRoundUpgradeCompletedSubject.OnNext(Unit.Default);
            });
        }

        private void PutCardsInHand() => _handView.PutCards(_playableCards);

        public IObservable<(List<string> upgrades, List<string> units)> ReRoll() => _rerollSubject;

        public void StartRound(Round round)
        {
            GetOrInstantiateHandCards(_presenter.GetHand());
            ShowMatchState(GameState.StartRoundUpgradeReveal);
        }
        
        private IList<CardView> GetOrInstantiateHandCards(Hand hand)
        {
            var inPlayCards = _playableCards.ToList();
            foreach (var card in hand.GetUnitCards().GroupBy(card => card.cardName))
            {
                var missingCards = card.Count() - inPlayCards.Count(cuc => cuc.name == card.Key);
                while (missingCards > 0)
                {
                    var go = CardInstantiator.Instance.CreateUnitCardGO(card.First());

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
                    var go = CardInstantiator.Instance.CreateUpgradeCardGO(card.First());
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

        public void PlayUpgradeCard(string cardName)
        {
            Debug.Log("UpgradeCardSentPlay");
            _playableCards.Remove(_upgradeCardPlayed);
            MoveUpgradeCardToShowdown();
        }

        public void ShowError(string message)
        {
            Toast.Instance.ShowToast(message, "Error");
            Debug.LogError(message);
        }

        private void MoveUpgradeCardToShowdown()
        {
            _showdownView.PlayUpgradeCard(_upgradeCardPlayed, PlayerType.Player);
            GetOrInstantiateHandCards(_presenter.GetHand());
            PutCardsInHand();
        }

        public void PlayUnitCard(string cardName)
        {
            _playableCards.Remove(_unitCardPlayed);
            MoveUnitCardToShowdown();
        }

        private void MoveUnitCardToShowdown()
        {
            _showdownView.PlayUnitCard(_unitCardPlayed, PlayerType.Player);
            GetOrInstantiateHandCards(_presenter.GetHand());
            PutCardsInHand();
        }

        public void ShowUpgradeCardsPlayedRound(Round round)
        {
            var rivalCards = round.CardsPlayed.Where(cp => cp.Player != UserName);
            var ownPlayedCard = round.CardsPlayed.FirstOrDefault(cp => cp.Player == UserName);
            var upgradePlayed = _playableCards.FirstOrDefault(c => c.CardName == ownPlayedCard.UpgradeCardData.cardName);
            if (_upgradeCardPlayed == null && upgradePlayed != null) {
                _playableCards.Remove(upgradePlayed);
                //TODO: This fixes timeout autoplay
                // _presenter.RemoveCard(upgradePlayed.CardName, true);
                _upgradeCardPlayed = (UpgradeCardView)upgradePlayed;

                MoveUpgradeCardToShowdown();
            }
            foreach (var card in rivalCards)
            {
                if (card.UpgradeCardData == null)
                    return;
                
                var upgradeCard = CardInstantiator.Instance.CreateUpgradeCardGO(card.UpgradeCardData);
                _showdownView.PlayUpgradeCard(upgradeCard, PlayerType.Rival);
            }

            StartCoroutine(_showdownView.RevealCards(() =>
            {
                _handView.ShowHandUnits();
                _upgradeShowDownCompleteSubject.OnNext(Unit.Default);
            }));
        }

        public void ShowUnitCardsPlayedRound(Round round)
        {
            var rivalCards = round.CardsPlayed.Where(cp => cp.Player != UserName);

            var ownPlayedCard = round.CardsPlayed.FirstOrDefault(cp => cp.Player == UserName);
            var unitPlayed = _playableCards.FirstOrDefault(c => c.CardName == ownPlayedCard.UnitCardData.cardName);
            if (_unitCardPlayed == null & unitPlayed != null)
            {
                _playableCards.Remove(unitPlayed);
                //TODO: This fixes timeout autoplay
                //_presenter.RemoveCard(unitPlayed.CardName, false);
                _unitCardPlayed = (UnitCardView)unitPlayed;
                MoveUnitCardToShowdown();
            }

            foreach (var card in rivalCards)
            {
                if (card.UnitCardData == null) //some player didnt play yet
                    return;
                
                var unitCard = CardInstantiator.Instance.CreateUnitCardGO(card.UnitCardData);
                _showdownView.PlayUnitCard(unitCard, PlayerType.Rival);
            }

            StartCoroutine(_showdownView.UnitShowdown(round, () =>
            {
                _unitShowDownCompleteSubject.OnNext(Unit.Default);
            }));
        }

        public void ShowRoundUpgrade(Round round)
        {
            Debug.Log("ShowRoundUpgrade");
            ClearGameObjectData();
            var upgradeCard = CardInstantiator.Instance.CreateUpgradeCardGO(round.UpgradeCardRound);
            _upgradesView.SetRoundUpgradeCard(upgradeCard.gameObject);
        }

        public void EndRound(Round round)
        {
            _showdownView.MoveCards(_upgradesView);
            _gameInfoView.WinRound(round.WinnerPlayers);
            ShowMatchState(GameState.StartRound);
        }

        public void EndGame()
        {
            var result = _gameInfoView.GetWinnerPlayer();
            _navigator.OpenResultView(result);
        }

        public void ClearRound()
        {
            _upgradeCardPlayed = null;
            _unitCardPlayed = null;
        }

        public void Clear()
        {
            if (_playableCards != null)
            {
                foreach (var card in _playableCards)
                {
                    if (card == null)
                        continue;
                    Destroy(card.gameObject);
                }
            }
            _playableCards = new List<CardView>();
            ClearGameObjectData();
            _handView.Clear();
            _gameInfoView.Clear();
            _showdownView.Clear();
            _upgradesView.Clear();
            _rerollView.Clear();
            _rerollView.gameObject.SetActive(false);
            _timerView.StopTimer();
            _focusOutGameObject.SetActive(false);
        }

        private void ClearGameObjectData()
        {
            _unitCardPlayed = null;
            _upgradeCardPlayed = null;
        }

        private void PlayUnitCard(Draggable draggable)
        {
            var unitCard = draggable.GetComponent<UnitCardView>();
            Debug.Log($"PlayUnitCard: {unitCard.CardName}");
            _unitCardPlayed = unitCard;
            _unitCardPlayedSubject.OnNext(unitCard.CardName);
        }

        private void PlayUpgradeCard(Draggable draggable)
        {
            var upgradeCard = draggable.GetComponent<UpgradeCardView>();
            Debug.Log($"PlayUnitCard: {upgradeCard.CardName}");
            _upgradeCardPlayed = upgradeCard;
            _upgradeCardPlayedSubject.OnNext(upgradeCard.CardName);
        }

        public void OnRerollComplete(Hand hand)
        {
            Debug.Log("OnRerollComplete");

            StartCoroutine(RerollComplete(hand));
        }

        public void ShowReroll()
        {
            if (_rerollView.isActiveAndEnabled)
                return;
            Debug.Log("ShowReroll");

            var cards = _playableCards.Where(c => c.CardName.ToLower() != "villager");
            _rerollView.WithRerollAction((upgrades, units) => 
            {
                _rerollSubject.OnNext((upgrades, units));
                // _presenter.SendReroll(upgrades, units);
            });
            _rerollView.PutCards(cards);
            _rerollView.gameObject.SetActive(true);
            ShowMatchState(GameState.SelectReroll);
        }

        public void ShowHand(Hand hand)
        {
            GetOrInstantiateHandCards(hand);
            PutCardsInHand();
        }

        public void ToggleView(HandType handType)
        {
            if (handType == HandType.Upgrade)
                _handView.ShowHandUpgrades();
            else 
                _handView.ShowHandUnits();
        }
        void IGameView.HideReroll() => HideReroll();
        public void ShowRivalWaitUpgrade() => _showdownView.ShowRivalWaitUpgrade();
        public void ShowRivalWaitUnit() => _showdownView.ShowRivalWaitUnit();
        private void HideReroll()
        {
            _rerollView.Clear();
            _rerollView.gameObject.SetActive(false);
            ShowMatchState(GameState.StartUpgrade);
        }

        private IEnumerator RerollComplete(Hand hand)
        {
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
        }
        private void ShowMatchState(GameState state)
        {
            _timerView.ShowState(state);
            _actionView.ShowState(state);
        }
    }
}