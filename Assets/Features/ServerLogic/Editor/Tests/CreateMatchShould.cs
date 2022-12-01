using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Service;
using NSubstitute;
using NUnit.Framework;
using ServerLogic.Cards.Domain.Units;
using ServerLogic.Cards.Domain.Upgrades;
using ServerLogic.Cards.Infrastructure;
using ServerLogic.Matches.Infrastructure;
using ServerLogic.Users.Domain;

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

        [SetUp]
        public void Setup()
        {
            _matchesRepository = Substitute.For<IMatchesRepository>();
            _cardRepository = Substitute.For<ICardRepository>();
            _serverConfiguration = Substitute.For<IServerConfiguration>();
            createMatch = new CreateMatch(_matchesRepository, _cardRepository, _serverConfiguration);
        }

        [Test]
        public void AddMatchToMatchRepository()
        {
            GivenCardRepositoryWithCards();
            _serverConfiguration.GetAmountUnitCardsForPlayers().Returns(1);
            _serverConfiguration.GetAmountUpgradeCardForPlayers().Returns(1);
            WhenCreateMatchWithOneUser();
            ThenMatchRepositoryReceived(Arg.Any<ServerMatch>());
        }

        [Test]
        public void AddMatchWithUsers()
        {
            GivenCardRepositoryWithCards();
            _serverConfiguration.GetAmountUnitCardsForPlayers().Returns(1);
            _serverConfiguration.GetAmountUpgradeCardForPlayers().Returns(1);
            WhenCreateMatchWithOneUser();
            ThenMatchRepositoryReceived(Arg.Is<ServerMatch>(x =>
                x.Users.Any(x => x.Id == UserIdOne && x.UserName == UserNameOne)));
        }

        [Test]
        public void AddMatchWithUnitCards()
        {
            GivenCardRepositoryWithCards();
            _serverConfiguration.GetAmountUnitCardsForPlayers().Returns(1);
            _serverConfiguration.GetAmountUpgradeCardForPlayers().Returns(1);
            
            WhenCreateMatchWithTwoUsers();
            _matchesRepository.Received(1).Add(Arg.Is<ServerMatch>(serverMatch => ThenServerMatchContainsExpectedUnitCards(serverMatch)));
            
        }

        [Test]
        public void AddMatchWithUpgradeCards()
        {
            GivenCardRepositoryWithCards();
            _serverConfiguration.GetAmountUnitCardsForPlayers().Returns(1);
            _serverConfiguration.GetAmountUpgradeCardForPlayers().Returns(1);
            
            WhenCreateMatchWithTwoUsers();
            _matchesRepository.Received(1).Add(Arg.Is<ServerMatch>(serverMatch => ThenServerMatchContainsExpectedUpgradeCards(serverMatch)));
        }

        private static List<User> AListOfUsersWithOneUser() => new()
            {new User {Id = UserIdOne, Password = "password", UserName = UserNameOne}};

        private static List<User> AListOfUsersWithTwoUsers() => new()
        {
            new User {Id = UserIdOne, Password = "password", UserName = UserNameOne},
            new User {Id = UserIdTwo, Password = "password", UserName = UserNameTwo}
        };

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
                new UpgradeCard() {CardName = "upgrade"},
                new UpgradeCard() {CardName = "upgrade-1"},
                new UpgradeCard() {CardName = "upgrade-2"},
                new UpgradeCard() {CardName = "upgrade-3"},
            };
            _cardRepository.GetUnitCards().Returns(unitCards);
            _cardRepository.GetUpgradeCards().Returns(upgradeCards);
        }

        private void WhenCreateMatchWithOneUser() => createMatch.Execute(AListOfUsersWithOneUser(), true, 0);

        private void WhenCreateMatchWithTwoUsers() => createMatch.Execute(AListOfUsersWithTwoUsers(), false, 0);

        private void ThenMatchRepositoryReceived(ServerMatch serverMatch) =>
            _matchesRepository.Received(1).Add(serverMatch);


        bool ThenServerMatchContainsExpectedUnitCards(ServerMatch sm) => sm.Board.Deck.UnitCards.Count == 1
                                                                         && sm.Board.PlayersHands[UserIdOne].UnitsCards.Count == 2
                                                                         && sm.Board.PlayersHands[UserIdTwo].UnitsCards.Count == 2;
        
        bool ThenServerMatchContainsExpectedUpgradeCards(ServerMatch sm) => sm.Board.Deck.UpgradeCards.Count == 1
                                                                         && sm.Board.PlayersHands[UserIdOne].UpgradeCards.Count == 1
                                                                         && sm.Board.PlayersHands[UserIdTwo].UpgradeCards.Count == 1 
                                                                         && sm.Board.RoundsPlayed.First().RoundUpgradeCard.CardName != string.Empty;
        //with users
        //bot
        //etc
    }
}