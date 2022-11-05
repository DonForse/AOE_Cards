using System.Collections.Generic;
using System.Linq;
using Data;
using Features.Game.Scripts.Domain;
using Features.Game.Scripts.Presentation;
using Game;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Services;
using NSubstitute;
using NUnit.Framework;
using Token;
using UniRx;
using UnityEngine;

namespace Features.Game.Scripts.Tests.Editor
{
    public class GamePresenterShould
    {
        private Hand _cardsInHand;
        private GamePresenter _presenter;
        private Match.Domain.GameMatch _gameMatchStatus;
        private ICardProvider _cardProvider;
        private IPlayService _playService;
        private ITokenService _tokenService;
        private IGameView _view;
        private IGetRoundEvery3Seconds _getRoundEvery3Seconds;
        private ICurrentMatchRepository _matchRepository;

        private const int CardsInHand = 5;

        [SetUp]
        public void Setup()
        {
            _cardProvider = Substitute.For<ICardProvider>();
            _playService = Substitute.For<IPlayService>();
            _tokenService = Substitute.For<ITokenService>();
            _view = Substitute.For<IGameView>();
            _getRoundEvery3Seconds = Substitute.For<IGetRoundEvery3Seconds>();
            _matchRepository = Substitute.For<ICurrentMatchRepository>();
            _presenter = new GamePresenter(_view, _playService, _tokenService, _getRoundEvery3Seconds,
                _matchRepository);
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
            _getRoundEvery3Seconds.Execute().Returns(Observable.Return(expectedRound));
            GivenMatchSetupWith(AMatch());
            WhenInitialize();
            _view.Received(1).OnGetRoundInfo(expectedRound);
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
            ISubject<string> unitCardPlayedSubject = new Subject<string>();
            _view.UnitCardPlayed().Returns(unitCardPlayedSubject);
            GivenMatchSetupWith(AMatch());
            GivenInitialize();
            GivenMatchInRepository(AMatch());
            unitCardPlayedSubject.OnNext(expectedCardName);
            ThenPlayUnitCardIsCalledInService(expectedCardName);
        }

        [Test]
        public void PlayUnitCardWhenPlayServiceReturns()
        {
            var expectedCardName = "some card";
            ISubject<string> unitCardPlayedSubject = new Subject<string>();
            var expectedHand = new Hand(new List<UnitCardData>(), new List<UpgradeCardData>());

            _view.UnitCardPlayed().Returns(unitCardPlayedSubject);
            GivenMatchSetupWith(AMatch());
            GivenInitialize();
            GivenMatchInRepository(AMatch());

            _playService.PlayUnitCard(expectedCardName).Returns(Observable.Return(expectedHand));
            unitCardPlayedSubject.OnNext(expectedCardName);
            
            ThenUpdatedHand(expectedHand);
            _view.Received(1).OnUnitCardPlayed();
        }

        [Test]
        public void PlayUpgradeCardWhenCardIsPlayed()
        {
            
            var expectedCardName = "some card";
            ISubject<string> upgradeCardPlayedSubject = new Subject<string>();
            _view.UpgradeCardPlayed().Returns(upgradeCardPlayedSubject);
            GivenMatchSetupWith(AMatch());
            GivenInitialize();
            GivenMatchInRepository(AMatch());

            WhenUpgradeCardIsPlayed();
            ThenPlayUpgradeCardIsCalledInService(expectedCardName);

            void WhenUpgradeCardIsPlayed() => upgradeCardPlayedSubject.OnNext(expectedCardName);
        }

        [Test]
        public void PlayUpgradeCardWhenPlayServiceReturns()
        {
            var expectedCardName = "some card";
            ISubject<string> upgradeCardSubject = new Subject<string>();
            var expectedHand = new Hand(new List<UnitCardData>(), new List<UpgradeCardData>());

            _view.UpgradeCardPlayed().Returns(upgradeCardSubject);
            GivenMatchSetupWith(AMatch());
            GivenInitialize();
            GivenMatchInRepository(AMatch());

            _playService.PlayUpgradeCard(expectedCardName).Returns(Observable.Return(expectedHand));
            upgradeCardSubject.OnNext(expectedCardName);
            
            ThenUpdatedHand(expectedHand);
            _view.Received(1).OnUpgradeCardPlayed();
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
            GivenMatchSetupWith(AMatch(withHand: hand));
            GivenInitialize();
            GivenMatchInRepository(AMatch(withHand: hand));

            unitCardPlayedSubject.OnNext(cardName);
            ThenUnitCardIsRemovedFromHand(hand, unitCard);
        }

        [Test]
        public void GiveUnitCardsToPlayerOnGameSetup()
        {
            Assert.Fail();
            //   WhenGetPlayerHand();
            ThenUnitCardsInPlayerHandsAreEqualTo(CardsInHand);
        }

        [Test]
        public void GiveUpgradeCardsToPlayerOnGameSetup()
        {
            Assert.Fail();
            ThenUpgradeCardsInPlayerHandsAreEqualTo(CardsInHand);
        }

        private Match.Domain.GameMatch AMatch(int withUnits = 5,
            int withUpgrades = 5,
            List<Round> withRounds = null,
            string withMatchId = "MatchId",
            string[] withUsers = null,
            Hand withHand = null)
        {
            return new Match.Domain.GameMatch
            {
                Hand = withHand ?? new Hand(_cardProvider.GetUnitCards().Take(withUnits).ToList(),
                    _cardProvider.GetUpgradeCards().Take(withUpgrades).ToList()),
                Board = new Board {Rounds = withRounds ?? new List<Round>() {new Round() {RoundNumber = 0}}},
                Id = withMatchId,
                Users = withUsers ?? new[] {"user-1", "user-2"}
            };
        }

        private void GivenCardProviderReturnsAListOfUnitsAndUpgrades()
        {
            _cardProvider.GetUnitCards().Returns(new List<UnitCardData>
            {
                ScriptableObject.CreateInstance<UnitCardData>(),
                ScriptableObject.CreateInstance<UnitCardData>(),
                ScriptableObject.CreateInstance<UnitCardData>(),
                ScriptableObject.CreateInstance<UnitCardData>(),
                ScriptableObject.CreateInstance<UnitCardData>(),
                ScriptableObject.CreateInstance<UnitCardData>(),
                ScriptableObject.CreateInstance<UnitCardData>(),
                ScriptableObject.CreateInstance<UnitCardData>(),
                ScriptableObject.CreateInstance<UnitCardData>(),
                ScriptableObject.CreateInstance<UnitCardData>(),
                ScriptableObject.CreateInstance<UnitCardData>(),
                ScriptableObject.CreateInstance<UnitCardData>(),
                ScriptableObject.CreateInstance<UnitCardData>(),
                ScriptableObject.CreateInstance<UnitCardData>(),
                ScriptableObject.CreateInstance<UnitCardData>(),
            });
            _cardProvider.GetUpgradeCards().Returns(new List<UpgradeCardData>
            {
                ScriptableObject.CreateInstance<UpgradeCardData>(),
                ScriptableObject.CreateInstance<UpgradeCardData>(),
                ScriptableObject.CreateInstance<UpgradeCardData>(),
                ScriptableObject.CreateInstance<UpgradeCardData>(),
                ScriptableObject.CreateInstance<UpgradeCardData>(),
                ScriptableObject.CreateInstance<UpgradeCardData>(),
                ScriptableObject.CreateInstance<UpgradeCardData>(),
                ScriptableObject.CreateInstance<UpgradeCardData>(),
                ScriptableObject.CreateInstance<UpgradeCardData>(),
                ScriptableObject.CreateInstance<UpgradeCardData>(),
                ScriptableObject.CreateInstance<UpgradeCardData>(),
                ScriptableObject.CreateInstance<UpgradeCardData>(),
                ScriptableObject.CreateInstance<UpgradeCardData>(),
            });
        }

        private void GivenMatchInRepository(Match.Domain.GameMatch gameMatch)
        {
            _matchRepository.Get().Returns(gameMatch);
        }


        private void GivenReRollCompletes(Hand hand) =>
            _playService.ReRollCards(Arg.Any<IList<string>>(), Arg.Any<IList<string>>())
                .Returns(Observable.Return(hand));


        private void GivenInitialize() => WhenInitialize();

        private Subject<(List<string> upgrades, List<string> units)> GivenReRoll()
        {
            var rerollSubject = new Subject<(List<string> upgrades, List<string> units)>();
            _view.ReRoll().Returns(rerollSubject);
            return rerollSubject;
        }

        private void GivenMatchSetupWith(Match.Domain.GameMatch gameMatch)
        {
            _presenter.SetMatch(gameMatch);
        }


        private static void WhenReRoll(Subject<(List<string> upgrades, List<string> units)> rerollSubject, List<string> expectedUpgrades, List<string> expectedUnits) => rerollSubject.OnNext((expectedUpgrades, expectedUnits));

        private void WhenRoundSetup() => _presenter.StartNewRound();
        private void WhenInitialize() => _presenter.Initialize();

        private void ThenUnitCardsInPlayerHandsAreEqualTo(int numberOfCards) =>
            Assert.AreEqual(numberOfCards, _cardsInHand.GetUnitCards().Count);

        private void ThenUpgradeCardsInPlayerHandsAreEqualTo(int numberOfCards) =>
            Assert.AreEqual(numberOfCards, _cardsInHand.GetUpgradeCards().Count);

        private void ThenUpdatedHand(Hand hand) => _matchRepository.Received(1).Set(hand);

        private void ThenPlayUpgradeCardIsCalledInService(string cardName) => _playService.Received(1).PlayUpgradeCard(cardName);
        private void ThenPlayUnitCardIsCalledInService(string cardName) => _playService.Received(1).PlayUnitCard(cardName);
        private void ThenUnitCardIsRemovedFromHand(Hand hand, UnitCardData card) => Assert.IsTrue(!hand.GetUnitCards().ToList().Contains(card));
        private void ThenUpgradeCardIsRemovedFromHand(Hand hand, UpgradeCardData card) =>  Assert.IsTrue(!hand.GetUpgradeCards().ToList().Contains(card));
        private void ThenGetRoundIsCalled(int round) => _playService.Received(1).GetRound(round);
    }
}