using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class GameplayPresenterShould
    {
        private Hand _cardsInHand;
        private GameplayPresenter _presenter;
        private GetDeck _getDeck;
        private IList<IPlayer> _players;
        private ICardProvider _cardProvider;
        private IGameplayView _view;
        private const string PlayerOneId = "0";

        [SetUp]
        public void Setup()
        {
            GivenPlayersAddedToTheGame(3);
            GivenCardProvider();
            _view = Substitute.For<IGameplayView>();
            _getDeck = new GetDeck(_cardProvider);
            _presenter = new GameplayPresenter(_view,_getDeck, _players);
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

        private void GivenPlayersAddedToTheGame(int amount)
        {
            _players = new List<IPlayer>();
            for (int i = 0; i < amount; i++)
            {

                var _player = Substitute.For<IPlayer>();
                _player.GetId().Returns(i.ToString());
                _players.Add(_player);
            }
        }

        [Test]
        public void GiveUnitCardsToPlayersOnGameSetup()
        {
            WhenGameSetup();
            WhenGetPlayerHand(PlayerOneId);
            ThenUnitCardsInPlayerHandsAreEqualTo(5);
        }
        [Test]
        public void GiveEventCardsToPlayersOnGameSetup()
        {
            WhenGameSetup();
            WhenGetPlayerHand(PlayerOneId);
            ThenEventCardsInPlayerHandsAreEqualTo(5);
        }
        private void WhenGetPlayerHand(string playerId)
        {
            _cardsInHand = _presenter.GetHand(playerId);
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
            Assert.AreEqual(numberOfCards, _cardsInHand.UnitCards.Count);
        }
        private void ThenEventCardsInPlayerHandsAreEqualTo(int numberOfCards)
        {
            Assert.AreEqual(numberOfCards, _cardsInHand.EventCards.Count);
        }

        [Test]
        public void PresentEventCardWhenRoundStarts() {
            WhenGameSetup();
            WhenRoundSetup();
            _view.Received(1).ShowRoundEventCard(Arg.Any<EventCardData>());
        }
        //// A Test behaves as an ordinary method
        //[Test]
        //public void SetCardWhenCardIsPlayed()
        //{
        //    // Use the Assert class to test conditions
        //}

        //[Test]
        //public void DecideWinnerWhenUnitCardsArePlayed() {

        //}
    }
}
