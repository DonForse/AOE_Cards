using System;
using System.Collections.Generic;
using System.Linq;
using Features.Game.Scripts.Domain;
using Features.Game.Scripts.Presentation.GameStateStrategy;
using Features.Game.Scripts.Presentation.RoundStateStrategy;
using Features.Infrastructure.Data;
using Features.Infrastructure.DTOs;
using Features.Infrastructure.Services;
using Features.Infrastructure.Services.Exceptions;
using Features.Match.Domain;
using Features.Token.Scripts.Domain;
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
        private readonly IGetMatchEvery3Seconds _getMatchEvery3Seconds;

        private readonly ICurrentMatchRepository _matchRepository;
        private readonly IMatchStateRepository _matchStateRepository;
        private CompositeDisposable _disposables = new CompositeDisposable();

        private readonly IList<IGameStateStrategy> _gameStateStrategies;
        private readonly IList<IRoundStateStrategy> _roundStateStrategies;

        public void Unload() => _disposables.Clear();

        public GamePresenter(IGameView view,
            IPlayService playService,
            ITokenService tokenService,
            IMatchService matchService,
            IGetRoundEvery3Seconds getRoundEvery3Seconds,
            IGetMatchEvery3Seconds getMatchEvery3Seconds,
            ICurrentMatchRepository currentMatchRepository,
            IMatchStateRepository matchStateRepository, 
            IPlayerPrefs playerPrefs
        )
        {
            _view = view;
            _playService = playService;
            _tokenService = tokenService;
            _matchService = matchService;
            _getRoundEvery3Seconds = getRoundEvery3Seconds;
            _getMatchEvery3Seconds = getMatchEvery3Seconds;
            _matchRepository = currentMatchRepository;
            _matchStateRepository = matchStateRepository;
            _playerPrefs = playerPrefs;

            _gameStateStrategies = new List<IGameStateStrategy>()
            {
                new StartGameStateStrategy(_view, _matchStateRepository),
                new StartGameUpgradeRevealStateStrategy(_view, _matchStateRepository),
                // new StartRerollStateStrategy(_view, _matchStateRepository),
                new StartUpgradeStateStrategy(_view, _matchStateRepository, _matchRepository),
                new StartUnitStateStrategy(_view, _matchStateRepository, _matchRepository)
            };
            _roundStateStrategies = new List<IRoundStateStrategy>()
            {
                new RerollRoundStateStrategy(_view, _matchStateRepository),
                new UpgradeRoundStateStrategy(_view, _matchStateRepository),
                new UnitRoundStateStrategy(_view, _matchStateRepository),
                new MatchStateChangeStateStrategy(_view, _matchStateRepository),
                new FinishedRoundStateStrategy(_view, _matchStateRepository, _matchRepository),
                new GameFinishedRoundStateStrategy(_view, _matchStateRepository, _matchRepository),

            };
        }

        public void Initialize(GameMatch gameMatch)
        {
            _matchRepository.Set(gameMatch);
            _playerPrefs.SetString(PlayerPrefsHelper.MatchId, gameMatch.Id);
            _playerPrefs.Save();
            _getMatchEvery3Seconds
                .Execute()
                .Do(OnGetMatchInfo)
                .Subscribe()
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
                    ? GameState.StartReroll
                    : GameState.StartUpgrade);
            }).AddTo(_disposables);

            _view.UnitShowDownCompleted().Subscribe(_ => ChangeMatchState(GameState.StartRound)).AddTo(_disposables);
            _view.UpgradeShowDownCompleted().Subscribe(_ =>
            {
                ChangeMatchState(GameState.StartUnit);
                _view.EndRound(_matchRepository.Get().Board.Rounds.Last());
            }).AddTo(_disposables);
            ChangeMatchState(GameState.StartRound);
        }

        private void PlayUpgradeCard(string cardName)
        {
            _view.Log($"Presenter Play Upgrade");

            var matchState = _matchStateRepository.Get();

            if (matchState != GameState.SelectUpgrade)
            {
                _view.Log($"Upgrade: {matchState}");
                return;
            }

            var hand = _matchRepository.Get().Hand;
            var card = hand.TakeUpgradeCard(cardName);
            if (card == null)
                return;
            _playService.PlayUpgradeCard(cardName)
                .DoOnSubscribe(() => ChangeMatchState(GameState.WaitUpgrade))
                .DoOnError(err => HandleError((PlayServiceException) err))
                .Subscribe(newHand => OnUpgradeCardPostComplete(cardName, newHand));
        }

        private void PlayUnitCard(string cardName)
        {
            _view.Log($"Presenter Play Unit");

            var matchState = _matchStateRepository.Get();

            if (matchState != GameState.SelectUnit)
            {
                _view.Log($"Unit: {matchState}");
                return;
            }

            var hand = _matchRepository.Get().Hand;
            var card = hand.TakeUnitCard(cardName);
            if (card == null)
                return;
            _playService.PlayUnitCard(cardName)
                .DoOnSubscribe(() => ChangeMatchState(GameState.WaitUnit))
                .DoOnError(err => HandleError((PlayServiceException) err))
                .Subscribe(newHand => OnUnitCardPostComplete(cardName, newHand));
        }

        private void SendReRoll(IList<string> upgradeCards, IList<string> unitCards)
        {
            _playService.ReRollCards(unitCards, upgradeCards)
                .DoOnError(err => HandleError((PlayServiceException) err))
                .Subscribe(OnRerollComplete);
            ChangeMatchState(GameState.WaitReroll);
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
            _view.PlayUnitCard(cardName);
        }

        private void OnUpgradeCardPostComplete(string cardName, Hand hand)
        {
            _matchRepository.Set(hand);
            _view.PlayUpgradeCard(cardName);
        }

        private string UserName => _playerPrefs.GetString(PlayerPrefsHelper.UserName);

        private void OnGetMatchInfo(GameMatch match)
        {
            var playMatch = _matchRepository.Get();
            _view.Log($"<color=yellow>" +
                      $"Play Round Number: {playMatch.Board.CurrentRound.RoundNumber}."+
                      $"Round Number: {match.Board.CurrentRound.RoundNumber}." +
                      $"Round State: {match.Board.CurrentRound.RoundState}." +
                      $"Has Reroll: {match.Board.CurrentRound.HasReroll}." +
                      $"Finished: {match.Board.CurrentRound.Finished}." +
                      $"Rival Ready:{match.Board.CurrentRound.RivalReady}." +
                      $"{match.Board.CurrentRound.CardsPlayed.Count}</color>");
            if (playMatch.Board.CurrentRound.RoundNumber != match.Board.CurrentRound.RoundNumber)
            {
                //cambio de ronda.
            }
            else
            {
            }

            OnGetRoundInfo(match.Board.CurrentRound);
        }

        //TODO: Change Match state (not priority) so it receives a list of actions that happened,
        //that way the match could be recreated, and its easier to know where is the user
        //also if random seed can be applied (so the orders of shuffles remains) it could be used to save lots of issues
        private void OnGetRoundInfo(Round round)
        {
            _matchRepository.SetRounds(round);
            foreach (var strategy in _gameStateStrategies)
            {
                if (!strategy.IsValid()) continue;
                strategy.Execute(round);
                break;
            }

            foreach (var strategy in _roundStateStrategies)
            {
                if (!strategy.IsValid(round)) continue;
                strategy.Execute(round);
                break;
            }
        }

        private void ResetGameState(GameMatch gameMatch)
        {
            _view.Clear();
            _view.SetupViews(gameMatch);
            _view.ShowHand(_matchRepository.Get().Hand);
            RecoverMatchState(gameMatch);

            var matchState = _matchStateRepository.Get();
            //TODO: deberia ser esto? tiene mas sentido que se refreshee con info del server:
            //gameMatch.Board.Rounds.Last().RoundState == RoundState.Reroll;
            if (matchState == GameState.StartReroll || matchState == GameState.SelectReroll)
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
                        ChangeMatchState(GameState.StartReroll);
                    else
                        ChangeMatchState(GameState.WaitReroll);
                    break;
                case RoundState.Upgrade:
                    if (round.CardsPlayed.FirstOrDefault(c => c.Player == UserName)?.UpgradeCardData != null)
                        ChangeMatchState(GameState.WaitUpgrade);
                    else
                        ChangeMatchState(GameState.StartUpgrade);
                    break;
                case RoundState.Unit:
                    if (round.CardsPlayed.FirstOrDefault(c => c.Player == UserName)?.UnitCardData != null)
                        ChangeMatchState(GameState.WaitUnit);
                    else
                        ChangeMatchState(GameState.StartUnit);
                    break;
                case RoundState.Finished:
                    ChangeMatchState(GameState.StartRound);
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
                case GameState.InitializeGame:
                case GameState.StartRound:
                case GameState.StartRoundUpgradeReveal:
                case GameState.StartReroll:
                case GameState.StartUpgrade:
                case GameState.UpgradeReveal:
                case GameState.StartUnit:
                case GameState.RoundResultReveal:
                case GameState.EndRound:
                case GameState.EndGame:
                    break;
                case GameState.SelectReroll:
                case GameState.WaitReroll:
                    _view.ShowHand(_matchRepository.Get().Hand);
                    _view.ShowReroll();
                    break;
                case GameState.SelectUpgrade:
                case GameState.WaitUpgrade:
                    _view.ShowHand(_matchRepository.Get().Hand);
                    _view.ClearRound();
                    ChangeMatchState(GameState.SelectUpgrade);
                    break;
                case GameState.SelectUnit:
                case GameState.WaitUnit:
                    _view.ShowHand(_matchRepository.Get().Hand);
                    _view.ClearRound();
                    ChangeMatchState(GameState.SelectUnit);
                    break;
                default:
                    break;
            }
        }

        private void ChangeMatchState(GameState state)
        {
            _view.Log($"{_matchStateRepository.Get()}->{state}");
            _matchStateRepository.Set(state);
        }
    }
}