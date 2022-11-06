﻿using System.Collections.Generic;
using Features.Game.Scripts.Domain;
using Features.Game.Scripts.Presentation;
using Features.Match.Domain;
using Infrastructure.Data;
using Infrastructure.DTOs;
using Infrastructure.Services;
using Infrastructure.Services.Exceptions;
using Match.Domain;
using Token;
using UniRx;
using UnityEngine;

namespace Game
{
    public class GamePresenter
    {
        private readonly IGameView _view;
        private readonly IPlayService _playService;
        private readonly ITokenService _tokenService;
        private readonly IMatchService _matchService;

        private CompositeDisposable _disposables = new CompositeDisposable();
        private readonly IGetRoundEvery3Seconds _getRoundEvery3Seconds;
        private readonly ICurrentMatchRepository _matchRepository;
        public void Unload() => _disposables.Clear();


        public GamePresenter(IGameView view, 
            IPlayService playService,
            ITokenService tokenService,
            IMatchService matchService,
            IGetRoundEvery3Seconds getRoundEvery3Seconds,
            ICurrentMatchRepository currentMatchRepository
        )
        {
            _view = view;
            _playService = playService;
            _tokenService = tokenService;
            _matchService = matchService;
            _getRoundEvery3Seconds = getRoundEvery3Seconds;
            _matchRepository = currentMatchRepository;
        }

        public void Initialize()
        {
            _getRoundEvery3Seconds.Execute()
                .Subscribe(OnGetRoundComplete)
                .AddTo(_disposables);
            
            _view.ReRoll()
                .Subscribe(rerollInfo => SendReRoll(rerollInfo.upgrades, rerollInfo.units))
                .AddTo(_disposables);

            _view.UnitCardPlayed().Subscribe(PlayUnitCard).AddTo(_disposables);
            _view.UpgradeCardPlayed().Subscribe(PlayUpgradeCard).AddTo(_disposables);
            _view.ApplicationRestoreFocus().Subscribe(_ =>
            {
                _matchService.GetMatch().ObserveOn(Scheduler.MainThread)
                    // .DoOnError(error=>SomeError(((MatchServiceException)error).Code, ((MatchServiceException)error).Message))
                    .Subscribe(ResetGameState)
                    .AddTo(_disposables);
            }).AddTo(_disposables);
        }

        public void SetMatch(GameMatch gameMatch)
        {
            _matchRepository.Set(gameMatch);
            PlayerPrefs.SetString(PlayerPrefsHelper.MatchId, gameMatch.Id);
            PlayerPrefs.Save();
        }

        public void StartNewRound()
        {
            var match = _matchRepository.Get();
            match.Board.Rounds.Add(new Round());
            _matchRepository.Set(match);
        }

        private void PlayUpgradeCard(string cardName)
        {
            if (matchState != MatchState.SelectUpgrade)
                return;
            var hand = _matchRepository.Get().Hand;
            hand.TakeUpgradeCard(cardName);
            _playService.PlayUpgradeCard(cardName)
                .DoOnError(err => HandleError((PlayServiceException)err))
                .Subscribe(newHand=>OnUpgradeCardPostComplete(cardName, newHand));
        }

        private void PlayUnitCard(string cardName)
        {
            if (matchState != MatchState.SelectUnit)
                return;
            
            var hand = _matchRepository.Get().Hand;
            hand.TakeUnitCard(cardName);
            _playService.PlayUnitCard(cardName)
                .DoOnError(err => HandleError((PlayServiceException)err))
                .Subscribe(newHand=>OnUnitCardPostComplete(cardName,newHand));
        }

        private void SendReRoll(IList<string> upgradeCards, IList<string> unitCards)
        {
            _playService.ReRollCards(unitCards, upgradeCards)
                 .DoOnError(err => HandleError((PlayServiceException)err))
                 .Subscribe(OnRerollComplete);
        }

        private void OnGetRoundComplete(Round round)
        {
            OnGetRoundInfo(round);
        }

        private void HandleError(PlayServiceException error)
        {
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
            PlayerPrefs.SetString(PlayerPrefsHelper.UserId, response.guid);
            PlayerPrefs.SetString(PlayerPrefsHelper.UserName, response.username);
            PlayerPrefs.SetString(PlayerPrefsHelper.FriendCode, response.friendCode);
            PlayerPrefs.SetString(PlayerPrefsHelper.AccessToken, response.accessToken);
            PlayerPrefs.SetString(PlayerPrefsHelper.RefreshToken, response.refreshToken);
            PlayerPrefs.Save();
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
        
        
        MatchState matchState = MatchState.InitializeGame;

        //TODO: Change Match state (not priority) so it receives a list of actions that happened,
        //that way the match could be recreated, and its easier to know where is the user
        //also if random seed can be applied (so the orders of shuffles remains) it could be used to save lots of issues
        public void OnGetRoundInfo(Round round)
        {
            _view.UpdateTimer(round);
            //_timerView.Update(round);
            
            if (isWorking) //todo: ignore? i guess is working means its doing some animation or anything, because it will ask again its done so it ignores requests...
                return;
            if (matchState == MatchState.StartRound) //where does match state comes from (I think it initializes on the view and then gets updated puaj
            {
                ChangeMatchState(MatchState.StartRoundUpgradeReveal);
                _view.StartRound(round);
            }
            
            if (matchState == MatchState.StartRoundUpgradeReveal)
            {
                _view.ShowRoundUpgrade(round);
                //en callback de coroutina de la vista
                round.RoundState == RoundState.Reroll && round.HasReroll ? MatchState.StartReroll : MatchState.StartUpgrade
                return;
            }
            if (matchState == MatchState.StartReroll)
            {
                _view.ShowReroll();
                return;
            }
            if (matchState == MatchState.StartUpgrade)
            {
                if (round.RoundState == RoundState.Upgrade)
                    ChangeMatchState(MatchState.SelectUpgrade);
                _view.ShowHand(_matchRepository.Get().Hand);// show hand deberia hacer los 2 siguientes metodos:
                // GetOrInstantiateHandCards(_presenter.GetHand());
                // PutCardsInHand();
                _handView.ShowHandUpgrades(); //toggle view a ver upgrades.
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
        private void ResetGameState(GameMatch gameMatch)
        {
            ClearView();
            StartGame(gameMatch);
            GetOrInstantiateHandCards(gameMatch.Hand);
            RecoverMatchState(gameMatch);
            if (matchState != MatchState.StartReroll && matchState != MatchState.Reroll)
                _handView.PutCards(_playableCards);
            Debug.Log("Reset");
            _focusOutGameObject.SetActive(false);
            
        }
        
        private void RecoverMatchState(GameMatch gameMatch)
        {
            if (gameMatch.Board == null)
                _navigator.OpenHomeView();
            var round = gameMatch.Board.Rounds.Last();
            switch (round.RoundState)
            {
                case RoundState.Reroll:
                    if (round.HasReroll)
                        matchState = MatchState.StartReroll;
                    else
                        matchState = MatchState.WaitReroll;
                    break;
                case RoundState.Upgrade:
                    if (round.CardsPlayed.FirstOrDefault(c => c.Player == UserName).UpgradeCardData != null)
                        matchState = MatchState.WaitUpgrade;
                    else
                        matchState = MatchState.StartUpgrade;
                    break;
                case RoundState.Unit:
                    if (round.CardsPlayed.FirstOrDefault(c => c.Player == UserName).UnitCardData != null)
                        matchState = MatchState.WaitUnit;
                    else
                        matchState = MatchState.StartUnit;
                    break;
                case RoundState.Finished:
                    matchState = MatchState.StartRound;
                    break;
                case RoundState.GameFinished:
                    EndGame();
                    break;
                default:
                    break;
            }
        }
        private void RevertLastAction()
        {
            Debug.Log(string.Format("match state: {0}", matchState));
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
                    GetOrInstantiateHandCards(_presenter.GetHand());
                    PutCardsInHand();
                    ShowReroll();
                    break;
                case MatchState.SelectUpgrade:
                case MatchState.WaitUpgrade:
                    GetOrInstantiateHandCards(_presenter.GetHand());
                    PutCardsInHand();
                    _upgradeCardPlayed = null;
                    ChangeMatchState(MatchState.SelectUpgrade);
                    break;
                case MatchState.SelectUnit:
                case MatchState.WaitUnit:
                    GetOrInstantiateHandCards(_presenter.GetHand());
                    PutCardsInHand();
                    _unitCardPlayed = null;
                    ChangeMatchState(MatchState.SelectUnit);
                    break;
                default:
                    break;
            }
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
                ChangeMatchState(MatchState.UpgradeReveal);
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

        private void ChangeMatchState(MatchState state)
        {
            matchState = state;
        }

    }
}