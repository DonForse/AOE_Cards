using System;
using System.Collections.Generic;
using Features.ServerLogic.Editor.Tests.Mothers;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Domain;
using NSubstitute;
using NUnit.Framework;
using ServerLogic.Cards.Domain.Upgrades;
using ServerLogic.Cards.Infrastructure;
using ServerLogic.Matches.Domain;
using ServerLogic.Matches.Infrastructure;
using ServerLogic.Users.Domain;

namespace Features.ServerLogic.Editor.Tests
{
    public class PlayUpgradeCardShould
    {
        private const string MatchId = "MatchId";
        private const string UserId = "UserId";
        private const string CardName = "CardName";
        private const string UserIdTwo = "UserId-2";
        private ICardRepository _cardRepository;
        private IMatchesRepository _matchesRepository;
        private PlayUpgradeCard _playUpgradeCard;

        [SetUp]
        public void Setup()
        {
            _matchesRepository = Substitute.For<IMatchesRepository>();
            _cardRepository = Substitute.For<ICardRepository>();
            _playUpgradeCard = new PlayUpgradeCard(_matchesRepository, _cardRepository);
        }

        [Test]
        public void AddCardToCardsPlayed()
        {
            var users = new List<string>() {UserId, UserIdTwo};
            var round = GivenRoundInUpgradeState(users);
            var upgradeCard = UpgradeCardMother.Create(CardName);
            var hand = AHand(upgradeCard);
            var match = AMatch(round, hand);
            
            GivenUpgradeCardInCardRepository(upgradeCard);
            GivenMatchRepositoryReturns(match);
            WhenExecute();
            ThenCardIsPlayed(round);
        }

        [Test]
        public void RemoveCardFromHand()
        {
            var users = new List<string>(){ UserId, UserIdTwo};
            var round = GivenRoundInUpgradeState(users);
            var upgradeCard = UpgradeCardMother.Create(CardName);
            var hand = AHand(upgradeCard);
            var match = AMatch(round, hand);
            
            GivenUpgradeCardInCardRepository(upgradeCard);
            GivenMatchRepositoryReturns(match);
            WhenExecute();
            ThenCardCountIsZero();
            void ThenCardCountIsZero() => Assert.AreEqual(0, hand.UpgradeCards.Count);
        }

        [Test]
        public void ChangeRoundStateToUnit()
        {
            var users = new List<string>(){ UserId, UserIdTwo};
            var round = GivenRoundInUpgradeState(users);
            var upgradeCard = UpgradeCardMother.Create(CardName);
            var hand = AHand(upgradeCard);
            var match = AMatch(round, hand);

            GivenUserTwoAlreadyPlayedUpgradeCard(round);
            GivenUpgradeCardInCardRepository(upgradeCard);
            GivenMatchRepositoryReturns(match);
            WhenExecute();
            ThenRoundStateIsUnit();
            void ThenRoundStateIsUnit() => Assert.AreEqual(RoundState.Unit,round.RoundState);
        }

        [Test]
        public void ThrowsWhenPlayerDontExist()
        {
            var users = new List<string>(){ UserId, UserIdTwo};
            var round = GivenRoundInUpgradeState(users);
            var upgradeCard = UpgradeCardMother.Create(CardName);
            var hand = AHand(upgradeCard);
            var match = AMatch(round, hand);

            GivenRoundPlayerCardsDoesntHaveUsers();
            GivenUpgradeCardInCardRepository(upgradeCard);
            GivenMatchRepositoryReturns(match);
            ThenThrowsException(WhenExecute);

            void GivenRoundPlayerCardsDoesntHaveUsers() => round.PlayerCards = new Dictionary<string, PlayerCard>();
            void ThenThrowsException(TestDelegate func) => Assert.Throws<ApplicationException>(func, "Player is not in Match");
        }

        [Test]
        public void ThrowsWhenPlayerHaveAlreadyPlayedUpgradeCard()
        {
            var users = new List<string>(){ UserId, UserIdTwo};
            var round = GivenRoundInUpgradeState(users);
            var upgradeCard = UpgradeCardMother.Create(CardName);
            var hand = AHand(upgradeCard);
            var match = AMatch(round, hand);

            GivenAlreadyPlayedUpgradeCard();
            GivenUpgradeCardInCardRepository(upgradeCard);
            GivenMatchRepositoryReturns(match);
            ThenThrowsException(WhenExecute);

            void ThenThrowsException(TestDelegate func) => Assert.Throws<ApplicationException>(func, "Upgrade card has already been played");
            void GivenAlreadyPlayedUpgradeCard()
            {
                round.PlayerCards = new Dictionary<string, PlayerCard>()
                {
                    {UserId, new PlayerCard(){UpgradeCard = UpgradeCardMother.Create("SomeCard")}},
                    {UserIdTwo, new PlayerCard() }
                };
            }
        }

        [Test]
        public void ThrowsRoundIsNotInUpgrade()
        {
            var users = new List<string>(){ UserId, UserIdTwo};
            var round = GivenRoundInUpgradeState(users);
            var upgradeCard = UpgradeCardMother.Create(CardName);
            var hand = AHand(upgradeCard);
            var match = AMatch(round, hand);
            
            GivenRoundInUnitState();
            GivenUpgradeCardInCardRepository(upgradeCard);
            GivenMatchRepositoryReturns(match);
            ThenThrowsException(WhenExecute);

            void ThenThrowsException(TestDelegate func) => Assert.Throws<ApplicationException>(func, "Upgrade card sent but not expecting it");
            void GivenRoundInUnitState() => round.ChangeRoundState(RoundState.Unit);
        }

        [Test]
        public void ThrowsWhenIsUpgradeCardNull()
        {
            var users = new List<string>(){ UserId, UserIdTwo};
            var round = GivenRoundInUpgradeState(users);
            var upgradeCard = UpgradeCardMother.Create(CardName);
            var hand = AHand(upgradeCard);
            var match = AMatch(round, hand);
            
            GivenMatchRepositoryReturns(match);
            ThenThrowsException(WhenExecute);

            void ThenThrowsException(TestDelegate func) => Assert.Throws<ApplicationException>(func, "Invalid Upgrade card");
        }

        [Test]
        public void ThrowsWhenCantRemoveCardFromHand()
        {
            var users = new List<string>(){ UserId, UserIdTwo};
            var round = GivenRoundInUpgradeState(users);
            var upgradeCard = UpgradeCardMother.Create(CardName);
            var hand = AHand(UpgradeCardMother.Create("Another Card"));
            var match = AMatch(round, hand);
            
            GivenUpgradeCardInCardRepository(upgradeCard);
            GivenMatchRepositoryReturns(match);
            ThenThrowsException(WhenExecute);

            void ThenThrowsException(TestDelegate func) => Assert.Throws<ApplicationException>(func, "Upgrade card is not in hand");
        }

        private static Hand AHand(UpgradeCard withUpgradeCard) => new() {UpgradeCards = new List<UpgradeCard> {withUpgradeCard}};

        private static ServerMatch AMatch(Round withRound, Hand withHand)
        {
            return new ServerMatch()
            {
                Guid = MatchId,
                Users = new List<User>(){new(){Id = UserId}, new(){Id =UserIdTwo }},
                Board = new Board()
                {
                    PlayersHands = new Dictionary<string, Hand>()
                    {
                        {UserId, withHand},
                        {UserIdTwo, new Hand()}
                    },
                    RoundsPlayed = new List<Round>() {withRound}
                }
            };
        }


        private static void GivenUserTwoAlreadyPlayedUpgradeCard(Round round)
        {
            round.PlayerCards = new Dictionary<string, PlayerCard>()
            {
                {UserId, new PlayerCard()},
                {UserIdTwo, new PlayerCard() {UpgradeCard = UpgradeCardMother.Create("SomeCard")}}
            };
        }

        private void GivenUpgradeCardInCardRepository(UpgradeCard upgradeCard) => _cardRepository.GetUpgradeCard(CardName).Returns(upgradeCard);

        private static Round GivenRoundInUpgradeState(List<string> users)
        {
            var round = new Round(users);
            round.ChangeRoundState(RoundState.Upgrade);
            return round;
        }


        private void GivenMatchRepositoryReturns(ServerMatch match) => _matchesRepository.Get(MatchId).Returns(match);
        private void WhenExecute() => _playUpgradeCard.Execute(MatchId, UserId, CardName);
        private static void ThenCardIsPlayed(Round round) => Assert.AreEqual(CardName, round.PlayerCards[UserId].UpgradeCard.CardName);
    }
}