using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Infrastructure.Services;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Editor
{
    public class GamePresenterShould
    {
        private Hand _cardsInHand;
        private GamePresenter _presenter;
        private MatchStatus _matchStatus;
        private ICardProvider _cardProvider;
        private IGameView _view;
        private IMatchService _matchService;

        private const int CardsInHand = 5;

        [SetUp]
        public void Setup()
        {
            GivenCardProvider();
            GivenGameplayView();
            GivenMatchService();
            _presenter = new GamePresenter(_view, _matchService);
            WhenGameSetup();
        }

        [Test]
        public void GiveUnitCardsToPlayerOnGameSetup()
        {
            WhenGetPlayerHand();
            ThenUnitCardsInPlayerHandsAreEqualTo(CardsInHand);
        }

        [Test]
        public void GiveUpgradeCardsToPlayerOnGameSetup()
        {
            WhenGetPlayerHand();
            ThenUpgradeCardsInPlayerHandsAreEqualTo(CardsInHand);
        }

        [Test]
        public void PresentUpgradeCardWhenRoundStarts()
        {
            WhenRoundSetup();
            _view.Received(1).ShowRoundUpgradeCard(Arg.Any<UpgradeCardData>());
        }

        [Test]
        public void RemoveUpgradeCardFromHandWhenUpgradeCardIsPlayed()
        {
            WhenUpgradeCardIsPlayed();
            WhenGetPlayerHand();
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
            WhenGetPlayerHand();
            ThenUnitCardIsRemovedFromHand();
        }

        [Test]
        public void PlayUnitCardWhenCardIsPlayed()
        {
            WhenUnitCardIsPlayed();
            ThenPlayUnitCardIsCalledInService();
        }

        private void GivenMatchService()
        {
            _matchService = Substitute.For<IMatchService>();
        }

        private void GivenGameplayView()
        {
            _view = Substitute.For<IGameView>();
        }

        private void GivenCardProvider()
        {
            _cardProvider = Substitute.For<ICardProvider>();
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

        private void WhenUnitCardIsPlayed()
        {
            _presenter.PlayUnitCard(null);
        }

        private void WhenUpgradeCardIsPlayed()
        {
            _presenter.PlayUpgradeCard(null);
        }

        private void WhenGetPlayerHand()
        {
            _cardsInHand = _presenter.GetHand();
        }

        private void WhenGameSetup()
        {
            _matchStatus = new MatchStatus()
            {
                hand = new Hand(_cardProvider.GetUnitCards().Take(5).ToList(),
                    _cardProvider.GetUpgradeCards().Take(5).ToList())
            };
            _presenter.GameSetup(_matchStatus);
        }

        private void WhenRoundSetup()
        {
            _presenter.RoundSetup(ScriptableObject.CreateInstance<UpgradeCardData>());
        }

        private void ThenUnitCardsInPlayerHandsAreEqualTo(int numberOfCards)
        {
            Assert.AreEqual(numberOfCards, _cardsInHand.GetUnitCards().Count);
        }

        private void ThenUpgradeCardsInPlayerHandsAreEqualTo(int numberOfCards)
        {
            Assert.AreEqual(numberOfCards, _cardsInHand.GetUpgradeCards().Count);
        }

        private void ThenPlayUpgradeCardIsCalledInService()
        {
            _matchService.Received(1).PlayUpgradeCard(null,Arg.Any<Action<Round>>(),Arg.Any<Action<string>>());
        }

        private void ThenPlayUnitCardIsCalledInService()
        {
            _matchService.Received(1).PlayUnitCard(null, Arg.Any<Action<RoundResult>>(),Arg.Any<Action<string>>());
        }

        private void ThenUnitCardIsRemovedFromHand()
        {
            ThenUnitCardsInPlayerHandsAreEqualTo(CardsInHand - 1);
        }

        private void ThenUpgradeCardIsRemovedFromHand()
        {
            ThenUpgradeCardsInPlayerHandsAreEqualTo(CardsInHand - 1);
        }
    }
}