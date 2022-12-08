using Features.ServerLogic.Handlers;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Matches.Service;
using Features.ServerLogic.Users.Infrastructure;
using NUnit.Framework;

namespace Features.ServerLogic.Editor.Tests
{
    public class MatchHandlerShould
    {
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
            _matchHandler = new MatchHandler(_usersQueuedRepository, _friendsQueuedRepository, _matchesRepository, _usersRepository, _matchCreatorService,_createMatch );
        }

        [Test]
        public void CreateMatchesWhenGet()
        {
            
        }
    }
}