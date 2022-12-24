using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Actions;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Cards.Infrastructure;
using Features.ServerLogic.Editor.Tests.Mothers;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure.DTO;
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


        [SetUp]
        public void Setup()
        {
            _getUnitCard = Substitute.For<IGetUnitCard>();
            _getUpgradeCard = Substitute.For<IGetUpgradeCard>();
            _playReroll = new PlayReroll(_getUnitCard, _getUpgradeCard);
        }

        [Test]
        public void ThrowsWhenNoRounds()
        {
            var match = ServerMatchMother.Create(MatchId, withBoard: BoardMother.Create());
            ThenThrows(() => WhenReroll(match));
        }

        [Test]
        public void ThrowsWhenNotInReroll()
        {
            var match = AMatch(RoundState.Unit);
            ThenThrows(() => WhenReroll(match));
        }

        [Test]
        public void ThrowsWhenUpgradeCardAlreadyPlayedForThatRound()
        {
            var match = AMatch(withPlayerCards: new Dictionary<string, PlayerCard>()
            {
                {UserId, new PlayerCard() {UpgradeCard = UpgradeCardMother.Create("some card")}}
            });
            ThenThrows(() => WhenReroll(match));
        }

        [Test]
        public void ThrowsWhenPlayerCannotReroll()
        {
            var match = AMatch(withPlayersReroll: new Dictionary<string, bool>() {{UserId, true}});
            ThenThrows(() => WhenReroll(match));
        }

        [Test]
        public void ThrowsWhenUnitCardSelectedForRerollDoesNotExists()
        {
            var cardName = "some card";
            _getUnitCard.Execute(cardName).Returns((UnitCard) null);
            var match = AMatch();
            ThenThrows(() => WhenReroll(match, ARerollInfoDto(new List<string> {cardName}, new List<string>())));
        }

        [Test]
        public void ThrowsWhenUnitCardSelectedForRerollIsNotInPlayerHand()
        {
            var rerolledCardName = "Rerolled Card";
            var cardNameInHand = "Card in Hand";
            var unitCardInHand = UnitCardMother.Create(cardNameInHand);
            _getUnitCard.Execute(cardNameInHand).Returns(unitCardInHand);
            _getUnitCard.Execute(rerolledCardName).Returns(UnitCardMother.Create(rerolledCardName));

            var match = AMatch(withPlayerHands: new Dictionary<string, Hand>
            {
                { UserId, new Hand {UnitsCards = new List<UnitCard> {unitCardInHand}} }
            });
            ThenThrows(() =>
                WhenReroll(match, ARerollInfoDto(new List<string> {rerolledCardName}, new List<string>())));
        }

        [Test]
        public void ThrowsWhenUpgradeCardSelectedForRerollDoesNotExists()
        {
            var cardName = "some card";
            _getUpgradeCard.Execute(cardName).Returns((UpgradeCard) null);
            var match = AMatch();
            ThenThrows(() => WhenReroll(match, ARerollInfoDto(new List<string>(), new List<string>() {cardName})));
        }


        [Test]
        public void ThrowsWhenUpgradeCardSelectedForRerollIsNotInPlayerHand()
        {
            var rerolledCardName = "Rerolled Card";
            var cardNameInHand = "Card in Hand";
            var upgradeCardInHand = UpgradeCardMother.Create(cardNameInHand);
            _getUpgradeCard.Execute(cardNameInHand).Returns(upgradeCardInHand);
            _getUpgradeCard.Execute(rerolledCardName).Returns(UpgradeCardMother.Create(rerolledCardName));

            var match = AMatch(withPlayerHands: new Dictionary<string, Hand>
            {
                {UserId, new Hand {UpgradeCards = new List<UpgradeCard> {upgradeCardInHand}}}
            });
            ThenThrows(() =>
                WhenReroll(match, ARerollInfoDto(new List<string>() , new List<string> {rerolledCardName})));
        }

        [Test]
        public void IgnoreVillagerCardReroll()
        {
            var playerRerolls= new Dictionary<string, bool>(){{UserId, false}};
            var unitCardName = "Villager";
            var upgradeCardName = "Card in Hand - Upgrade";
            var unitCardThatWillBeRerolled = UnitCardMother.Create(unitCardName);
            var upgradeCardThatWillBeRerolled = UpgradeCardMother.Create(upgradeCardName);

            _getUnitCard.Execute(unitCardName).Returns(unitCardThatWillBeRerolled);
            _getUpgradeCard.Execute(upgradeCardName).Returns(upgradeCardThatWillBeRerolled);

            var unitDeckCard = UnitCardMother.Create("Deck card");
            var upgradeDeckCard = UpgradeCardMother.Create("Deck card");
            var deck = ADeckWithCards();

            var match = AMatch(
                withPlayerHands: new Dictionary<string, Hand>
                {
                    {
                        UserId, new Hand
                        {
                            UnitsCards = new List<UnitCard> {unitCardThatWillBeRerolled},
                            UpgradeCards = new List<UpgradeCard> {upgradeCardThatWillBeRerolled}
                        }
                    }
                },
                withDeck: deck, 
                withPlayersReroll:playerRerolls);
            WhenReroll(match, ARerollInfoDto(new List<string> {unitCardName}, new List<string> {upgradeCardName}));

            
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
            var unitCardThatWillBeRerolled = UnitCardMother.Create(unitCardName);
            var upgradeCardThatWillBeRerolled = UpgradeCardMother.Create(upgradeCardName);

            _getUnitCard.Execute(unitCardName).Returns(unitCardThatWillBeRerolled);
            _getUpgradeCard.Execute(upgradeCardName).Returns(upgradeCardThatWillBeRerolled);

            var unitDeckCard = UnitCardMother.Create("Deck card");
            var upgradeDeckCard = UpgradeCardMother.Create("Deck card");
            var deck = ADeckWithCards();

            var match = AMatch(
                withPlayerHands: new Dictionary<string, Hand>
                {
                    {
                        UserId, new Hand
                        {
                            UnitsCards = new List<UnitCard> {unitCardThatWillBeRerolled},
                            UpgradeCards = new List<UpgradeCard> {upgradeCardThatWillBeRerolled}
                        }
                    }
                },
                withDeck: deck, 
                withPlayersReroll:playerRerolls);
            WhenReroll(match, ARerollInfoDto(new List<string> {unitCardName}, new List<string> {upgradeCardName}));

            
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
            var unitCardInHand = UnitCardMother.Create(cardNameInHand);
            var unitCardThatWillBeRerolled = UnitCardMother.Create(anotherCardNameInHandThatWillBeRerolled);

            var deck = ADeckWithAUnitCard();

            _getUnitCard.Execute(cardNameInHand).Returns(unitCardInHand);
            _getUnitCard.Execute(anotherCardNameInHandThatWillBeRerolled).Returns(unitCardThatWillBeRerolled);

            var match = AMatch(
                withPlayerHands: new Dictionary<string, Hand>
                    {{UserId, new Hand
                    {
                        UnitsCards = new List<UnitCard> {unitCardInHand, unitCardThatWillBeRerolled},
                        UpgradeCards = new List<UpgradeCard>()
                    }}},
                withDeck: deck);
            WhenReroll(match, ARerollInfoDto(new List<string> {anotherCardNameInHandThatWillBeRerolled}, new List<string>()));
            
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
            var unitCardInHand = UnitCardMother.Create(cardNameInHand);
            var unitCardThatWillBeRerolled = UnitCardMother.Create(anotherCardNameInHandThatWillBeRerolled);

            var deck = ADeckWithAUnitCard();

            _getUnitCard.Execute(cardNameInHand).Returns(unitCardInHand);
            _getUnitCard.Execute(anotherCardNameInHandThatWillBeRerolled).Returns(unitCardThatWillBeRerolled);

            var match = AMatch(
                withPlayerHands: new Dictionary<string, Hand>
                {{UserId, new Hand
                {
                    UnitsCards = new List<UnitCard> {unitCardInHand, unitCardThatWillBeRerolled},
                    UpgradeCards = new List<UpgradeCard>()
                }}},
                withDeck: deck);
            WhenReroll(match, ARerollInfoDto(new List<string> {anotherCardNameInHandThatWillBeRerolled}, new List<string>()));
            
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
            var unitCardInHand = UnitCardMother.Create(cardNameInHand);
            var unitCardThatWillBeRerolled = UnitCardMother.Create(anotherCardNameInHandThatWillBeRerolled);

            var deck = ADeckWithAUnitCard();

            _getUnitCard.Execute(cardNameInHand).Returns(unitCardInHand);
            _getUnitCard.Execute(anotherCardNameInHandThatWillBeRerolled).Returns(unitCardThatWillBeRerolled);

            var match = AMatch(
                withPlayerHands: new Dictionary<string, Hand>
                {{UserId, new Hand
                {
                    UnitsCards = new List<UnitCard> {unitCardInHand, unitCardThatWillBeRerolled},
                    UpgradeCards = new List<UpgradeCard>()
                }}},
                withDeck: deck);
            WhenReroll(match, ARerollInfoDto(new List<string> {anotherCardNameInHandThatWillBeRerolled}, new List<string>()));
            
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
            var upgradeCardInHand = UpgradeCardMother.Create(cardNameInHand);
            var upgradeCardThatWillBeRerolled = UpgradeCardMother.Create(anotherCardNameInHandThatWillBeRerolled);

            var deck = ADeckWithAUpgradeCard();

            _getUpgradeCard.Execute(cardNameInHand).Returns(upgradeCardInHand);
            _getUpgradeCard.Execute(anotherCardNameInHandThatWillBeRerolled).Returns(upgradeCardThatWillBeRerolled);

            var match = AMatch(
                withPlayerHands: new Dictionary<string, Hand>
                {{UserId, new Hand
                {
                    UnitsCards = new List<UnitCard>() ,
                    UpgradeCards = new List<UpgradeCard>{upgradeCardInHand, upgradeCardThatWillBeRerolled}
                }}},
                withDeck: deck);
            WhenReroll(match, ARerollInfoDto(new List<string>(),new List<string> {anotherCardNameInHandThatWillBeRerolled}));
            
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
            var upgradeCardInHand = UpgradeCardMother.Create(cardNameInHand);
            var upgradeCardThatWillBeRerolled = UpgradeCardMother.Create(anotherCardNameInHandThatWillBeRerolled);

            var deck = ADeckWithAUpgradeCard();

            _getUpgradeCard.Execute(cardNameInHand).Returns(upgradeCardInHand);
            _getUpgradeCard.Execute(anotherCardNameInHandThatWillBeRerolled).Returns(upgradeCardThatWillBeRerolled);

            var match = AMatch(
                withPlayerHands: new Dictionary<string, Hand>
                {{UserId, new Hand
                {
                    UnitsCards = new List<UnitCard>() ,
                    UpgradeCards = new List<UpgradeCard>{upgradeCardInHand, upgradeCardThatWillBeRerolled}
                }}},
                withDeck: deck);
            WhenReroll(match, ARerollInfoDto(new List<string>(),new List<string> {anotherCardNameInHandThatWillBeRerolled}));
            
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
            var upgradeCardInHand = UpgradeCardMother.Create(cardNameInHand);
            var upgradeCardThatWillBeRerolled = UpgradeCardMother.Create(anotherCardNameInHandThatWillBeRerolled);

            var deck = ADeckWithAUpgradeCard();

            _getUpgradeCard.Execute(cardNameInHand).Returns(upgradeCardInHand);
            _getUpgradeCard.Execute(anotherCardNameInHandThatWillBeRerolled).Returns(upgradeCardThatWillBeRerolled);

            var match = AMatch(
                withPlayerHands: new Dictionary<string, Hand>
                {{UserId, new Hand
                {
                    UnitsCards = new List<UnitCard>() ,
                    UpgradeCards = new List<UpgradeCard>{upgradeCardInHand, upgradeCardThatWillBeRerolled}
                }}},
                withDeck: deck);
            WhenReroll(match, ARerollInfoDto(new List<string>(),new List<string> {anotherCardNameInHandThatWillBeRerolled}));
            
            Assert.Contains(upgradeCardThatWillBeRerolled, match.Board.Deck.UpgradeCards);

            Deck ADeckWithAUpgradeCard() =>
                new()
                {
                    UnitCards = new ConcurrentStack<UnitCard>(new List<UnitCard>()),
                    UpgradeCards = new ConcurrentStack<UpgradeCard>(new List<UpgradeCard>(){ deckCard})
                };

        }

        private static ServerMatch AMatch(RoundState withRoundState = RoundState.Reroll,
            Dictionary<string, PlayerCard> withPlayerCards = null,
            IDictionary<string, bool> withPlayersReroll = null,
            Deck withDeck = null,
            IDictionary<string, Hand> withPlayerHands = null) =>
            ServerMatchMother.Create(MatchId, withBoard:
                BoardMother.Create(withRoundsPlayed:
                    new List<Round>()
                    {
                        RoundMother.Create(withUsers: new List<string> {UserId},
                            withRoundState: withRoundState,
                            withPlayerCards: withPlayerCards,
                            withPlayerReroll: withPlayersReroll ??= new Dictionary<string, bool>() {{UserId, false}}),
                    },
                    withDeck: withDeck,
                    withPlayerHands: withPlayerHands ??= new Dictionary<string, Hand>() {{UserId, new Hand()}}));

        private RerollInfoDto ARerollInfoDto(List<string> units, List<string> upgrades) =>
            new RerollInfoDto() {unitCards = units, upgradeCards = upgrades};

        private void WhenReroll(ServerMatch match, RerollInfoDto rerollInfoDto = null) =>
            _playReroll.Execute(match, UserId, rerollInfoDto ??= new RerollInfoDto());

        private void ThenThrows(TestDelegate when) => Assert.Throws<ApplicationException>(when);

        /*
         var round = serverMatch.Board.RoundsPlayed.LastOrDefault();
            if (round.RoundState != RoundState.Reroll || round.PlayerReroll.ContainsKey(userId) && round.PlayerReroll[userId] )
                throw new ApplicationException("Reroll Not Available");

            if (IsRoundAlradyPlayed(round, userId))
                throw new ApplicationException("Reroll Not Available");

            var unitCards = new List<UnitCard>();
            GetUnitCards(serverMatch, userId, cards, unitCards);

            var upgradeCards = new List<UpgradeCard>();
            GetUpgradeCards(serverMatch, userId, cards, upgradeCards);

            bool revert = false;
            List<UpgradeCard> upgrades = GetNewHandUpgrades(serverMatch, userId, upgradeCards, ref revert);
            List<UnitCard> units = GetNewHandUnits(serverMatch, userId, unitCards, ref revert);

            if (revert)
                throw new ApplicationException("Invalid Reroll");

            round.PlayerReroll[userId] = true;
            AddNewUpgradeCards(serverMatch, userId, upgradeCards, upgrades);
            AddNewUnitCards(serverMatch, userId, unitCards, units);

            if (round.PlayerReroll.Values.Count(v=>v) == 2)
                serverMatch.Board.RoundsPlayed.LastOrDefault().ChangeRoundState(RoundState.Upgrade);
         */
    }
}