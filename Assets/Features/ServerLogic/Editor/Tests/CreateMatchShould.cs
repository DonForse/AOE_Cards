using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Domain;
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
        private CreateMatch createMatch;
        private IMatchesRepository _matchesRepository;
        private ICardRepository _cardRepository;

        [SetUp]
        public void Setup()
        {
            _matchesRepository = Substitute.For<IMatchesRepository>();
            _cardRepository = Substitute.For<ICardRepository>();
            createMatch = new CreateMatch(_matchesRepository, _cardRepository);
        }

        [Test]
        public void AddMatchToMatchRepository()
        {
            WhenCreateMatchWihtOneUser();
            ThenMatchRepositoryReceived(Arg.Any<ServerMatch>());
        }

        [Test]
        public void AddMatchWithUsers()
        {
            WhenCreateMatchWihtOneUser();
            ThenMatchRepositoryReceived(Arg.Is<ServerMatch>(x=>x.Users.Any(x=>x.Id == "Id" && x.UserName == "userName")));
        }
        
        [Test]
        public void AddMatchWithUnitCards()
        {
            Assert.Fail();
        }

        [Test]
        public void AddMatchWithUpgradeCards()
        {
            Assert.Fail();
        }
        
        private static List<User> AListOfUsersWithOneUser() => new(){new User(){ Id = "Id", Password = "password", UserName = "userName"}};
        private void WhenCreateMatchWihtOneUser() => createMatch.Execute(AListOfUsersWithOneUser(), false, 0);
        private void ThenMatchRepositoryReceived(ServerMatch serverMatch) => _matchesRepository.Received(1).Add(serverMatch);

        //with users
        //bot
        //etc
    }
}
