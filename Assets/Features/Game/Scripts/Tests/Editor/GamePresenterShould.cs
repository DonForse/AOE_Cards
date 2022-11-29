using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Data;
using Features.Game.Scripts.Domain;
using Features.Game.Scripts.Presentation;
using Features.Match.Domain;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Services;
using Match.Domain;
using NSubstitute;
using NUnit.Framework;
using Token;
using UniRx;

namespace Features.Game.Scripts.Tests.Editor
{
    public class GamePresenterShould
    {
        private GamePresenter _presenter;
        private Match.Domain.GameMatch _gameMatchStatus;
        private ICardProvider _cardProvider;
        private IPlayService _playService;
        private ITokenService _tokenService;
        private IGameView _view;
        private IGetRoundEvery3Seconds _getRoundEvery3Seconds;
        private ICurrentMatchRepository _matchRepository;
        private IMatchService _matchService;
        private IMatchStateRepository _matchStateRepository;
        private IPlayerPrefs _playerPrefs;

        private const int CardsInHand = 5;

        [SetUp]
        public void Setup()
        {
            _cardProvider = Substitute.For<ICardProvider>();
            _playService = Substitute.For<IPlayService>();
            _tokenService = Substitute.For<ITokenService>();
            _matchService = Substitute.For<IMatchService>();
            _view = Substitute.For<IGameView>();
            _getRoundEvery3Seconds = Substitute.For<IGetRoundEvery3Seconds>();
            _matchRepository = Substitute.For<ICurrentMatchRepository>();
            _matchStateRepository = Substitute.For<IMatchStateRepository>();
            _playerPrefs = Substitute.For<IPlayerPrefs>();
            _presenter = new GamePresenter(_view, _playService, _tokenService, _matchService, _getRoundEvery3Seconds,
                _matchRepository, _matchStateRepository, _playerPrefs);
        }

        [Test]
        public void CallGetRoundsEvery3Seconds()
        {
            GivenMatchInRepository(AMatch());
            WhenInitialize(AMatch());
            _getRoundEvery3Seconds.Received(1).Execute();
        }

        [Test]
        public void CallOnRoundOnViewWhenGetRound()
        {
            var expectedRound = new Round();
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchInRepository(AMatch());
            WhenInitialize(AMatch());
            Received.InOrder(() => { _view.Received(1).UpdateTimer(expectedRound); });
        }

        [Test]
        public void CallStartRoundWhenGetRoundInfoAndMatchStateIsStartRound()
        {
            var expectedRound = new Round();
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchStateRepository(GameState.StartRound);
            GivenMatchInRepository(AMatch());
            WhenInitialize(AMatch());
            Received.InOrder(() =>
            {
                _view.Received(1).UpdateTimer(expectedRound);
                _matchStateRepository.Received(1).Set(GameState.StartRoundUpgradeReveal);
                _view.Received(1).StartRound(expectedRound);
            });
        }

        [Test]
        public void CallShowHandWhenGetRoundInfoAndMatchStateIsStartUpgrade()
        {
            var expectedRound = new Round();
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchStateRepository(GameState.StartUpgrade);
            GivenMatchInRepository(AMatch());
            WhenInitialize(AMatch());
            Received.InOrder(() =>
            {
                _view.Received(1).UpdateTimer(expectedRound);
                _view.Received(1).ShowHand(Arg.Any<Hand>());
                _view.Received(1).ToggleView(HandType.Upgrade);
            });
        }

        [Test]
        public void ChangeMatchStateToUpgradeWhenRoundIsInUpgradeInGetRoundInfoAndMatchStateIsStartUpgrade()
        {
            var expectedRound = new Round() {RoundState = RoundState.Upgrade};
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchStateRepository(GameState.StartUpgrade);
            GivenMatchInRepository(AMatch());
            WhenInitialize(AMatch(withRounds: new List<Round>() {expectedRound}));
            Received.InOrder(() =>
            {
                _view.Received(1).UpdateTimer(expectedRound);
                _matchStateRepository.Received(1).Set(GameState.SelectUpgrade);
                _view.Received(1).ShowHand(Arg.Any<Hand>());
                _view.Received(1).ToggleView(HandType.Upgrade);
            });
        }

        [Test]
        public void CallShowHandWhenGetRoundInfoAndMatchStateIsStartUnit()
        {
            var expectedRound = new Round();
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchStateRepository(GameState.StartUnit);
            GivenMatchInRepository(AMatch());
            WhenInitialize(AMatch());
            Received.InOrder(() =>
            {
                _view.Received(1).UpdateTimer(expectedRound);
                _view.Received(1).ShowHand(Arg.Any<Hand>());
                _view.Received(1).ToggleView(HandType.Unit);
            });
        }

        [Test]
        public void ChangeMatchStateToUnitWhenRoundIsInUnitInGetRoundInfoAndMatchStateIsStartUnit()
        {
            var expectedRound = new Round() {RoundState = RoundState.Unit};
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchStateRepository(GameState.StartUnit);
            GivenMatchInRepository(AMatch(withRounds: new List<Round>() {expectedRound}));
            WhenInitialize(AMatch(withRounds: new List<Round>() {expectedRound}));
            Received.InOrder(() =>
            {
                _view.Received(1).UpdateTimer(expectedRound);
                _matchStateRepository.Received(1).Set(GameState.SelectUnit);
                _view.Received(1).ShowHand(Arg.Any<Hand>());
                _view.Received(1).ToggleView(HandType.Unit);
            });
        }

        [TestCase(GameState.SelectReroll)]
        [TestCase(GameState.WaitReroll)]
        public void HideRerollWhenRoundStatusIsUpgradeButMatchStateIs(GameState gameState)
        {
            var expectedRound = new Round() {RoundState = RoundState.Upgrade};
            GivenMatchInRepository(AMatch(withRounds: new List<Round>() {expectedRound}));
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchStateRepository(gameState);
            WhenInitialize(AMatch(withRounds: new List<Round>() {expectedRound}));
            Received.InOrder(() =>
            {
                _matchStateRepository.Received(1).Set(GameState.StartUpgrade);
                _view.Received(1).HideReroll();
            });
        }

        [TestCase(GameState.SelectReroll)]
        [TestCase(GameState.WaitReroll)]
        public void ShowRivalWaitUpgradeWhenRoundStatusIsUpgradeButMatchStateIs(GameState gameState)
        {
            var expectedRound = new Round() {RoundState = RoundState.Upgrade, RivalReady = true};
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchInRepository(AMatch(withRounds: new List<Round>() {expectedRound}));
            GivenMatchStateRepository(gameState);
            WhenInitialize(AMatch(withRounds: new List<Round>() {expectedRound}));
            Received.InOrder(() =>
            {
                _view.Received(1).ShowRivalWaitUpgrade();
                _matchStateRepository.Received(1).Set(GameState.StartUpgrade);
                _view.Received(1).HideReroll();
            });
        }
        
        [Test]
        public void ShowRivalReadyWhenRoundStatusIsUpgradeAndRivalIsReady()
        {
            var expectedRound = new Round() {RoundState = RoundState.Upgrade, RivalReady = true};
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchInRepository(AMatch(withRounds: new List<Round>() {expectedRound}));
            GivenMatchStateRepository(GameState.SelectUpgrade);
            WhenInitialize(AMatch(withRounds: new List<Round>() {expectedRound}));
            ThenShowRivalWaitUpgrade();
        }
        
        [Test]
        public void ShowRivalReadyWhenRoundStatusIsUnitAndMatchStateIsUpgrade()
        {
            var expectedRound = new Round() {RoundState = RoundState.Unit};
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchInRepository(AMatch(withRounds: new List<Round>() {expectedRound}));
            GivenMatchStateRepository(GameState.SelectUpgrade);
            WhenInitialize(AMatch(withRounds: new List<Round>() {expectedRound}));
            ThenShowUpgradeCardsPlayedInCurrentRound(expectedRound);
        }

        [Test]
        public void ShowRivalReadyWhenRoundStatusIsUnitAndRivalIsReady()
        {
            var expectedRound = new Round() {RoundState = RoundState.Unit, RivalReady = true};
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchInRepository(AMatch(withRounds: new List<Round>() {expectedRound}));
            GivenMatchStateRepository(GameState.SelectUnit);
            WhenInitialize(AMatch(withRounds: new List<Round>() {expectedRound}));
            ThenShowRivalWaitUnit();
        }

        [TestCase(RoundState.Finished, GameState.RoundResultReveal)]
        [TestCase(RoundState.GameFinished, GameState.RoundResultReveal)]
        public void ShowEndRoundWhenRoundStateIsFinishedAndMatchStateIsNotUnitPhase(RoundState roundState, GameState gameState)
        {
            var expectedRound = new Round() {RoundState = roundState};
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchInRepository(AMatch(withRounds: new List<Round>() {expectedRound}));
            GivenMatchStateRepository(gameState);
            WhenInitialize(AMatch(withRounds: new List<Round>() {expectedRound}));
            
            _view.Received(1).EndRound(expectedRound);
            
        }
        
        [TestCase(RoundState.Finished, GameState.SelectUnit)]
        [TestCase(RoundState.Finished, GameState.StartUnit)]
        [TestCase(RoundState.Finished, GameState.WaitUnit)]
        [TestCase(RoundState.GameFinished, GameState.SelectUnit)]
        [TestCase(RoundState.GameFinished, GameState.StartUnit)]
        [TestCase(RoundState.GameFinished, GameState.WaitUnit)]
        public void NotShowEndRoundWhenRoundStateIsFinishedAndMatchStateIsUnitPhase(RoundState roundState, GameState gameState)
        {
            var expectedRound = new Round() {RoundState = roundState};
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchInRepository(AMatch(withRounds: new List<Round>() {expectedRound}));
            GivenMatchStateRepository(gameState);
            WhenInitialize(AMatch(withRounds: new List<Round>() {expectedRound}));
            
            _view.DidNotReceive().EndRound(expectedRound);
        }

         
        [TestCase(RoundState.Finished, GameState.SelectUnit)]
        // [TestCase(RoundState.Finished, MatchState.StartUnit)]
        [TestCase(RoundState.Finished, GameState.WaitUnit)]
        [TestCase(RoundState.GameFinished, GameState.SelectUnit)]
        // [TestCase(RoundState.GameFinished, MatchState.StartUnit)]
        [TestCase(RoundState.GameFinished, GameState.WaitUnit)]
        public void ShowUnitCardsPlayedWhenWhenRoundStateIsFinishedAndMatchStateIsUnitPhase(RoundState roundState, GameState gameState)
        {
            var expectedRound = new Round() {RoundState = roundState};
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchInRepository(AMatch(withRounds: new List<Round>() {expectedRound}));
            GivenMatchStateRepository(gameState);
            WhenInitialize(AMatch(withRounds: new List<Round>() {expectedRound}));
            
            Received.InOrder(() =>
            {
                _matchStateRepository.Received(1).Set(GameState.RoundResultReveal);
                _view.Received(1).ShowUnitCardsPlayedRound(expectedRound);
            });
        }
        

        [Test] 
        public void ShowRoundUpgradeWhenMatchStateIsStartRoundUpgradeReveal()
        {
            var expectedRound = new Round() {};
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchInRepository(AMatch(withRounds: new List<Round>() {expectedRound}));
            GivenMatchStateRepository(GameState.StartRoundUpgradeReveal);
            WhenInitialize(AMatch(withRounds: new List<Round>() {expectedRound}));
            Received.InOrder(() =>
            {
                _view.Received(1).UpdateTimer(expectedRound);
                _matchStateRepository.Received(1).Set(GameState.WaitRoundUpgradeReveal);
                _view.ShowRoundUpgrade(expectedRound);
            });
        }
        
        [Test]
        public void ShowStartRoundWhenMatchStateIsStartRound()
        {
            var expectedRound = new Round() {};
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchInRepository(AMatch(withRounds: new List<Round>() {expectedRound}));
            GivenMatchStateRepository(GameState.StartRound);
            WhenInitialize(AMatch(withRounds: new List<Round>() {expectedRound}));
            Received.InOrder(() =>
            {
                _view.Received(1).UpdateTimer(expectedRound);
                _matchStateRepository.Received(1).Set(GameState.StartRoundUpgradeReveal);
                _view.Received(1).StartRound(expectedRound);
            });
        }

        [Test]
        public void ShowRerollWhenGetRoundInfoAndRoundStateIsRerollAndHasReroll()
        {
            var expectedRound = new Round(){RoundState = RoundState.Reroll, HasReroll = true};
            _getRoundEvery3Seconds.Execute().Returns(Observable.Return(expectedRound));
            GivenMatchStateRepository(GameState.StartReroll);
            WhenInitialize(AMatch());
            Received.InOrder(() =>
            {
                _view.Received(1).UpdateTimer(expectedRound);
                _matchStateRepository.Received(1).Set(GameState.SelectReroll);
                _view.Received(1).ShowReroll();
            });
        }

        [Test]
        public void SendReRollWhenReRoll()
        {
            var expectedUnits = new List<string>();
            var expectedUpgrades = new List<string>();

            var reRollSubject = GivenReRoll();
            GivenInitialize(AMatch());

            WhenReRoll(reRollSubject, expectedUpgrades, expectedUnits);
            ThenCalledReRoll();

            void ThenCalledReRoll() => _playService.Received(1).ReRollCards(expectedUnits, expectedUpgrades);
        }

        [Test]
        public void CallViewOnReRollCompleteWhenPlayServiceReturns()
        {
            var expectedUnits = new List<string>();
            var expectedUpgrades = new List<string>();
            var expectedHand = new Hand(new List<UnitCardData>(), new List<UpgradeCardData>());
            var reRollSubject = GivenReRoll();
            GivenInitialize(AMatch());
            GivenReRollCompletes(expectedHand);
            WhenReRoll(reRollSubject, expectedUpgrades, expectedUnits);
            ThenUpdatedHand(expectedHand);
            ThenCalledReRollCompleteOnView();

            void ThenCalledReRollCompleteOnView() => _view.Received(1).OnRerollComplete(expectedHand);
        }

        [Test]
        public void PlayUnitCardWhenCardIsPlayed()
        {
            var expectedCardName = "some card";
            var card = new UnitCardData() {cardName = expectedCardName};
            var match = AMatch(
                withHand: new Hand(new List<UnitCardData> {card}, null));
            ISubject<string> unitCardPlayedSubject = new Subject<string>();
            _view.UnitCardPlayed().Returns(unitCardPlayedSubject);
            GivenInitialize(match);
            GivenMatchInRepository(match);
            GivenMatchStateRepository(GameState.SelectUnit);
            WhenPlayUnit();
            ThenPlayUnitCardIsCalledInService(expectedCardName);

            void WhenPlayUnit() => unitCardPlayedSubject.OnNext(expectedCardName);
        }

        [Test]
        public void NotPlayUnitCardWhenCardIsPlayedAndNotInSelectUnitState()
        {
            var expectedCardName = "some card";
            var card = new UnitCardData() {cardName = expectedCardName};
            var match = AMatch(
                withHand: new Hand(new List<UnitCardData> {card}, null));       
            ISubject<string> unitCardPlayedSubject = new Subject<string>();
            _view.UnitCardPlayed().Returns(unitCardPlayedSubject);
            GivenInitialize(match);
            GivenMatchInRepository(match);
            GivenMatchStateRepository(GameState.SelectUpgrade);
            WhenPlayUnit();
            ThenNotPlayUnitCardIsCalledInService(expectedCardName);

            void WhenPlayUnit() => unitCardPlayedSubject.OnNext(expectedCardName);
        }

        [Test]
        public void PlayUnitCardWhenPlayServiceReturns()
        {
            var expectedCardName = "some card";
            var card = new UnitCardData() {cardName = expectedCardName};
            var match = AMatch(
                withHand: new Hand(new List<UnitCardData> {card}, null));
            
            ISubject<string> unitCardPlayedSubject = new Subject<string>();
            var expectedHand = new Hand(new List<UnitCardData>(), new List<UpgradeCardData>());

            _view.UnitCardPlayed().Returns(unitCardPlayedSubject);
            
            GivenInitialize(match);
            GivenMatchInRepository(match);
            GivenPlayServicePlayUnitCardReturns(expectedCardName, expectedHand);
            GivenMatchStateRepository(GameState.SelectUnit);
            WhenPlayUnitCard();

            Received.InOrder(() =>
            {
                ThenUpdatedHand(expectedHand);
                ThenViewReceivedOnUnitCardPlayed(expectedCardName);
            });

            void WhenPlayUnitCard() => unitCardPlayedSubject.OnNext(expectedCardName);
        }


        [Test]
        public void PlayUpgradeCardWhenCardIsPlayed()
        {
            var expectedCardName = "some card";
            var card = new UpgradeCardData() {cardName = expectedCardName};
            var match = AMatch(
                withHand: new Hand(null,new List<UpgradeCardData> {card}));     
            ISubject<string> upgradeCardPlayedSubject = new Subject<string>();
            _view.UpgradeCardPlayed().Returns(upgradeCardPlayedSubject);
            
            GivenInitialize(match);
            GivenMatchInRepository(match);
            GivenMatchStateRepository(GameState.SelectUpgrade);
            WhenUpgradeCardIsPlayed();
            ThenPlayUpgradeCardIsCalledInService(expectedCardName);

            void WhenUpgradeCardIsPlayed() => upgradeCardPlayedSubject.OnNext(expectedCardName);
        }

        [Test]
        public void PlayUpgradeCardWhenPlayServiceReturns()
        {
            var expectedCardName = "some card";
            var card = new UpgradeCardData() {cardName = expectedCardName};
            var match = AMatch(
                withHand: new Hand(null,new List<UpgradeCardData> {card}));     
            
            ISubject<string> upgradeCardSubject = new Subject<string>();
            var expectedHand = new Hand(new List<UnitCardData>(), new List<UpgradeCardData>());

            _view.UpgradeCardPlayed().Returns(upgradeCardSubject);
            
            GivenInitialize(match);
            GivenMatchInRepository(match);
            GivenMatchStateRepository(GameState.SelectUpgrade);
            GivenPlayServicePlayUpgradeCardReturns(expectedCardName, expectedHand);
            upgradeCardSubject.OnNext(expectedCardName);

            ThenUpdatedHand(expectedHand);
            _view.Received(1).PlayUpgradeCard(expectedCardName);
        }

        [Test]
        public void RemoveUpgradeCardFromHandWhenUpgradeCardIsPlayed()
        {
            var cardName = "some card";
            var upgradeCard = new UpgradeCardData() {cardName = cardName};
            var hand = new Hand(null, new List<UpgradeCardData>()
            {
                new UpgradeCardData(),
                new UpgradeCardData(),
                upgradeCard
            });

            ISubject<string> upgradeCardPlayedSubject = new Subject<string>();
            _view.UpgradeCardPlayed().Returns(upgradeCardPlayedSubject);
            GivenMatchStateRepository(GameState.SelectUpgrade);
            GivenInitialize(AMatch(withHand: hand));
            GivenMatchInRepository(AMatch(withHand: hand));

            upgradeCardPlayedSubject.OnNext(cardName);
            ThenUpgradeCardIsRemovedFromHand(hand, upgradeCard);
        }

        [Test]
        public void RemoveUnitCardFromHandWhenUnitCardIsPlayed()
        {
            var cardName = "some card";
            var unitCard = new UnitCardData() {cardName = cardName};
            var hand = new Hand(new List<UnitCardData>()
            {
                unitCard,
                new UnitCardData(),
                new UnitCardData()
            }, null);

            ISubject<string> unitCardPlayedSubject = new Subject<string>();
            _view.UnitCardPlayed().Returns(unitCardPlayedSubject);
            GivenMatchStateRepository(GameState.SelectUnit);
            GivenInitialize(AMatch(withHand: hand));
            GivenMatchInRepository(AMatch(withHand: hand));

            WhenUnitCardIsPlayed();
            ThenUnitCardIsRemovedFromHand(hand, unitCard);

            void WhenUnitCardIsPlayed() => unitCardPlayedSubject.OnNext(cardName);
        }

        [Test]
        public void WhenShowRoundUpgradeCompletedAndRoundIsInRerollThenChangeMatchStateToStartReroll()
        {
            var expectedRound = new Round() {RoundState = RoundState.Reroll, HasReroll = true};
            var match = AMatch(withRounds: new List<Round> {expectedRound});
            GivenMatchInRepository(match);
            var roundUpgradeCompletedSubject = GivenRoundUpgradeCompletedSubject();
            GivenInitialize(match);
            WhenRoundUpgradeCompletesBeingShown();
            _matchStateRepository.Received(1).Set(GameState.StartReroll);

            void WhenRoundUpgradeCompletesBeingShown() => roundUpgradeCompletedSubject.OnNext(Unit.Default);
        }

        [Test]
        public void WhenShowRoundUpgradeCompletedAndRoundIsNotInRerollThenChangeMatchStateToStartUpgrade()
        {
            var expectedRound = new Round() {RoundState = RoundState.Upgrade, HasReroll = false};
            GivenMatchInRepository(AMatch(withRounds: new List<Round> {expectedRound}));
            var roundUpgradeCompletedSubject = GivenRoundUpgradeCompletedSubject();
            GivenInitialize(AMatch(withRounds: new List<Round> {expectedRound}));
            WhenRoundUpgradeCompletesBeingShown();
            _matchStateRepository.Received(1).Set(GameState.StartUpgrade);
            
            void WhenRoundUpgradeCompletesBeingShown() => roundUpgradeCompletedSubject.OnNext(Unit.Default);

        }

        [Test]
        public void OnRecoverFocusResetGameState()
        {
            var expectedRound = new Round() {RoundState = RoundState.Reroll, HasReroll = true};
            var game = AMatch(withRounds: new List<Round> {expectedRound});
            GivenMatchInRepository(game);
            GivenMatchServiceReturns(game);

            var applicationRestoreFocusSubject = GivenApplicationRestoreFocusSubject();
            GivenInitialize(game);
            WhenRestoreFocus();
            
            Received.InOrder(() =>
            {
                _view.Received(1).Clear();
                _view.Received(1).SetupViews(game);
                _view.Received(1).ShowHand(game.Hand);
            });
            
            void WhenRestoreFocus() => applicationRestoreFocusSubject.OnNext(Unit.Default);
        }

        
        [Test]
        public void OnRecoverFocusStartRerollWhenRoundInReroll()
        {
            var expectedRound = new Round() {RoundState = RoundState.Reroll, HasReroll = true};
            var game = AMatch(withRounds: new List<Round> {expectedRound});
            GivenMatchInRepository(game);
            GivenMatchServiceReturns(game);

            var applicationRestoreFocusSubject = GivenApplicationRestoreFocusSubject();
            GivenInitialize(game);
            WhenRestoreFocus();
            ThenChangeMatchStateTo(GameState.StartReroll);
            
           void WhenRestoreFocus() => applicationRestoreFocusSubject.OnNext(Unit.Default);
        }
        
        [Test]
        public void OnRecoverFocusWaitRerollWhenRoundInRerollAndNoHasReroll()
        {
            var expectedRound = new Round() {RoundState = RoundState.Reroll, HasReroll = false};
            var game = AMatch(withRounds: new List<Round> {expectedRound});
            GivenMatchInRepository(game);
            GivenMatchServiceReturns(game);

            var applicationRestoreFocusSubject = GivenApplicationRestoreFocusSubject();
            GivenInitialize(game);
            WhenRestoreFocus();
            ThenChangeMatchStateTo(GameState.WaitReroll);
            
            void WhenRestoreFocus() => applicationRestoreFocusSubject.OnNext(Unit.Default);
        }

        [Test]
        public void OnRecoverFocusWaitUpgradeWhenRoundInUpgradeAndCardPlayedFromUser()
        {
            var username = "user";
            _playerPrefs.GetString(PlayerPrefsHelper.UserName).Returns(username);
            var expectedRound = new Round() {RoundState = RoundState.Upgrade, CardsPlayed = new List<PlayerCard>(){ new PlayerCard()
            {
                Player = username,
                UpgradeCardData = new UpgradeCardData()
            } }};
            var game = AMatch(withRounds: new List<Round> {expectedRound});
            GivenMatchInRepository(game);
            GivenMatchServiceReturns(game);

            var applicationRestoreFocusSubject = GivenApplicationRestoreFocusSubject();
            GivenInitialize(game);
            WhenRestoreFocus();
            ThenChangeMatchStateTo(GameState.WaitUpgrade);
            
            void WhenRestoreFocus() => applicationRestoreFocusSubject.OnNext(Unit.Default);
        }
        
        [Test]
        public void OnRecoverFocusStartUpgradeWhenRoundInUpgradeAndCardNotPlayedFromUser()
        {
            var username = "user";
            _playerPrefs.GetString(PlayerPrefsHelper.UserName).Returns(username);
            var expectedRound = new Round() {RoundState = RoundState.Upgrade, CardsPlayed = new List<PlayerCard>()};
            var game = AMatch(withRounds: new List<Round> {expectedRound});
            GivenMatchInRepository(game);
            GivenMatchServiceReturns(game);

            var applicationRestoreFocusSubject = GivenApplicationRestoreFocusSubject();
            GivenInitialize(game);
            WhenRestoreFocus();
            ThenChangeMatchStateTo(GameState.StartUpgrade);
            
            void WhenRestoreFocus() => applicationRestoreFocusSubject.OnNext(Unit.Default);
        }
        
        [Test]
        public void OnRecoverFocusWaitUnitWhenRoundInUnitAndCardPlayedFromUser()
        {
            var username = "user";
            _playerPrefs.GetString(PlayerPrefsHelper.UserName).Returns(username);
            var expectedRound = new Round() {RoundState = RoundState.Unit, 
                CardsPlayed = new List<PlayerCard>(){ new PlayerCard()
            {
                Player = username,
                UnitCardData = new UnitCardData()
            }, }};
            var game = AMatch(withRounds: new List<Round> {expectedRound});
            GivenMatchInRepository(game);
            GivenMatchServiceReturns(game);

            var applicationRestoreFocusSubject = GivenApplicationRestoreFocusSubject();
            GivenInitialize(game);
            WhenRestoreFocus();
            ThenChangeMatchStateTo(GameState.WaitUnit);
            
            void WhenRestoreFocus() => applicationRestoreFocusSubject.OnNext(Unit.Default);
        }
        
        [Test]
        public void OnRecoverFocusStartUnitWhenRoundInUnitAndCardNotPlayedFromUser()
        {
            var username = "user";
            _playerPrefs.GetString(PlayerPrefsHelper.UserName).Returns(username);
            var expectedRound = new Round() {RoundState = RoundState.Unit, CardsPlayed = new List<PlayerCard>()};
            var game = AMatch(withRounds: new List<Round> {expectedRound});
            GivenMatchInRepository(game);
            GivenMatchServiceReturns(game);

            var applicationRestoreFocusSubject = GivenApplicationRestoreFocusSubject();
            GivenInitialize(game);
            WhenRestoreFocus();
            ThenChangeMatchStateTo(GameState.StartUnit);
            
            void WhenRestoreFocus() => applicationRestoreFocusSubject.OnNext(Unit.Default);
        }
        
        [Test]
        public void OnRecoverFocusStartRoundWhenRoundFinished()
        {
            var expectedRound = new Round() {RoundState = RoundState.Finished};
            var game = AMatch(withRounds: new List<Round> {expectedRound});
            GivenMatchInRepository(game);
            GivenMatchServiceReturns(game);

            var applicationRestoreFocusSubject = GivenApplicationRestoreFocusSubject();
            GivenInitialize(game);
            _matchStateRepository.ClearReceivedCalls();
            WhenRestoreFocus();
            ThenChangeMatchStateTo(GameState.StartRound);
            
            void WhenRestoreFocus() => applicationRestoreFocusSubject.OnNext(Unit.Default);
        }
        
        [Test]
        public void OnRecoverFocusEndWhenGameFinished()
        {
            var expectedRound = new Round() {RoundState = RoundState.GameFinished};
            var game = AMatch(withRounds: new List<Round> {expectedRound});
            GivenMatchInRepository(game);
            GivenMatchServiceReturns(game);

            var applicationRestoreFocusSubject = GivenApplicationRestoreFocusSubject();
            GivenInitialize(game);
            WhenRestoreFocus();
            ThenEndGame();
            
            void WhenRestoreFocus() => applicationRestoreFocusSubject.OnNext(Unit.Default);
        }
        
        [Test]
        public void ChangeMatchStateWhenUnitsFinishBeingRevealed()
        {
            ISubject<Unit> unitShowdownCompletedSubject = new Subject<Unit>();
            _view.UnitShowDownCompleted().Returns(unitShowdownCompletedSubject);
            GivenInitialize(AMatch());
            _matchStateRepository.ClearReceivedCalls();
            unitShowdownCompletedSubject.OnNext(Unit.Default);
            _matchStateRepository.Received(1).Set(GameState.StartRound);
        }
        
        [Test]
        public void ChangeMatchStateWhenUpgradesFinishBeingRevealed() 
        {
            ISubject<Unit> upgradeShowdownCompletedSubject = new Subject<Unit>();
            _view.UpgradeShowDownCompleted().Returns(upgradeShowdownCompletedSubject);
            GivenInitialize(AMatch());
            upgradeShowdownCompletedSubject.OnNext(Unit.Default);
            _matchStateRepository.Received(1).Set(GameState.StartUnit);
        }


        private GameMatch AMatch(int withUnits = 5,
            int withUpgrades = 5,
            List<Round> withRounds = null,
            string withMatchId = "MatchId",
            string[] withUsers = null,
            Hand withHand = null)
        {
            return new GameMatch
            {
                Hand = withHand ?? new Hand(_cardProvider.GetUnitCards().Take(withUnits).ToList(),
                    _cardProvider.GetUpgradeCards().Take(withUpgrades).ToList()),
                Board = new Board {Rounds = withRounds ?? new List<Round>() {new Round() {RoundNumber = 0}}},
                Id = withMatchId,
                Users = withUsers ?? new[] {"user-1", "user-2"}
            };
        }


        private void GivenMatchServiceReturns(GameMatch match) => _matchService.GetMatch().Returns(Observable.Return(match));

        private Subject<Unit> GivenApplicationRestoreFocusSubject()
        {
            var applicationRestoreFocusSubject = new Subject<Unit>();
            _view.ApplicationRestoreFocus().Returns(applicationRestoreFocusSubject);
            return applicationRestoreFocusSubject;
        }


        private void GivenGetRoundEvery3SecondsReturns(Round expectedRound) => _getRoundEvery3Seconds.Execute().Returns(Observable.Return(expectedRound));

        private Subject<Unit> GivenRoundUpgradeCompletedSubject()
        {
            var showRoundUpgradeCompletedSubject = new Subject<Unit>();
            _view.ShowRoundUpgradeCompleted().Returns(showRoundUpgradeCompletedSubject);
            return showRoundUpgradeCompletedSubject;
        }

        private void GivenMatchStateRepository(GameState gameState) =>
            _matchStateRepository.Get().Returns(gameState);

        private void GivenMatchInRepository(GameMatch gameMatch) =>
            _matchRepository.Get().Returns(gameMatch);

        private void GivenReRollCompletes(Hand hand) =>
            _playService.ReRollCards(Arg.Any<IList<string>>(), Arg.Any<IList<string>>())
                .Returns(Observable.Return(hand));


        private void GivenPlayServicePlayUpgradeCardReturns(string expectedCardName, Hand expectedHand) => _playService.PlayUpgradeCard(expectedCardName).Returns(Observable.Return(expectedHand));
        private void GivenInitialize(GameMatch gameMatch) => WhenInitialize(gameMatch);

        private Subject<(List<string> upgrades, List<string> units)> GivenReRoll()
        {
            var rerollSubject = new Subject<(List<string> upgrades, List<string> units)>();
            _view.ReRoll().Returns(rerollSubject);
            return rerollSubject;
        }
        

        private void GivenPlayServicePlayUnitCardReturns(string expectedCardName, Hand expectedHand) =>
            _playService.PlayUnitCard(expectedCardName).Returns(Observable.Return(expectedHand));

        private static void WhenReRoll(Subject<(List<string> upgrades, List<string> units)> rerollSubject,
            List<string> expectedUpgrades, List<string> expectedUnits) =>
            rerollSubject.OnNext((expectedUpgrades, expectedUnits));
        
        private void WhenInitialize(GameMatch match) => _presenter.Initialize(match);
        private void ThenShowUpgradeCardsPlayedInCurrentRound(Round expectedRound) => _view.Received(1).ShowUpgradeCardsPlayedRound(expectedRound);
        private void ThenUpdatedHand(Hand hand) => _matchRepository.Received(1).Set(hand);

        private void ThenPlayUpgradeCardIsCalledInService(string cardName) =>
            _playService.Received(1).PlayUpgradeCard(cardName);

        private void ThenPlayUnitCardIsCalledInService(string cardName) =>
            _playService.Received(1).PlayUnitCard(cardName);

        private void ThenNotPlayUnitCardIsCalledInService(string cardName) =>
            _playService.DidNotReceive().PlayUnitCard(cardName);

        private void ThenUnitCardIsRemovedFromHand(Hand hand, UnitCardData card) =>
            Assert.IsTrue(!hand.GetUnitCards().ToList().Contains(card));

        private void ThenUpgradeCardIsRemovedFromHand(Hand hand, UpgradeCardData card) =>
            Assert.IsTrue(!hand.GetUpgradeCards().ToList().Contains(card));
        private void ThenShowRivalWaitUnit() => _view.Received(1).ShowRivalWaitUnit();
        private void ThenShowRivalWaitUpgrade() => _view.Received(1).ShowRivalWaitUpgrade();

        private void ThenViewReceivedOnUnitCardPlayed(string expectedCardName) =>
            _view.Received(1).PlayUnitCard(expectedCardName);


        private void ThenChangeMatchStateTo(GameState state) => _matchStateRepository.Received(1).Set(state);
        private void ThenEndGame() => _view.Received(1).EndGame();
    }
}