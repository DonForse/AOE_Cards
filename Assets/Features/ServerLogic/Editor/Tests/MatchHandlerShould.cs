using Features.ServerLogic.Handlers;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Matches.Service;
using Features.ServerLogic.Users.Infrastructure;
using NSubstitute;
using NUnit.Framework;

namespace Features.ServerLogic.Editor.Tests
{
    public class MatchHandlerShould
    {
        private const string UserId = "UserId";
        private MatchHandler _matchHandler;
        private IUsersQueuedRepository _usersQueuedRepository;
        private IFriendsUsersQueuedRepository _friendsQueuedRepository;
        private IMatchesRepository _matchesRepository;
        private IUsersRepository _usersRepository;
        private IMatchCreatorService _matchCreatorService;
        private ICreateMatch _createMatch;

        [SetUp]
        public void Setup()
        {
            _matchCreatorService = Substitute.For<IMatchCreatorService>();
            _matchHandler = new MatchHandler(_usersQueuedRepository, _friendsQueuedRepository, _matchesRepository, _usersRepository, _matchCreatorService,_createMatch );
        }

        [Test]
        public void CreateMatchesWhenGet()
        {
            _matchHandler.Get(UserId);
            _matchCreatorService.Received(1).CreateMatches();
        }
        
        [Test]
        public void ReturnNullWhenGetAndNoMatch()
        {
            _matchHandler.Get(UserId);
            _matchCreatorService.Received(1).CreateMatches();
            Assert.Fail();
        }
        
        [Test]
        public void ReturnMatchWhenGetAndMatchExists()
        {
            _matchHandler.Get(UserId);
            _matchCreatorService.Received(1).CreateMatches();
            Assert.Fail();
        }
    }
}