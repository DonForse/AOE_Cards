using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Utilities;
using Data;
using Features.Game.Scripts.Domain;
using Features.Match.Domain;
using Home;
using Infrastructure.Data;
using Sound;
using UniRx;
using UnityEngine;

namespace Game
{
    public class GameView : MonoBehaviour, IView, IGameView
    {
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
        private bool isWorking = false;
        // MatchState matchState = MatchState.InitializeGame;
        private CompositeDisposable _disposables = new CompositeDisposable();
        private readonly ISubject<(List<string> units, List<string> upgrades)> _rerollSubject = new Subject<(List<string> units, List<string> upgrades)>();
        private readonly ISubject<string> _unitCardPlayedSubject = new Subject<string>();
        private readonly ISubject<string> _upgradeCardPlayedSubject = new Subject<string>();
        private readonly ISubject<Unit> _applicationRestoreFocusSubject = new Subject<Unit>();
        public IObservable<string> UnitCardPlayed() => _unitCardPlayedSubject;
        public IObservable<string> UpgradeCardPlayed() => _upgradeCardPlayedSubject;
        public IObservable<Unit> ApplicationRestoreFocus() => _applicationRestoreFocusSubject;


        private string UserName => PlayerPrefs.GetString(PlayerPrefsHelper.UserName);

        public void OnOpening()
        {
            ClearView();
            LoadAudio();
            this.gameObject.SetActive(true);
        }

        private void RegisterToPresenterEvents()
        {
            var repo = new InMemoryCurrentMatchRepository();
            _presenter = new GamePresenter(this, servicesProvider.GetPlayService(), servicesProvider.GetTokenService(),
                servicesProvider.GetMatchService(),
                new GetRoundEvery3Seconds(servicesProvider.GetPlayService(), repo), repo);
            _presenter.Initialize();
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
            ClearView();
            this.gameObject.SetActive(false);
        }

        public void SetGame(GameMatch gameMatch)
        {
            RegisterToPresenterEvents();
            InitializeGame(gameMatch);
        }

        //bool _wait = false;

        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                Debug.Log("Focus");

                _applicationRestoreFocusSubject.OnNext(Unit.Default);
            }
            else {
                Debug.Log("Pause");
                isWorking = true;
                _focusOutGameObject.SetActive(true);
            }
        }
        
       

        public void UpdateTimer(Round round) => _timerView.Update(round);

        private void SomeError(long arg1, string arg2)
        {
            
            Debug.LogError(arg2);
            //throw new NotImplementedException();
        }

        

        private void InitializeGame(GameMatch gameMatch)
        {
            StartGame(gameMatch);

            ShowMatchState(MatchState.StartRound);
        }

        private void StartGame(GameMatch gameMatch)
        {
            _presenter.SetMatch(gameMatch);
            ShowMatchState(MatchState.InitializeGame);
            _gameInfoView.SetGame(gameMatch);
            _upgradesView.WithShowDownView(_showdownView).SetGame(gameMatch);
            _showdownView.SetRound(gameMatch.Board.Rounds.Last());
            _timerView.WithLowTimer(5f);
            _timerView.StartTimer();
            GetOrInstantiateHandCards(_presenter.GetHand());
        }

        private void PutCardsInHand()
        {
            _handView.PutCards(_playableCards);
        }

        public IObservable<(List<string> upgrades, List<string> units)> ReRoll() => _rerollSubject;
        public void OnGetRoundInfo(Round round)
        {
            throw new NotImplementedException();
        }

        public void StartRound(Round round)
        {
            isWorking = true;
            GetOrInstantiateHandCards(_presenter.GetHand());
            ShowMatchState(MatchState.StartRoundUpgradeReveal);
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

        public void OnUpgradeCardPlayed(string cardName)
        {
            Debug.Log("UpgradeCardSentPlay");
            _playableCards.Remove(_upgradeCardPlayed);
            MoveUpgradeCardToShowdown();
            isWorking = false;
        }

        public void ShowError(string message)
        {
            isWorking = false;
            Toast.Instance.ShowToast(message, "Error");
            Debug.LogError(message);
        }

        private void MoveUpgradeCardToShowdown()
        {
            _showdownView.PlayUpgradeCard(_upgradeCardPlayed, PlayerType.Player);
            GetOrInstantiateHandCards(_presenter.GetHand());
            PutCardsInHand();
        }

        public void OnUnitCardPlayed(string cardName)
        {
            _playableCards.Remove(_unitCardPlayed);
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
                callbackComplete?.Invoke();
            }));
        }
        
        public void ShowRoundUpgrade(Round round)
        {
            isWorking = true;
            ClearGameObjectData();
            var upgradeCard = CardInstantiator.Instance.CreateUpgradeCardGO(round.UpgradeCardRound);
            StartCoroutine(_upgradesView.SetRoundUpgradeCard(upgradeCard.gameObject, () => 
            {
                ShowMatchState(round.RoundState == RoundState.Reroll && round.HasReroll ? MatchState.StartReroll : MatchState.StartUpgrade);
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
            ShowMatchState(MatchState.StartRound);
        }


        private void EndGame()
        {
            var result = _gameInfoView.GetWinnerPlayer();
            _navigator.OpenResultView(result);
        }

        private void ClearView()
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
            isWorking = false;
            _timerView.StopTimer();
        }

        private void ClearGameObjectData()
        {
            _unitCardPlayed = null;
            _upgradeCardPlayed = null;
        }

        private void PlayUnitCard(Draggable draggable)
        {
            Debug.Log("Play Unit");

            if (_unitCardPlayed != null)
                return;
            isWorking = true;
            var unitCard = draggable.GetComponent<UnitCardView>();
            _unitCardPlayed = unitCard;
            _unitCardPlayedSubject.OnNext(unitCard.CardName);
        }

        private void PlayUpgradeCard(Draggable draggable)
        {
            Debug.Log("Play Upgrade Card");
//TODO: This logic should go when play card is confirmed
            if (_upgradeCardPlayed != null)
                return;

            isWorking = true;
            var upgradeCard = draggable.GetComponent<UpgradeCardView>();
            _upgradeCardPlayed = upgradeCard;
            _upgradeCardPlayedSubject.OnNext(upgradeCard.CardName);
                
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
                _rerollSubject.OnNext((upgrades, units));
                // _presenter.SendReroll(upgrades, units);
            });
            _rerollView.PutCards(cards);
            _rerollView.gameObject.SetActive(true);
            ShowMatchState(MatchState.Reroll);
            isWorking = false;
        }

        private void HideReroll()
        {
            _rerollView.Clear();
            _rerollView.gameObject.SetActive(false);
            ShowMatchState(MatchState.StartUpgrade);
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

        private void ShowMatchState(MatchState state)
        {
            _timerView.ShowState(state);
            _actionView.ShowState(state);
        }
        
    }
}