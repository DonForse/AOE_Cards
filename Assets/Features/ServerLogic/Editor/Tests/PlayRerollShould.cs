using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Actions;
using Features.ServerLogic.Cards.Domain.Entities;
using Features.ServerLogic.Cards.Infrastructure;
using Features.ServerLogic.Editor.Tests.Mothers;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Matches.Infrastructure.DTO;
using Features.ServerLogic.Users.Domain;
using NSubstitute;
using NUnit.Framework;

namespace Features.ServerLogic.Editor.Tests
{
    public class PlayRerollShould
    {
        private const string MatchId = "MatchId";
        private const string UserId = "UserId";
        private ICardRepository _cardRepository;
        private PlayReroll _playReroll;
        private IGetUpgradeCard _getUpgradeCard;
        private IGetUnitCard _getUnitCard;
        private IMatchesRepository _matchesRepository;


        [SetUp]
        public void Setup()
        {
            _getUnitCard = Substitute.For<IGetUnitCard>();
            _getUpgradeCard = Substitute.For<IGetUpgradeCard>();
            _matchesRepository = Substitute.For<IMatchesRepository>();
            _playReroll = new PlayReroll(_matchesRepository, _getUnitCard, _getUpgradeCard);
        }

        [Test]
        public void ThrowsWhenNoMatch()
        {
            GivenNoMatch();
            ThenThrows(() => WhenReroll());

            void GivenNoMatch() => _matchesRepository.Get(MatchId).Returns((ServerMatch)null);
        }
        
        [Test]
        public void ThrowsWhenNoRounds()
        {
            var match = ServerMatchMother.Create(MatchId, withBoard: BoardMother.Create());
            GivenMatch(match);
            ThenThrows(() => WhenReroll());
        }

        [Test]
        public void ThrowsWhenNotInReroll()
        {
            var match = AMatch(RoundState.Unit);
            GivenMatch(match);

            ThenThrows(() => WhenReroll());
        }

        [Test]
        public void ThrowsWhenUpgradeCardAlreadyPlayedForThatRound()
        {
            var match = AMatchWithAnUpgradeCardPlayed();
            GivenMatch(match);
            ThenThrows(() => WhenReroll());

            ServerMatch AMatchWithAnUpgradeCardPlayed() =>
                AMatch(withPlayerCards: new Dictionary<string, PlayerCard>
                {
                    {UserId, new PlayerCard() {UpgradeCard = UpgradeCardMother.Create("some card")}}
                });
        }

        [Test]
        public void ThrowsWhenPlayerCannotReroll()
        {
            var match = AMatch(withPlayersReroll: new Dictionary<string, bool>() {{UserId, true}});
            GivenMatch(match);
            ThenThrows(() => WhenReroll());
        }

        [Test]
        public void ThrowsWhenUnitCardSelectedForRerollDoesNotExists()
        {
            var cardName = "some card";
            GivenGetUnitCardsReturnsNull();
            var match = AMatch();
            GivenMatch(match);
            ThenThrows(() => WhenReroll(ARerollInfoDto(new List<string> {cardName}, new List<string>())));

            void GivenGetUnitCardsReturnsNull() => _getUnitCard.Execute(cardName).Returns((UnitCard) null);
        }

        [Test]
        public void ThrowsWhenUnitCardSelectedForRerollIsNotInPlayerHand()
        {
            var rerolledCardName = "Rerolled Card";
            var cardNameInHand = "Card in Hand";
            var unitCardInHand = GivenUnitCard(cardNameInHand);
            GivenUnitCard(rerolledCardName);

            var match = AMatch(withPlayerHands: PlayerHandsWithUnit(unitCardInHand));
            GivenMatch(match);
            ThenThrows(() =>
                WhenReroll(ARerollInfoDto(new List<string> {rerolledCardName}, new List<string>())));
        }

        [Test]
        public void ThrowsWhenUpgradeCardSelectedForRerollDoesNotExists()
        {
            var cardName = "some card";
            GivenUpgradeCardIsNull();
            var match = AMatch();
            GivenMatch(match);
            ThenThrows(() => WhenReroll(ARerollInfoDto(new List<string>(), new List<string>() {cardName})));

            void GivenUpgradeCardIsNull() => _getUpgradeCard.Execute(cardName).Returns((UpgradeCard) null);
        }


        [Test]
        public void ThrowsWhenUpgradeCardSelectedForRerollIsNotInPlayerHand()
        {
            var rerolledCardName = "Rerolled Card";
            var cardNameInHand = "Card in Hand";
            var upgradeCardInHand = GivenUpgradeCard(cardNameInHand);
            GivenUpgradeCard(rerolledCardName);

            var match = AMatch(withPlayerHands: PlayerHandsWithUpgradeCard(upgradeCardInHand));
            GivenMatch(match);
            ThenThrows(() =>
                WhenReroll(ARerollInfoDto(new List<string>() , new List<string> {rerolledCardName})));
        }

        [Test]
        public void IgnoreVillagerCardReroll()
        {
            var playerRerolls= new Dictionary<string, bool>(){{UserId, false}};
            var unitCardName = "Villager";
            var upgradeCardName = "Card in Hand - Upgrade";
            var unitCardThatWillBeRerolled = GivenUnitCard(unitCardName);
            var upgradeCardThatWillBeRerolled = GivenUpgradeCard(upgradeCardName);
            
            var unitDeckCard = UnitCardMother.Create("Deck card");
            var upgradeDeckCard = UpgradeCardMother.Create("Deck card");
            var deck = ADeckWithCards();

            var match = AMatch(
                withPlayerHands: PlayerHandWithCards(unitCardThatWillBeRerolled, upgradeCardThatWillBeRerolled),
                withDeck: deck, 
                withPlayersReroll:playerRerolls);
            GivenMatch(match);

            WhenReroll(ARerollInfoDto(new List<string> {unitCardName}, new List<string> {upgradeCardName}));

            
            Assert.Contains(unitCardThatWillBeRerolled, match.Board.PlayersHands[UserId].UnitsCards.ToList());
            Assert.Contains(upgradeDeckCard, match.Board.PlayersHands[UserId].UpgradeCards.ToList());
            Assert.Contains(unitDeckCard, match.Board.Deck.UnitCards);
            Assert.Contains(upgradeCardThatWillBeRerolled, match.Board.Deck.UpgradeCards);
            Assert.AreEqual(true, playerRerolls[UserId]);
            
            Deck ADeckWithCards() =>
                new()
                {
                    UnitCards = new ConcurrentStack<UnitCard>(new List<UnitCard> {unitDeckCard}),
                    UpgradeCards = new ConcurrentStack<UpgradeCard>(new List<UpgradeCard> {upgradeDeckCard})
                };
        }

        [Test]
        public void SetPlayerRerollToTrue()
        {
            var playerRerolls= new Dictionary<string, bool>(){{UserId, false}};
            var unitCardName = "Card in Hand - Unit";
            var upgradeCardName = "Card in Hand - Upgrade";
            var unitCardThatWillBeRerolled = GivenUnitCard(unitCardName);
            var upgradeCardThatWillBeRerolled = GivenUpgradeCard(upgradeCardName);

            var unitDeckCard = UnitCardMother.Create("Deck card");
            var upgradeDeckCard = UpgradeCardMother.Create("Deck card");
            var deck = ADeckWithCards();

            var match = AMatch(
                withPlayerHands: PlayerHandWithCards(unitCardThatWillBeRerolled, upgradeCardThatWillBeRerolled),
                withDeck: deck, 
                withPlayersReroll:playerRerolls);
            GivenMatch(match);

            WhenReroll(ARerollInfoDto(new List<string> {unitCardName}, new List<string> {upgradeCardName}));

            
            Assert.Contains(unitDeckCard, match.Board.PlayersHands[UserId].UnitsCards.ToList());
            Assert.Contains(upgradeDeckCard, match.Board.PlayersHands[UserId].UpgradeCards.ToList());
            Assert.Contains(unitCardThatWillBeRerolled, match.Board.Deck.UnitCards);
            Assert.Contains(upgradeCardThatWillBeRerolled, match.Board.Deck.UpgradeCards);
            Assert.AreEqual(true, playerRerolls[UserId]);
            
            Deck ADeckWithCards() =>
                new()
                {
                    UnitCards = new ConcurrentStack<UnitCard>(new List<UnitCard> {unitDeckCard}),
                    UpgradeCards = new ConcurrentStack<UpgradeCard>(new List<UpgradeCard> {upgradeDeckCard})
                };
        }

        [Test]
        public void NotChangeNotRerolledUnitHandCards()
        {
            var cardNameInHand = "Card in Hand";
            var anotherCardNameInHandThatWillBeRerolled = "Card in Hand";
            var deckCard = UnitCardMother.Create("Deck card");
            var unitCardInHand = GivenUnitCard(cardNameInHand);
            var unitCardThatWillBeRerolled = GivenUnitCard(anotherCardNameInHandThatWillBeRerolled);

            var deck = ADeckWithAUnitCard();

            var match = AMatch(
                withPlayerHands: APlayerHandWithMultipleUnits(unitCardInHand, unitCardThatWillBeRerolled),
                withDeck: deck);
            GivenMatch(match);
            WhenReroll(ARerollInfoDto(new List<string> {anotherCardNameInHandThatWillBeRerolled}, new List<string>()));
            
            Assert.Contains(unitCardInHand, match.Board.PlayersHands[UserId].UnitsCards.ToList());

            Deck ADeckWithAUnitCard() =>
                new()
                {
                    UnitCards = new ConcurrentStack<UnitCard>(new List<UnitCard> {deckCard}),
                    UpgradeCards = new ConcurrentStack<UpgradeCard>(new List<UpgradeCard>())
                };
        }

        [Test]
        public void ChangeRerolledUnitHandCards()
        {
            var cardNameInHand = "Card in Hand";
            var anotherCardNameInHandThatWillBeRerolled = "Card in Hand";
            var deckCard = UnitCardMother.Create("Deck card");
            var unitCardInHand = GivenUnitCard(cardNameInHand);
            var unitCardThatWillBeRerolled = GivenUnitCard(anotherCardNameInHandThatWillBeRerolled);
            var deck = ADeckWithAUnitCard();

            var match = AMatch(
                withPlayerHands: APlayerHandWithMultipleUnits(unitCardInHand, unitCardThatWillBeRerolled),
                withDeck: deck);
            
            GivenMatch(match);
            WhenReroll(ARerollInfoDto(new List<string> {anotherCardNameInHandThatWillBeRerolled}, new List<string>()));
            
            Assert.Contains(deckCard, match.Board.PlayersHands[UserId].UnitsCards.ToList());
            Assert.IsFalse(match.Board.PlayersHands[UserId].UnitsCards.Contains(unitCardThatWillBeRerolled));

            Deck ADeckWithAUnitCard() =>
                new()
                {
                    UnitCards = new ConcurrentStack<UnitCard>(new List<UnitCard> {deckCard}),
                    UpgradeCards = new ConcurrentStack<UpgradeCard>(new List<UpgradeCard>())
                };
        }

        [Test]
        public void AddRerolledUnitCardsToDeck()
        {
            var cardNameInHand = "Card in Hand";
            var anotherCardNameInHandThatWillBeRerolled = "Card in Hand";
            var deckCard = UnitCardMother.Create("Deck card");
            var unitCardInHand = GivenUnitCard(cardNameInHand);
            var unitCardThatWillBeRerolled = GivenUnitCard(anotherCardNameInHandThatWillBeRerolled);

            var deck = ADeckWithAUnitCard();

            var match = AMatch(
                withPlayerHands: APlayerHandWithMultipleUnits(unitCardInHand, unitCardThatWillBeRerolled),
                withDeck: deck);
                        
            GivenMatch(match);
            WhenReroll(ARerollInfoDto(new List<string> {anotherCardNameInHandThatWillBeRerolled}, new List<string>()));
            
            Assert.Contains(unitCardThatWillBeRerolled, match.Board.Deck.UnitCards);

            Deck ADeckWithAUnitCard() =>
                new()
                {
                    UnitCards = new ConcurrentStack<UnitCard>(new List<UnitCard> {deckCard}),
                    UpgradeCards = new ConcurrentStack<UpgradeCard>(new List<UpgradeCard>())
                };
        }

        [Test]
        public void NotChangeNotRerolledUpgradeHandCards()
        {
            var cardNameInHand = "Card in Hand";
            var anotherCardNameInHandThatWillBeRerolled = "Card in Hand";
            var deckCard = UpgradeCardMother.Create("Deck card");
            var upgradeCardInHand = GivenUpgradeCard(cardNameInHand);
            var upgradeCardThatWillBeRerolled = GivenUpgradeCard(anotherCardNameInHandThatWillBeRerolled);

            var deck = ADeckWithAUpgradeCard();

            var match = AMatch(
                withPlayerHands: PlayerHandsWithMultipleUpgradeCards(upgradeCardInHand, upgradeCardThatWillBeRerolled),
                withDeck: deck);
                        
            GivenMatch(match);
            WhenReroll(ARerollInfoDto(new List<string>(),new List<string> {anotherCardNameInHandThatWillBeRerolled}));
            
            Assert.Contains(upgradeCardInHand, match.Board.PlayersHands[UserId].UpgradeCards.ToList());

            Deck ADeckWithAUpgradeCard() =>
                new()
                {
                    UnitCards = new ConcurrentStack<UnitCard>(new List<UnitCard>()),
                    UpgradeCards = new ConcurrentStack<UpgradeCard>(new List<UpgradeCard>(){ deckCard})
                };
        }

        [Test]
        public void ChangeRerolledUpgradeHandCards()
        {
            var cardNameInHand = "Card in Hand";
            var anotherCardNameInHandThatWillBeRerolled = "Card in Hand";
            var deckCard = UpgradeCardMother.Create("Deck card");
            var upgradeCardInHand = GivenUpgradeCard(cardNameInHand);
            var upgradeCardThatWillBeRerolled = GivenUpgradeCard(anotherCardNameInHandThatWillBeRerolled);

            var deck = ADeckWithAUpgradeCard();

            var match = AMatch(
                withPlayerHands:PlayerHandsWithMultipleUpgradeCards(upgradeCardInHand, upgradeCardThatWillBeRerolled),
                withDeck: deck);
                        
            GivenMatch(match);
            WhenReroll(ARerollInfoDto(new List<string>(),new List<string> {anotherCardNameInHandThatWillBeRerolled}));
            
            Assert.Contains(upgradeCardInHand, match.Board.PlayersHands[UserId].UpgradeCards.ToList());
            Assert.Contains(deckCard, match.Board.PlayersHands[UserId].UpgradeCards.ToList());
            Assert.IsFalse(match.Board.PlayersHands[UserId].UpgradeCards
                .Contains(upgradeCardThatWillBeRerolled));
            
            Deck ADeckWithAUpgradeCard() =>
                new()
                {
                    UnitCards = new ConcurrentStack<UnitCard>(new List<UnitCard>()),
                    UpgradeCards = new ConcurrentStack<UpgradeCard>(new List<UpgradeCard>(){ deckCard})
                };
        }

        [Test]
        public void AddRerolledUpgradeCardsToDeck()
        {
            var cardNameInHand = "Card in Hand";
            var anotherCardNameInHandThatWillBeRerolled = "Card in Hand";
            var deckCard = UpgradeCardMother.Create("Deck card");
            var upgradeCardInHand = GivenUpgradeCard(cardNameInHand);
            var upgradeCardThatWillBeRerolled = GivenUpgradeCard(anotherCardNameInHandThatWillBeRerolled);

            var deck = ADeckWithAUpgradeCard();
            
            var match = AMatch(
                withPlayerHands:PlayerHandsWithMultipleUpgradeCards(upgradeCardInHand, upgradeCardThatWillBeRerolled),
                withDeck: deck);
                        
            GivenMatch(match);
            WhenReroll(ARerollInfoDto(new List<string>(),new List<string> {anotherCardNameInHandThatWillBeRerolled}));
            
            Assert.Contains(upgradeCardThatWillBeRerolled, match.Board.Deck.UpgradeCards);

            Deck ADeckWithAUpgradeCard() =>
                new()
                {
                    UnitCards = new ConcurrentStack<UnitCard>(new List<UnitCard>()),
                    UpgradeCards = new ConcurrentStack<UpgradeCard>(new List<UpgradeCard>(){ deckCard})
                };
        }

        [Test]
        public void ChangeMatchStateIfAllPlayersFinishedRerolling()
        {
            var playerRerolls= new Dictionary<string, bool>(){{UserId, false}};
            var unitCardName = "Card in Hand - Unit";
            var upgradeCardName = "Card in Hand - Upgrade";
            var unitCardThatWillBeRerolled = GivenUnitCard(unitCardName);
            var upgradeCardThatWillBeRerolled = GivenUpgradeCard(upgradeCardName);
            
            var unitDeckCard = UnitCardMother.Create("Deck card");
            var upgradeDeckCard = UpgradeCardMother.Create("Deck card");
            var deck = ADeckWithCards();

            var match = AMatch(
                withPlayerHands: PlayerHandWithCards(unitCardThatWillBeRerolled, upgradeCardThatWillBeRerolled),
                withDeck: deck, 
                withPlayersReroll:playerRerolls);
                        
            GivenMatch(match);
            WhenReroll(ARerollInfoDto(new List<string> {unitCardName}, new List<string> {upgradeCardName}));
            
            Assert.AreEqual(RoundState.Upgrade,match.Board.CurrentRound!.RoundState);

            Deck ADeckWithCards() =>
                new()
                {
                    UnitCards = new ConcurrentStack<UnitCard>(new List<UnitCard> {unitDeckCard}),
                    UpgradeCards = new ConcurrentStack<UpgradeCard>(new List<UpgradeCard> {upgradeDeckCard})
                };
        }

        private static ServerMatch AMatch(RoundState withRoundState = RoundState.Reroll,
            Dictionary<string, PlayerCard> withPlayerCards = null,
            IDictionary<string, bool> withPlayersReroll = null,
            Deck withDeck = null,
            IDictionary<string, Hand> withPlayerHands = null) =>
            ServerMatchMother.Create(MatchId, withBoard:
                BoardMother.Create(withRoundsPlayed: new List<Round>(),
                    withCurrentRound:RoundMother.Create(withUsers: new List<string> {UserId},
                        withRoundState: withRoundState,
                        withPlayerCards: withPlayerCards,
                        withPlayerReroll: withPlayersReroll ??= new Dictionary<string, bool>() {{UserId, false}}),
                    withDeck: withDeck,
                    withPlayerHands: withPlayerHands ??= new Dictionary<string, Hand>() {{UserId, new Hand()}}),
                withUsers: new List<User>() {UserMother.Create(UserId)}
            );

        private UnitCard GivenUnitCard(string cardNameInHand)
        {
            var unitCard = UnitCardMother.Create(cardNameInHand);
            _getUnitCard.Execute(cardNameInHand).Returns(unitCard);
            return unitCard;
        }

        private UpgradeCard GivenUpgradeCard(string cardNameInHand)
        {
            var upgradeCard = UpgradeCardMother.Create(cardNameInHand);
            _getUpgradeCard.Execute(cardNameInHand).Returns(upgradeCard);
            return upgradeCard;
        }

        private RerollInfoDto ARerollInfoDto(List<string> units, List<string> upgrades) =>
            new RerollInfoDto() {unitCards = units, upgradeCards = upgrades};
        private static Dictionary<string, Hand> PlayerHandsWithUnit(UnitCard unitCardInHand)
        {
            var unit = new Dictionary<string, Hand>();
            unit.Add(UserId, new Hand {UnitsCards = new List<UnitCard> {unitCardInHand}});
            return unit;
        }
        
        private static Dictionary<string, Hand> PlayerHandsWithUpgradeCard(UpgradeCard upgradeCardInHand)
        {
            var card = new Dictionary<string, Hand>();
            card.Add(UserId, new Hand {UpgradeCards = new List<UpgradeCard> {upgradeCardInHand}});
            return card;
        }
        
        private static Dictionary<string, Hand> PlayerHandWithCards(UnitCard unitCardThatWillBeRerolled, UpgradeCard upgradeCardThatWillBeRerolled)
        {
            var cards = new Dictionary<string, Hand>();
            cards.Add(UserId, new Hand
            {
                UnitsCards = new List<UnitCard> {unitCardThatWillBeRerolled},
                UpgradeCards = new List<UpgradeCard> {upgradeCardThatWillBeRerolled}
            });
            return cards;
        }
        
        private static Dictionary<string, Hand> APlayerHandWithMultipleUnits(UnitCard unitCardInHand, UnitCard unitCardThatWillBeRerolled)
        {
            return new Dictionary<string, Hand>
            {{UserId, new Hand
            {
                UnitsCards = new List<UnitCard> {unitCardInHand, unitCardThatWillBeRerolled},
                UpgradeCards = new List<UpgradeCard>()
            }}};
        }

        private static Dictionary<string, Hand> PlayerHandsWithMultipleUpgradeCards(UpgradeCard upgradeCardInHand, UpgradeCard upgradeCardThatWillBeRerolled)
        {
            return new Dictionary<string, Hand>
            {{UserId, new Hand
            {
                UnitsCards = new List<UnitCard>() ,
                UpgradeCards = new List<UpgradeCard>{upgradeCardInHand, upgradeCardThatWillBeRerolled}
            }}};
        }

        private void GivenMatch(ServerMatch match) => _matchesRepository.Get(MatchId).Returns(match);
        private void WhenReroll(RerollInfoDto rerollInfoDto = null) =>
            _playReroll.Execute(MatchId, UserId, rerollInfoDto ??= new RerollInfoDto());

        private void ThenThrows(TestDelegate when) => Assert.Throws<ApplicationException>(when);
    }
}