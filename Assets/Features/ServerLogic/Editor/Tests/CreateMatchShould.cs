using System;
using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Cards.Infrastructure;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Matches.Service;
using Features.ServerLogic.Users.Actions;
using Features.ServerLogic.Users.Domain;
using NSubstitute;
using NUnit.Framework;

namespace Features.ServerLogic.Editor.Tests
{
    public class CreateMatchShould
    {
        private const string UserIdOne = "Id";
        private const string UserIdTwo = "Id-2";
        private const string UserNameOne = "userName";
        private const string UserNameTwo = "userName-2";
        private CreateMatch createMatch;
        private IMatchesRepository _matchesRepository;
        private ICardRepository _cardRepository;
        private IServerConfiguration _serverConfiguration;
        private ICreateBotUser _createBotUser;
        private IUserMatchesRepository _userMatchesRepository;

        [SetUp]
        public void Setup()
        {
            _matchesRepository = Substitute.For<IMatchesRepository>();
            _cardRepository = Substitute.For<ICardRepository>();
            _serverConfiguration = Substitute.For<IServerConfiguration>();
            _createBotUser = Substitute.For<ICreateBotUser>();
            _userMatchesRepository = Substitute.For<IUserMatchesRepository>();
            createMatch = new CreateMatch(_matchesRepository, _cardRepository,
                _serverConfiguration, _createBotUser,_userMatchesRepository);
        }
        
        [Test]
        public void AddMatchToMatchRepository()
        {
            GivenCardRepositoryWithCards();
            GivenCreateBotUserReturns();
            GivenAmountOfUnitCardsForPlayerIs(1);
            GivenAmountOfUpgradeCardsForPlayersIs(1);
            WhenCreateMatchWithOneUser();
            ThenMatchRepositoryReceived(Arg.Any<ServerMatch>());
            ThenUserMatchIsAddedWithOneUser();
        }
        
        [Test]
        public void FailIfCreateBotMatchWithAlreadyTwoUsers()
        {
            ThenThrowsApplicationException(WhenCreateMatchWithTwoUsersAndABot);
            void ThenThrowsApplicationException(TestDelegate expr) => Assert.Throws<ApplicationException>(expr);
        }

        [Test]
        public void AddMatchWithBot()
        {
            GivenCardRepositoryWithCards();
            GivenAmountOfUnitCardsForPlayerIs(1);
            GivenAmountOfUpgradeCardsForPlayersIs(1);
            GivenCreateBotUserReturns();
            WhenCreateMatchWithOneUser();
            _createBotUser.Received(1).Execute();
            ThenUserMatchIsAddedWithOneUser();
        }

        [Test]
        public void AddMatchWithUsers()
        {
            GivenCardRepositoryWithCards();
            GivenAmountOfUnitCardsForPlayerIs(1);
            GivenAmountOfUpgradeCardsForPlayersIs(1);
            WhenCreateMatchWithTwoUsers();
            ThenDidNotAddBot();
            ThenMatchRepositoryReceived(Arg.Is<ServerMatch>(x =>
                x.Users.Any(x => x.Id == UserIdOne && x.UserName == UserNameOne)));
            ThenUserMatchIsAddedWithTwoUsers();
        }

        [Test]
        public void AddMatchWithUnitCards()
        {
            GivenCardRepositoryWithCards();
            GivenAmountOfUnitCardsForPlayerIs(1);
            GivenAmountOfUpgradeCardsForPlayersIs(1);
            WhenCreateMatchWithTwoUsers();
            ThenMatchRepositoryReceived(Arg.Is<ServerMatch>(serverMatch =>
                    ThenServerMatchContainsExpectedUnitCards(serverMatch) 
                    && ThenServerMatchContainsHandsWithVillagerCards(serverMatch)));
            ThenUserMatchIsAddedWithTwoUsers();
        }
        
        [Test]
        public void AddMatchWithUpgradeCards()
        {
            GivenCardRepositoryWithCards();
            GivenAmountOfUnitCardsForPlayerIs(1);
            GivenAmountOfUpgradeCardsForPlayersIs(1);
            
            WhenCreateMatchWithTwoUsers();
            _matchesRepository.Received(1)
                .Add(Arg.Is<ServerMatch>(serverMatch => ThenServerMatchContainsExpectedUpgradeCards(serverMatch)));
            ThenUserMatchIsAddedWithTwoUsers();
        }

        private void ThenUserMatchIsAddedWithTwoUsers()
        {
            Received.InOrder(() =>
            {
                _userMatchesRepository.Received(1).Add(UserIdOne, Arg.Any<string>());
                _userMatchesRepository.Received(1).Add(UserIdTwo, Arg.Any<string>());    
            });
        }

        private void ThenUserMatchIsAddedWithOneUser()
        {
            _userMatchesRepository.Received(1).Add(UserIdOne, Arg.Any<string>());
        }

        private static List<User> AListOfUsersWithOneUser() => new()
            {new User {Id = UserIdOne, Password = "password", UserName = UserNameOne}};

        private static List<User> AListOfUsersWithTwoUsers() => new()
        {
            new User {Id = UserIdOne, Password = "password", UserName = UserNameOne},
            new User {Id = UserIdTwo, Password = "password", UserName = UserNameTwo}
        };
        
        private void GivenAmountOfUpgradeCardsForPlayersIs(int amount) => _serverConfiguration.GetAmountUpgradeCardForPlayers().Returns(amount);
        private void GivenAmountOfUnitCardsForPlayerIs(int amount) => _serverConfiguration.GetAmountUnitCardsForPlayers().Returns(amount);
        private void GivenCreateBotUserReturns(User user = null) => _createBotUser.Execute().Returns(user ?? new User() {Id = "TEST"});
        private void GivenCardRepositoryWithCards()
        {
            var unitCards = new List<UnitCard>()
            {
                new UnitCard() {CardName = "villager"},
                new UnitCard() {CardName = "card-1"},
                new UnitCard() {CardName = "card-2"},
                new UnitCard() {CardName = "card-3"},
                
            };
            var upgradeCards = new List<UpgradeCard>()
            {
                new UpgradeCard("upgrade", 0, null, null),
                new UpgradeCard("upgrade-1", 0, null, null),
                new UpgradeCard("upgrade-2", 0, null, null) ,
                new UpgradeCard("upgrade-3", 0, null, null),
            };
            _cardRepository.GetUnitCards().Returns(unitCards);
            _cardRepository.GetUnitCard("villager").Returns(new UnitCard(){CardName = "villager"});
            _cardRepository.GetUpgradeCards().Returns(upgradeCards);
        }

        private void WhenCreateMatchWithOneUser() => createMatch.Execute(AListOfUsersWithOneUser(), true, 0);
        private void WhenCreateMatchWithTwoUsers() => createMatch.Execute(AListOfUsersWithTwoUsers(), false, 0);
        private void WhenCreateMatchWithTwoUsersAndABot() => createMatch.Execute(AListOfUsersWithTwoUsers(), true, 0);
        private void ThenMatchRepositoryReceived(ServerMatch serverMatch) =>
            _matchesRepository.Received(1).Add(serverMatch);
        private void ThenDidNotAddBot() => _createBotUser.DidNotReceive().Execute();
        bool ThenServerMatchContainsExpectedUnitCards(ServerMatch sm) => sm.Board.Deck.UnitCards.Count == 1
                                                                         && sm.Board.PlayersHands[UserIdOne].UnitsCards.Count == 2
                                                                         && sm.Board.PlayersHands[UserIdTwo].UnitsCards.Count == 2;

        bool ThenServerMatchContainsExpectedUpgradeCards(ServerMatch sm) => sm.Board.Deck.UpgradeCards.Count == 2
                                                                            && sm.Board.PlayersHands[UserIdOne]
                                                                                .UpgradeCards.Count == 1
                                                                            && sm.Board.PlayersHands[UserIdTwo]
                                                                                .UpgradeCards.Count == 1;

        private bool ThenServerMatchContainsHandsWithVillagerCards(ServerMatch sm) =>
            sm.Board.Deck.UnitCards.Count == 1
            && sm.Board.PlayersHands[UserIdOne].UnitsCards.Any(x => x.CardName == "villager")
            && sm.Board.PlayersHands[UserIdTwo].UnitsCards.Any(x => x.CardName == "villager");
    }
}