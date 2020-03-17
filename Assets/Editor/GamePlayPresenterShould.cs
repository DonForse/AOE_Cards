using System.Collections.Generic;
using GamePlay;
using Infrastructure.Services;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Editor
{
    public class GamePlayPresenterShould
    {
        private Hand _cardsInHand;
        private GamePlayPresenter _presenter;
        private GetDeck _getDeck;
        private ICardProvider _cardProvider;
        private IGameplayView _view;
        private IMatchService _matchService;

        private const int CardsInHand = 5;

        [SetUp]
        public void Setup()
        {
            GivenCardProvider();
            GivenGameplayView();
            GivenMatchService();
            GivenGameProvider();
            _presenter = new GamePlayPresenter(_view, _matchService, _getDeck);
            WhenGameSetup();
        }

        [Test]
        public void GiveUnitCardsToPlayerOnGameSetup()
        {
            WhenGetPlayerHand();
            ThenUnitCardsInPlayerHandsAreEqualTo(CardsInHand);
        }

        [Test]
        public void GiveEventCardsToPlayerOnGameSetup()
        {
            WhenGetPlayerHand();
            ThenEventCardsInPlayerHandsAreEqualTo(CardsInHand);
        }

        [Test]
        public void PresentEventCardWhenRoundStarts()
        {
            WhenRoundSetup();
            _view.Received(1).ShowRoundEventCard(Arg.Any<EventCardData>());
        }

        [Test]
        public void RemoveEventCardFromHandWhenEventCardIsPlayed()
        {
            WhenEventCardIsPlayed();
            WhenGetPlayerHand();
            ThenEventCardIsRemovedFromHand();
        }

        [Test]
        public void PlayEventCardWhenCardIsPlayed()
        {
            WhenEventCardIsPlayed();
            ThenPlayEventCardIsCalledInService();
        }

        [Test]
        public void RemoveUnitCardFromHandWhenEventCardIsPlayed()
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

        private void GivenGameProvider()
        {
            _getDeck = new GetDeck(_cardProvider);
        }

        private void GivenMatchService()
        {
            _matchService = Substitute.For<IMatchService>();
        }

        private void GivenGameplayView()
        {
            _view = Substitute.For<IGameplayView>();
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
            _cardProvider.GetEventCards().Returns(new List<EventCardData>
            {
                ScriptableObject.CreateInstance<EventCardData>(),
                ScriptableObject.CreateInstance<EventCardData>(),
                ScriptableObject.CreateInstance<EventCardData>(),
                ScriptableObject.CreateInstance<EventCardData>(),
                ScriptableObject.CreateInstance<EventCardData>(),
                ScriptableObject.CreateInstance<EventCardData>(),
                ScriptableObject.CreateInstance<EventCardData>(),
                ScriptableObject.CreateInstance<EventCardData>(),
                ScriptableObject.CreateInstance<EventCardData>(),
                ScriptableObject.CreateInstance<EventCardData>(),
                ScriptableObject.CreateInstance<EventCardData>(),
                ScriptableObject.CreateInstance<EventCardData>(),
                ScriptableObject.CreateInstance<EventCardData>(),
            });
        }
        private void WhenUnitCardIsPlayed()
        {
            _presenter.PlayUnitCard(null);
        }

        private void WhenEventCardIsPlayed()
        {
            _presenter.PlayEventCard(null);
        }

        private void WhenGetPlayerHand()
        {
            _cardsInHand = _presenter.GetHand();
        }

        private void WhenGameSetup()
        {
            _presenter.GameSetup();
        }

        private void WhenRoundSetup()
        {
            _presenter.RoundSetup();
        }

        private void ThenUnitCardsInPlayerHandsAreEqualTo(int numberOfCards)
        {
            Assert.AreEqual(numberOfCards, _cardsInHand.GetUnitCards().Count);
        }

        private void ThenEventCardsInPlayerHandsAreEqualTo(int numberOfCards)
        {
            Assert.AreEqual(numberOfCards, _cardsInHand.GetEventCards().Count);
        }

        private void ThenPlayEventCardIsCalledInService()
        {
            _matchService.Received(1).PlayEventCard(null);
        }

        private void ThenPlayUnitCardIsCalledInService()
        {
            _matchService.Received(1).PlayUnitCard(null);
        }

        private void ThenUnitCardIsRemovedFromHand()
        {
            ThenUnitCardsInPlayerHandsAreEqualTo(CardsInHand - 1);
        }

        private void ThenEventCardIsRemovedFromHand()
        {
            ThenEventCardsInPlayerHandsAreEqualTo(CardsInHand - 1);
        }
    }
}