using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Features.Game.Scripts.Domain;
using Features.Game.Scripts.Presentation.RoundStateStrategy;
using Features.Match.Domain;
using Game;
using Infrastructure.Data;
using Infrastructure.DTOs;
using Infrastructure.Services;
using Infrastructure.Services.Exceptions;
using Match.Domain;
using Token;
using UniRx;

namespace Features.Game.Scripts.Presentation
{
    public class GamePresenter
    {
        private readonly IGameView _view;
        private readonly IPlayService _playService;
        private readonly ITokenService _tokenService;
        private readonly IMatchService _matchService;
        private readonly IPlayerPrefs _playerPrefs;

        private readonly IGetRoundEvery3Seconds _getRoundEvery3Seconds;
        private readonly ICurrentMatchRepository _matchRepository;
        private readonly IMatchStateRepository _matchStateRepository;
        private CompositeDisposable _disposables = new CompositeDisposable();

        private readonly IList<IRoundStateStrategy> _roundStateStrategies;

        public void Unload() => _disposables.Clear();

        public GamePresenter(IGameView view,
            IPlayService playService,
            ITokenService tokenService,
            IMatchService matchService,
            IGetRoundEvery3Seconds getRoundEvery3Seconds,
            ICurrentMatchRepository currentMatchRepository,
            IMatchStateRepository matchStateRepository, IPlayerPrefs playerPrefs
        )
        {
            _view = view;
            _playService = playService;
            _tokenService = tokenService;
            _matchService = matchService;
            _getRoundEvery3Seconds = getRoundEvery3Seconds;
            _matchRepository = currentMatchRepository;
            _matchStateRepository = matchStateRepository;
            _playerPrefs = playerPrefs;

            _roundStateStrategies = new List<IRoundStateStrategy>()
            {
                new StartRoundStateStrategy(_view, _matchStateRepository),
                new StartRoundUpgradeRevealStateStrategy(_view, _matchStateRepository),
                new StartRerollStateStrategy(_view, _matchStateRepository),
                new StartUpgradeStateStrategy(_view, _matchStateRepository, _matchRepository),
                new StartUnitStateStrategy(_view, _matchStateRepository, _matchRepository)
            };
        }

        public void Initialize()
        {
            _getRoundEvery3Seconds.Execute()
                .Subscribe(OnGetRoundInfo)
                .AddTo(_disposables);

            _view.ReRoll()
                .Subscribe(rerollInfo => SendReRoll(rerollInfo.upgrades, rerollInfo.units))
                .AddTo(_disposables);

            _view.UnitCardPlayed().Subscribe(PlayUnitCard).AddTo(_disposables);
            _view.UpgradeCardPlayed().Subscribe(PlayUpgradeCard).AddTo(_disposables);
            _view.ApplicationRestoreFocus().Subscribe(_ =>
            {
                _matchService.GetMatch()
                    .Subscribe(ResetGameState)
                    .AddTo(_disposables);
            }).AddTo(_disposables);

            _view.ShowRoundUpgradeCompleted().Subscribe(_ =>
            {
                var round = _matchRepository.Get().Board.Rounds
                    .Last(); //revisar si es lo mismo que round.getround info o quedo desactualizado.
                _view.Log($"{round.RoundState} : {round.HasReroll}");
                //TODO: Why Has Reroll False???
                ChangeMatchState(round.RoundState == RoundState.Reroll && round.HasReroll
                    ? MatchState.StartReroll
                    : MatchState.StartUpgrade);
            }).AddTo(_disposables);
        }

        public void SetMatch(GameMatch gameMatch)
        {
            _matchRepository.Set(gameMatch);
            _playerPrefs.SetString(PlayerPrefsHelper.MatchId, gameMatch.Id);
            _playerPrefs.Save();
            ChangeMatchState(MatchState.StartRound);
        }

        public void StartNewRound()
        {
            ChangeMatchState(MatchState.StartRound);
            var match = _matchRepository.Get();
            match.Board.Rounds.Add(new Round());
            _matchRepository.Set(match);
        }

        private void PlayUpgradeCard(string cardName)
        {
            _view.Log($"Presenter Play Unit");

            var matchState = _matchStateRepository.Get();

            if (matchState != MatchState.SelectUpgrade)
            {
                _view.Log($"Upgrade: {matchState}");
                return;
            }

            var hand = _matchRepository.Get().Hand;
            var card = hand.TakeUpgradeCard(cardName);
            if (card == null)
                return;
            _playService.PlayUpgradeCard(cardName)
                .DoOnSubscribe(() => ChangeMatchState(MatchState.WaitUpgrade))
                .DoOnError(err => HandleError((PlayServiceException) err))
                .Subscribe(newHand => OnUpgradeCardPostComplete(cardName, newHand));
        }

        private void PlayUnitCard(string cardName)
        {
            _view.Log($"Presenter Play Unit");

            var matchState = _matchStateRepository.Get();

            if (matchState != MatchState.SelectUnit)
            {
                _view.Log($"Unit: {matchState}");
                return;
            }

            var hand = _matchRepository.Get().Hand;
            var card = hand.TakeUnitCard(cardName);
            if (card == null)
                return;
            _playService.PlayUnitCard(cardName)
                .DoOnSubscribe(() => ChangeMatchState(MatchState.WaitUnit))
                .DoOnError(err => HandleError((PlayServiceException) err))
                .Subscribe(newHand => OnUnitCardPostComplete(cardName, newHand));
        }

        private void SendReRoll(IList<string> upgradeCards, IList<string> unitCards)
        {
            _playService.ReRollCards(unitCards, upgradeCards)
                .DoOnError(err => HandleError((PlayServiceException) err))
                .Subscribe(OnRerollComplete);
            ChangeMatchState(MatchState.WaitReroll);
        }

        private void HandleError(PlayServiceException error)
        {
            _view.Log("ERROR!!!");
            if (error.Code == 401)
            {
                _tokenService.RefreshToken()
                    .DoOnError(err => OnRefreshTokenError(err.Message))
                    .Subscribe(OnRefreshTokenComplete);
                return;
            }

            RevertLastAction();

            _view.ShowError(error.Message);
        }

        private void OnRefreshTokenError(string error)
        {
            GameManager.SessionExpired();
        }

        private void OnRefreshTokenComplete(UserResponseDto response)
        {
            _playerPrefs.SetString(PlayerPrefsHelper.UserId, response.guid);
            _playerPrefs.SetString(PlayerPrefsHelper.UserName, response.username);
            _playerPrefs.SetString(PlayerPrefsHelper.FriendCode, response.friendCode);
            _playerPrefs.SetString(PlayerPrefsHelper.AccessToken, response.accessToken);
            _playerPrefs.SetString(PlayerPrefsHelper.RefreshToken, response.refreshToken);
            _playerPrefs.Save();
        }

        internal Hand GetHand() => _matchRepository.Get().Hand;

        private void OnRerollComplete(Hand hand)
        {
            _matchRepository.Set(hand);
            _view.OnRerollComplete(hand);
        }

        private void OnUnitCardPostComplete(string cardName, Hand hand)
        {
            _matchRepository.Set(hand);
            _view.OnUnitCardPlayed(cardName);
        }

        private void OnUpgradeCardPostComplete(string cardName, Hand hand)
        {
            _matchRepository.Set(hand);
            _view.OnUpgradeCardPlayed(cardName);
        }

        private string UserName => _playerPrefs.GetString(PlayerPrefsHelper.UserName);


        //TODO: Change Match state (not priority) so it receives a list of actions that happened,
        //that way the match could be recreated, and its easier to know where is the user
        //also if random seed can be applied (so the orders of shuffles remains) it could be used to save lots of issues
        private void OnGetRoundInfo(Round round)
        {
            var matchState = _matchStateRepository.Get();
            _view.UpdateTimer(round);

            // if (isWorking) //todo: ignore? i guess is working means its doing some animation or anything, because it will ask again its done so it ignores requests...
            //     return;
            foreach (var strategy in _roundStateStrategies)
            {
                if (!strategy.IsValid()) continue;
                strategy.Execute(round);
                break;
            }

            if (round.RoundState == RoundState.Upgrade)
            {
                if (matchState == MatchState.Reroll || matchState == MatchState.WaitReroll)
                {
                    _view.HideReroll();
                    ChangeMatchState(MatchState.StartUpgrade);
                }

                if (round.RivalReady)
                {
                    _view.ShowRivalWaitUpgrade();
                }

                return;
            }

            if (round.RoundState == RoundState.Unit)
            {
                if (matchState.IsUpgradePhase())
                {
                    ChangeMatchState(MatchState.UpgradeReveal);
                    //en callback de coroutina de la vista
                    _view.ShowUpgradeCardsPlayedRound(round, () =>
                    {
                        ChangeMatchState(MatchState.StartUnit);
                        // isWorking = false;
                    });
                    return;
                }

                if (round.RivalReady)
                {
                    _view.ShowRivalWaitUnit();
                }

                return;
            }

            if (round.RoundState == RoundState.Finished || round.RoundState == RoundState.GameFinished)
            {
                if (matchState.IsUnitPhase())
                {
                    ChangeMatchState(MatchState.RoundResultReveal);
                    _view.ShowUnitCardsPlayedRound(round, () => { ChangeMatchState(MatchState.StartRound); });
                    return;
                }

                _view.EndRound(round);
                if (round.RoundState == RoundState.GameFinished)
                {
                    _view.EndGame();
                }
                else
                    StartNewRound();
            }
        }

        private void ResetGameState(GameMatch gameMatch)
        {
            _view.Clear();
            _view.StartGame(gameMatch);
            _view.ShowHand(_matchRepository.Get().Hand);
            RecoverMatchState(gameMatch);

            var matchState = _matchStateRepository.Get();
            if (matchState == MatchState.StartReroll || matchState == MatchState.Reroll)
                _view.ShowReroll();
            _view.Log("Reset");
        }

        private void RecoverMatchState(GameMatch gameMatch)
        {
            if (gameMatch.Board == null)
                throw new ApplicationException("Match already finished");
            var round = gameMatch.Board.Rounds.Last();
            switch (round.RoundState)
            {
                case RoundState.Reroll:
                    if (round.HasReroll)
                        ChangeMatchState(MatchState.StartReroll);
                    else
                        ChangeMatchState(MatchState.WaitReroll);
                    break;
                case RoundState.Upgrade:
                    if (round.CardsPlayed.FirstOrDefault(c => c.Player == UserName)?.UpgradeCardData != null)
                        ChangeMatchState(MatchState.WaitUpgrade);
                    else
                        ChangeMatchState(MatchState.StartUpgrade);
                    break;
                case RoundState.Unit:
                    if (round.CardsPlayed.FirstOrDefault(c => c.Player == UserName)?.UnitCardData != null)
                        ChangeMatchState(MatchState.WaitUnit);
                    else
                        ChangeMatchState(MatchState.StartUnit);
                    break;
                case RoundState.Finished:
                    ChangeMatchState(MatchState.StartRound);
                    break;
                case RoundState.GameFinished:
                    _view.EndGame();
                    break;
                default:
                    break;
            }
        }

        private void RevertLastAction()
        {
            var matchState = _matchStateRepository.Get();

            _view.Log($"match state: {matchState}");
            switch (matchState)
            {
                case MatchState.InitializeGame:
                case MatchState.StartRound:
                case MatchState.StartRoundUpgradeReveal:
                case MatchState.RoundUpgradeReveal:
                case MatchState.StartReroll:
                case MatchState.StartUpgrade:
                case MatchState.UpgradeReveal:
                case MatchState.StartUnit:
                case MatchState.RoundResultReveal:
                case MatchState.EndRound:
                case MatchState.EndGame:
                    break;
                case MatchState.Reroll:
                case MatchState.WaitReroll:
                    _view.ShowHand(_matchRepository.Get().Hand);
                    _view.ShowReroll();
                    break;
                case MatchState.SelectUpgrade:
                case MatchState.WaitUpgrade:
                    _view.ShowHand(_matchRepository.Get().Hand);
                    _view.ClearRound();
                    ChangeMatchState(MatchState.SelectUpgrade);
                    break;
                case MatchState.SelectUnit:
                case MatchState.WaitUnit:
                    _view.ShowHand(_matchRepository.Get().Hand);
                    _view.ClearRound();
                    ChangeMatchState(MatchState.SelectUnit);
                    break;
                default:
                    break;
            }
        }

        private void ChangeMatchState(MatchState state)
        {
            _view.Log($"{_matchStateRepository.Get()}->{state}");
            _matchStateRepository.Set(state);
        }
    }
}