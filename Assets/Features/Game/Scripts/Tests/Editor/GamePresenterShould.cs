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
            GivenMatchSetupWith(AMatch());
            WhenInitialize();
            _getRoundEvery3Seconds.Received(1).Execute();
        }

        [Test]
        public void CallOnRoundOnViewWhenGetRound()
        {
            var expectedRound = new Round();
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchSetupWith(AMatch());
            WhenInitialize();
            Received.InOrder(() => { _view.Received(1).UpdateTimer(expectedRound); });
        }

        [Test]
        public void CallStartRoundWhenGetRoundInfoAndMatchStateIsStartRound()
        {
            var expectedRound = new Round();
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchStateRepository(MatchState.StartRound);
            GivenMatchSetupWith(AMatch());
            WhenInitialize();
            Received.InOrder(() =>
            {
                _view.Received(1).UpdateTimer(expectedRound);
                _matchStateRepository.Received(1).Set(MatchState.StartRoundUpgradeReveal);
                _view.Received(1).StartRound(expectedRound);
            });
        }

        [Test]
        public void CallShowHandWhenGetRoundInfoAndMatchStateIsStartUpgrade()
        {
            var expectedRound = new Round();
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchSetupWith(AMatch());
            GivenMatchStateRepository(MatchState.StartUpgrade);
            WhenInitialize();
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
            GivenMatchSetupWith(AMatch(withRounds: new List<Round>() {expectedRound}));
            GivenMatchStateRepository(MatchState.StartUpgrade);
            WhenInitialize();
            Received.InOrder(() =>
            {
                _view.Received(1).UpdateTimer(expectedRound);
                _matchStateRepository.Received(1).Set(MatchState.SelectUpgrade);
                _view.Received(1).ShowHand(Arg.Any<Hand>());
                _view.Received(1).ToggleView(HandType.Upgrade);
            });
        }

        [Test]
        public void CallShowHandWhenGetRoundInfoAndMatchStateIsStartUnit()
        {
            var expectedRound = new Round();
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchSetupWith(AMatch());
            GivenMatchStateRepository(MatchState.StartUnit);
            WhenInitialize();
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
            GivenMatchSetupWith(AMatch(withRounds: new List<Round>() {expectedRound}));
            GivenMatchStateRepository(MatchState.StartUnit);
            WhenInitialize();
            Received.InOrder(() =>
            {
                _view.Received(1).UpdateTimer(expectedRound);
                _matchStateRepository.Received(1).Set(MatchState.SelectUnit);
                _view.Received(1).ShowHand(Arg.Any<Hand>());
                _view.Received(1).ToggleView(HandType.Unit);
            });
        }

        [TestCase(MatchState.SelectReroll)]
        [TestCase(MatchState.WaitReroll)]
        public void HideRerollWhenRoundStatusIsUpgradeButMatchStateIs(MatchState matchState)
        {
            var expectedRound = new Round() {RoundState = RoundState.Upgrade};
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchInRepository(AMatch(withRounds: new List<Round>() {expectedRound}));
            GivenMatchStateRepository(matchState);
            WhenInitialize();
            Received.InOrder(() =>
            {
                _view.Received(1).HideReroll();
                _matchStateRepository.Received(1).Set(MatchState.StartUpgrade);
            });
        }

        [TestCase(MatchState.SelectReroll)]
        [TestCase(MatchState.WaitReroll)]
        public void ShowShowRivalWaitUpgradeWhenRoundStatusIsUpgradeButMatchStateIs(MatchState matchState)
        {
            var expectedRound = new Round() {RoundState = RoundState.Upgrade, RivalReady = true};
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchInRepository(AMatch(withRounds: new List<Round>() {expectedRound}));
            GivenMatchStateRepository(matchState);
            WhenInitialize();
            Received.InOrder(() =>
            {
                _view.Received(1).HideReroll();
                _matchStateRepository.Received(1).Set(MatchState.StartUpgrade);
                _view.Received(1).ShowRivalWaitUpgrade();
            });
        }
        
        [Test]
        public void ShowRivalReadyWhenRoundStatusIsUpgradeAndRivalIsReady()
        {
            var expectedRound = new Round() {RoundState = RoundState.Upgrade, RivalReady = true};
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchInRepository(AMatch(withRounds: new List<Round>() {expectedRound}));
            GivenMatchStateRepository(MatchState.SelectUpgrade);
            WhenInitialize();
            ThenShowRivalWaitUpgrade();
        }
        
        [Test]
        public void ShowRivalReadyWhenRoundStatusIsUnitAndMatchStateIsUpgrade()
        {
            var expectedRound = new Round() {RoundState = RoundState.Unit};
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchInRepository(AMatch(withRounds: new List<Round>() {expectedRound}));
            GivenMatchStateRepository(MatchState.SelectUpgrade);
            WhenInitialize();
            ThenShowUpgradeCardsPlayedInCurrentRound(expectedRound);
        }

        [Test]
        public void ShowRivalReadyWhenRoundStatusIsUnitAndRivalIsReady()
        {
            var expectedRound = new Round() {RoundState = RoundState.Unit, RivalReady = true};
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchInRepository(AMatch(withRounds: new List<Round>() {expectedRound}));
            GivenMatchStateRepository(MatchState.SelectUnit);
            WhenInitialize();
            ThenShowRivalWaitUnit();
        }

        [TestCase(RoundState.Finished, MatchState.RoundResultReveal)]
        [TestCase(RoundState.GameFinished, MatchState.RoundResultReveal)]
        public void ShowEndRoundWhenRoundStateIsFinishedAndMatchStateIsNotUnitPhase(RoundState roundState, MatchState matchState)
        {
            var expectedRound = new Round() {RoundState = roundState};
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchInRepository(AMatch(withRounds: new List<Round>() {expectedRound}));
            GivenMatchStateRepository(matchState);
            WhenInitialize();
            
            _view.Received(1).EndRound(expectedRound);
            
        }
        
        [TestCase(RoundState.Finished, MatchState.SelectUnit)]
        [TestCase(RoundState.Finished, MatchState.StartUnit)]
        [TestCase(RoundState.Finished, MatchState.WaitUnit)]
        [TestCase(RoundState.GameFinished, MatchState.SelectUnit)]
        [TestCase(RoundState.GameFinished, MatchState.StartUnit)]
        [TestCase(RoundState.GameFinished, MatchState.WaitUnit)]
        public void NotShowEndRoundWhenRoundStateIsFinishedAndMatchStateIsUnitPhase(RoundState roundState, MatchState matchState)
        {
            var expectedRound = new Round() {RoundState = roundState};
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchInRepository(AMatch(withRounds: new List<Round>() {expectedRound}));
            GivenMatchStateRepository(matchState);
            WhenInitialize();
            
            _view.DidNotReceive().EndRound(expectedRound);
        }

         
        [TestCase(RoundState.Finished, MatchState.SelectUnit)]
        // [TestCase(RoundState.Finished, MatchState.StartUnit)]
        [TestCase(RoundState.Finished, MatchState.WaitUnit)]
        [TestCase(RoundState.GameFinished, MatchState.SelectUnit)]
        // [TestCase(RoundState.GameFinished, MatchState.StartUnit)]
        [TestCase(RoundState.GameFinished, MatchState.WaitUnit)]
        public void ShowUnitCardsPlayedWhenWhenRoundStateIsFinishedAndMatchStateIsUnitPhase(RoundState roundState, MatchState matchState)
        {
            var expectedRound = new Round() {RoundState = roundState};
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchInRepository(AMatch(withRounds: new List<Round>() {expectedRound}));
            GivenMatchStateRepository(matchState);
            WhenInitialize();
            
            Received.InOrder(() =>
            {
                _matchStateRepository.Received(1).Set(MatchState.RoundResultReveal);
                _view.Received(1).ShowUnitCardsPlayedRound(expectedRound, Arg.Any<Action>());
            });
        }
        

        [Test] 
        public void ShowRoundUpgradeWhenMatchStateIsStartRoundUpgradeReveal()
        {
            var expectedRound = new Round() {};
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchInRepository(AMatch(withRounds: new List<Round>() {expectedRound}));
            GivenMatchStateRepository(MatchState.StartRoundUpgradeReveal);
            WhenInitialize();
            Received.InOrder(() =>
            {
                _view.Received(1).UpdateTimer(expectedRound);
                _matchStateRepository.Received(1).Set(MatchState.WaitRoundUpgradeReveal);
                _view.ShowRoundUpgrade(expectedRound);
            });
        }
        
        [Test]
        public void ShowStartRoundWhenMatchStateIsStartRound()
        {
            var expectedRound = new Round() {};
            GivenGetRoundEvery3SecondsReturns(expectedRound);
            GivenMatchInRepository(AMatch(withRounds: new List<Round>() {expectedRound}));
            GivenMatchStateRepository(MatchState.StartRound);
            WhenInitialize();
            Received.InOrder(() =>
            {
                _view.Received(1).UpdateTimer(expectedRound);
                _matchStateRepository.Received(1).Set(MatchState.StartRoundUpgradeReveal);
                _view.Received(1).StartRound(expectedRound);
            });
        }

        [Test]
        public void ShowRerollWhenGetRoundInfoAndMatchStateIsStartReroll()
        {
            var expectedRound = new Round();
            _getRoundEvery3Seconds.Execute().Returns(Observable.Return(expectedRound));
            GivenMatchStateRepository(MatchState.StartReroll);
            GivenMatchSetupWith(AMatch());
            WhenInitialize();
            Received.InOrder(() =>
            {
                _view.Received(1).UpdateTimer(expectedRound);
                _matchStateRepository.Received(1).Set(MatchState.SelectReroll);
                _view.Received(1).ShowReroll();
            });
        }

        [Test]
        public void SendReRollWhenReRoll()
        {
            var expectedUnits = new List<string>();
            var expectedUpgrades = new List<string>();

            var reRollSubject = GivenReRoll();
            GivenMatchSetupWith(AMatch());
            GivenInitialize();

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
            GivenMatchSetupWith(AMatch());
            GivenInitialize();
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
            GivenMatchSetupWith(match);
            GivenInitialize();
            GivenMatchInRepository(match);
            GivenMatchStateRepository(MatchState.SelectUnit);
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
            GivenMatchSetupWith(match);
            GivenInitialize();
            GivenMatchInRepository(match);
            GivenMatchStateRepository(MatchState.SelectUpgrade);
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
            GivenMatchSetupWith(match);
            GivenInitialize();
            GivenMatchInRepository(match);
            GivenPlayServicePlayUnitCardReturns(expectedCardName, expectedHand);
            GivenMatchStateRepository(MatchState.SelectUnit);
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
            GivenMatchSetupWith(match);
            GivenInitialize();
            GivenMatchInRepository(match);
            GivenMatchStateRepository(MatchState.SelectUpgrade);
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
            GivenMatchSetupWith(match);
            GivenInitialize();
            GivenMatchInRepository(match);
            GivenMatchStateRepository(MatchState.SelectUpgrade);
            GivenPlayServicePlayUpgradeCardReturns(expectedCardName, expectedHand);
            upgradeCardSubject.OnNext(expectedCardName);

            ThenUpdatedHand(expectedHand);
            _view.Received(1).OnUpgradeCardPlayed(expectedCardName);
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
            GivenMatchStateRepository(MatchState.SelectUpgrade);

            GivenMatchSetupWith(AMatch(withHand: hand));
            GivenInitialize();
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
            GivenMatchStateRepository(MatchState.SelectUnit);
            GivenMatchSetupWith(AMatch(withHand: hand));
            GivenInitialize();
            GivenMatchInRepository(AMatch(withHand: hand));

            WhenUnitCardIsPlayed();
            ThenUnitCardIsRemovedFromHand(hand, unitCard);

            void WhenUnitCardIsPlayed() => unitCardPlayedSubject.OnNext(cardName);
        }

        [Test]
        public void WhenShowRoundUpgradeCompletedAndRoundIsInRerollThenChangeMatchStateToStartReroll()
        {
            var expectedRound = new Round() {RoundState = RoundState.Reroll, HasReroll = true};
            GivenMatchInRepository(AMatch(withRounds: new List<Round> {expectedRound}));
            var roundUpgradeCompletedSubject = GivenRoundUpgradeCompletedSubject();
            GivenInitialize();
            WhenRoundUpgradeCompletesBeingShown();
            _matchStateRepository.Received(1).Set(MatchState.StartReroll);

            void WhenRoundUpgradeCompletesBeingShown() => roundUpgradeCompletedSubject.OnNext(Unit.Default);
        }

        [Test]
        public void WhenShowRoundUpgradeCompletedAndRoundIsNotInRerollThenChangeMatchStateToStartUpgrade()
        {
            var expectedRound = new Round() {RoundState = RoundState.Upgrade, HasReroll = false};
            GivenMatchInRepository(AMatch(withRounds: new List<Round> {expectedRound}));
            var roundUpgradeCompletedSubject = GivenRoundUpgradeCompletedSubject();
            GivenInitialize();
            WhenRoundUpgradeCompletesBeingShown();
            _matchStateRepository.Received(1).Set(MatchState.StartUpgrade);
            
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
            GivenInitialize();
            WhenRestoreFocus();
            
            Received.InOrder(() =>
            {
                _view.Received(1).Clear();
                _view.Received(1).StartGame(game);
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
            GivenInitialize();
            WhenRestoreFocus();
            ThenChangeMatchStateTo(MatchState.StartReroll);
            
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
            GivenInitialize();
            WhenRestoreFocus();
            ThenChangeMatchStateTo(MatchState.WaitReroll);
            
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
            GivenInitialize();
            WhenRestoreFocus();
            ThenChangeMatchStateTo(MatchState.WaitUpgrade);
            
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
            GivenInitialize();
            WhenRestoreFocus();
            ThenChangeMatchStateTo(MatchState.StartUpgrade);
            
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
            GivenInitialize();
            WhenRestoreFocus();
            ThenChangeMatchStateTo(MatchState.WaitUnit);
            
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
            GivenInitialize();
            WhenRestoreFocus();
            ThenChangeMatchStateTo(MatchState.StartUnit);
            
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
            GivenInitialize();
            WhenRestoreFocus();
            ThenChangeMatchStateTo(MatchState.StartRound);
            
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
            GivenInitialize();
            WhenRestoreFocus();
            ThenEndGame();
            
            void WhenRestoreFocus() => applicationRestoreFocusSubject.OnNext(Unit.Default);
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

        private void GivenMatchStateRepository(MatchState matchState) =>
            _matchStateRepository.Get().Returns(matchState);

        private void GivenMatchInRepository(GameMatch gameMatch) =>
            _matchRepository.Get().Returns(gameMatch);

        private void GivenReRollCompletes(Hand hand) =>
            _playService.ReRollCards(Arg.Any<IList<string>>(), Arg.Any<IList<string>>())
                .Returns(Observable.Return(hand));


        private void GivenPlayServicePlayUpgradeCardReturns(string expectedCardName, Hand expectedHand) => _playService.PlayUpgradeCard(expectedCardName).Returns(Observable.Return(expectedHand));
        private void GivenInitialize() => WhenInitialize();

        private Subject<(List<string> upgrades, List<string> units)> GivenReRoll()
        {
            var rerollSubject = new Subject<(List<string> upgrades, List<string> units)>();
            _view.ReRoll().Returns(rerollSubject);
            return rerollSubject;
        }


        private void GivenMatchSetupWith(Match.Domain.GameMatch gameMatch) => _presenter.SetMatch(gameMatch);

        private void GivenPlayServicePlayUnitCardReturns(string expectedCardName, Hand expectedHand) =>
            _playService.PlayUnitCard(expectedCardName).Returns(Observable.Return(expectedHand));

        private static void WhenReRoll(Subject<(List<string> upgrades, List<string> units)> rerollSubject,
            List<string> expectedUpgrades, List<string> expectedUnits) =>
            rerollSubject.OnNext((expectedUpgrades, expectedUnits));


        private void WhenRoundSetup() => _presenter.StartNewRound();
        private void WhenInitialize() => _presenter.Initialize();

        private void ThenShowUpgradeCardsPlayedInCurrentRound(Round expectedRound) => _view.Received(1).ShowUpgradeCardsPlayedRound(expectedRound, Arg.Any<Action>());
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


        private void ThenGetRoundIsCalled(int round) => _playService.Received(1).GetRound(round);
        private void ThenShowRivalWaitUnit() => _view.Received(1).ShowRivalWaitUnit();
        private void ThenShowRivalWaitUpgrade() => _view.Received(1).ShowRivalWaitUpgrade();

        private void ThenViewReceivedOnUnitCardPlayed(string expectedCardName) =>
            _view.Received(1).OnUnitCardPlayed(expectedCardName);


        private void ThenChangeMatchStateTo(MatchState state) => _matchStateRepository.Received(1).Set(state);
        private void ThenEndGame() => _view.Received(1).EndGame();
    }
}