using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Features.Game.Scripts.Domain;
using Game;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Services;
using NSubstitute;
using NUnit.Framework;
using ServerLogic.Matches.Infrastructure;
using Token;
using UniRx;
using UnityEngine;

namespace Features.Game.Scripts.Tests.Editor
{
    public class GamePresenterShould
    {
        private Hand _cardsInHand;
        private GamePresenter _presenter;
        private Match.Domain.Match _matchStatus;
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
            _presenter = new GamePresenter(_view, _playService, _tokenService,_getRoundEvery3Seconds, _matchRepository);
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
        public void SendRerollWhenReroll()
        {
            var rerollSubject = new Subject<(List<string> upgrades, List<string> units)>();
            _view.Reroll().Returns(rerollSubject);
            GivenMatchSetupWith(AMatch());
             GivenInitialize();
             var units = new List<string>();
             var upgrades = new List<string>();
             rerollSubject.OnNext((upgrades,units));
             _playService.Received(1).RerollCards(units, upgrades);
        }

        [Test]
        public void GiveUnitCardsToPlayerOnGameSetup()
        {
         //   WhenGetPlayerHand();
            ThenUnitCardsInPlayerHandsAreEqualTo(CardsInHand);
        }

        [Test]
        public void GiveUpgradeCardsToPlayerOnGameSetup()
        {
            ThenUpgradeCardsInPlayerHandsAreEqualTo(CardsInHand);
        }

        [Test]
        public void RemoveUpgradeCardFromHandWhenUpgradeCardIsPlayed()
        {
            WhenUpgradeCardIsPlayed();
            ThenUpgradeCardIsRemovedFromHand();
        }

        [Test]
        public void PlayUpgradeCardWhenCardIsPlayed()
        {
            WhenUpgradeCardIsPlayed();
            ThenPlayUpgradeCardIsCalledInService();
        }

        [Test]
        public void RemoveUnitCardFromHandWhenUnitCardIsPlayed()
        {
            WhenUnitCardIsPlayed();
            ThenUnitCardIsRemovedFromHand();
        }

        [Test]
        public void PlayUnitCardWhenCardIsPlayed()
        {
            WhenUnitCardIsPlayed();
            ThenPlayUnitCardIsCalledInService();
        }

        private Match.Domain.Match AMatch(int withUnits = 5,
            int withUpgrades = 5, 
            List<Round> withRounds = null,
            string withMatchId = "MatchId",
            string[] withUsers = null)
        {
            return new Match.Domain.Match
            {
                Hand = new Hand(_cardProvider.GetUnitCards().Take(withUnits).ToList(),
                    _cardProvider.GetUpgradeCards().Take(withUpgrades).ToList()),
                Board = new Board{Rounds = withRounds ?? new List<Round>(){new Round(){RoundNumber = 0}}},
                Id = withMatchId,
                Users = withUsers ?? new []{"user-1", "user-2"}
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

        private void GivenInitialize() => WhenInitialize();

        private void WhenUnitCardIsPlayed() => _presenter.PlayUnitCard(null);
        private void WhenUpgradeCardIsPlayed() => _presenter.PlayUpgradeCard(null);


        private void GivenMatchSetupWith(Match.Domain.Match match)
        {
            _presenter.SetMatch(match);
        }

        private void WhenRoundSetup() => _presenter.StartNewRound();
        private void WhenInitialize() => _presenter.Initialize();

        private void ThenUnitCardsInPlayerHandsAreEqualTo(int numberOfCards) => Assert.AreEqual(numberOfCards, _cardsInHand.GetUnitCards().Count);
        private void ThenUpgradeCardsInPlayerHandsAreEqualTo(int numberOfCards) => Assert.AreEqual(numberOfCards, _cardsInHand.GetUpgradeCards().Count);
        private void ThenPlayUpgradeCardIsCalledInService() => _playService.Received(1).PlayUpgradeCard(null);
        private void ThenPlayUnitCardIsCalledInService() => _playService.Received(1).PlayUnitCard(null);
        private void ThenUnitCardIsRemovedFromHand() => ThenUnitCardsInPlayerHandsAreEqualTo(CardsInHand - 1);
        private void ThenUpgradeCardIsRemovedFromHand() => ThenUpgradeCardsInPlayerHandsAreEqualTo(CardsInHand - 1);
        private void ThenGetRoundIsCalled(int round) => _playService.Received(1).GetRound(round);

    }
}