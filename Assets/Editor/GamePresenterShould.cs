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
        private Match _matchStatus;
        private ICardProvider _cardProvider;
        private IGameView _view;
        private IPlayService _playService;
        private ITokenService _tokenService;

        private const int CardsInHand = 5;

        [SetUp]
        public void Setup()
        {
            GivenCardProvider();
            GivenGameplayView();
            GivenPlayService();
            GivenTokenService();
            _presenter = new GamePresenter(_view, _playService, _tokenService);
            WhenGameSetup();
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
        public void PresentUpgradeCardWhenRoundStarts()
        {
            WhenRoundSetup();
            _view.Received(1).OnGetRoundInfo(Arg.Any<Round>());
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

        private void GivenPlayService()
        {
            _playService = Substitute.For<IPlayService>();
        }

        private void GivenGameplayView()
        {
            _view = Substitute.For<IGameView>();
        }


        private void GivenTokenService()
        {
            _tokenService = Substitute.For<ITokenService>();
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


        private void WhenGameSetup()
        {
            _matchStatus = new Match()
            {
                Hand = new Hand(_cardProvider.GetUnitCards().Take(5).ToList(),
                    _cardProvider.GetUpgradeCards().Take(5).ToList())
            };
            _presenter.SetMatch(_matchStatus);
        }

        private void WhenRoundSetup()
        {
            _presenter.StartNewRound();
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
            _playService.Received(1).PlayUpgradeCard(null,Arg.Any<Action<Hand>>(),Arg.Any<Action<long, string>>());
        }

        private void ThenPlayUnitCardIsCalledInService()
        {
            _playService.Received(1).PlayUnitCard(null, Arg.Any<Action<Hand>>(),Arg.Any<Action<long, string>>());
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