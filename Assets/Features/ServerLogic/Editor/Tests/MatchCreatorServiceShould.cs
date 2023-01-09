using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Matches.Service;
using Features.ServerLogic.Users.Actions;
using NUnit.Framework;

namespace Features.ServerLogic.Editor.Tests
{
    public class MatchCreatorServiceShould
    {

        MatchCreatorService matchCreatorService;
        private IUsersQueuedRepository _usersQueuedRepository;
        private IFriendsUsersQueuedRepository _friendsQueuedRepository;
        private IDequeueMatch _dequeueMatch;
        private IEnqueueMatch _enqueueMatch;
        private IDequeueFriendMatch _dequeueFriendMatch;
        private IGetUserMatch _getUserMatch;
        private IGetUser _getUser;
        private ICreateRound _createRound;
        private ICreateMatch _createMatch;

        [SetUp]
        public void Setup()
        {
            matchCreatorService = new MatchCreatorService(_usersQueuedRepository,_friendsQueuedRepository, 
                _dequeueMatch,_enqueueMatch, _dequeueFriendMatch,_getUser,_createMatch, _createRound);
        }

        [Test]
        public void asd()
        {
         Assert.Fail();   
        }
    }
}