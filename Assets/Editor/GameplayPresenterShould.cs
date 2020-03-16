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
        private const string PlayerOneId = "0";

        [SetUp]
        public void Setup()
        {
            GivenPlayersAddedToTheGame(3);
            _cardProvider = Substitute.For<ICardProvider>();
            _cardProvider.GetUnitCards().Returns(new List<UnitCardData>
            {
                ScriptableObject.CreateInstance<UnitCardData>(),
                ScriptableObject.CreateInstance<UnitCardData>(),
                ScriptableObject.CreateInstance<UnitCardData>(),
                ScriptableObject.CreateInstance<UnitCardData>(),
                ScriptableObject.CreateInstance<UnitCardData>()
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
            _getDeck = new GetDeck(_cardProvider);
            _presenter = new GameplayPresenter(_getDeck, _players);
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
        public void GiveUnitCardsToPlayersOnRoundSetup()
        {
            WhenRoundSetup();
            WhenGetPlayerHand(PlayerOneId);
            ThenUnitCardsInPlayerHandsAreEqualTo(5);
        }
        [Test]
        public void GiveEventCardsToPlayersOnRoundSetup()
        {
            WhenRoundSetup();
            WhenGetPlayerHand(PlayerOneId);
            ThenEventCardsInPlayerHandsAreEqualTo(5);
        }

        private void WhenGetPlayerHand(string playerId)
        {
            _cardsInHand = _presenter.GetHand(playerId);
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



        //[Test]
        //public void GiveEventsCardsToPlayersOnRoundSetup() { }


        //[Test]
        //public void RemoveCardFromDeckWhenCardIsPlayed() {

        //}

        //[Test]
        //public void PresentEventCardWhenRoundStarts() {

        //}
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
