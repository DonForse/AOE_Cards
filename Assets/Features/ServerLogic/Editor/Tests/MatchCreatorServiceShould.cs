using System;
using System.Collections.Generic;
using Features.ServerLogic.Editor.Tests.Mothers;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Matches.Service;
using Features.ServerLogic.Users.Actions;
using Features.ServerLogic.Users.Domain;
using NSubstitute;
using NUnit.Framework;

namespace Features.ServerLogic.Editor.Tests
{
    public class MatchCreatorServiceShould
    {
        private const string UserId = "UserId";
        private const string UserId2 = "UserId2";
        private const string UserId3 = "UserId3";
        private const string MatchId = "MatchId";

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

        private IServerConfiguration _serverConfiguration;

        private static User UserOne = UserMother.Create(UserId);
        private static User UserTwo = UserMother.Create(UserId2);
        private static User UserThree = UserMother.Create(UserId3);
        
        [SetUp]
        public void Setup()
        {
            _usersQueuedRepository = Substitute.For<IUsersQueuedRepository>();
            _friendsQueuedRepository = Substitute.For<IFriendsUsersQueuedRepository>();
            _dequeueMatch = Substitute.For<IDequeueMatch>();
            _enqueueMatch = Substitute.For<IEnqueueMatch>();
            _dequeueFriendMatch = Substitute.For<IDequeueFriendMatch>();
            _getUser = Substitute.For<IGetUser>();
            _createMatch = Substitute.For<ICreateMatch>();
            _createRound = Substitute.For<ICreateRound>();
            _serverConfiguration = Substitute.For<IServerConfiguration>();
            matchCreatorService = new MatchCreatorService(_usersQueuedRepository,_friendsQueuedRepository, 
                _dequeueMatch,_enqueueMatch, _dequeueFriendMatch,_getUser,_createMatch, _createRound, _serverConfiguration);
            
            _serverConfiguration.GetAmountOfPlayersInMatch().Returns(2);
        }

        [Test]
        public void CreateFriendMatchesWhenTwoUsersMatch()
        {
            _createMatch.Execute(Arg.Any<List<User>>(), false).Returns(ServerMatchMother.Create(MatchId));
            _getUser.Execute(UserId).Returns(UserOne);
            _getUser.Execute(UserId2).Returns(UserTwo);
            var friendsEnqueued = new Dictionary<string, string>() {{UserId, UserId2}, {UserId2, UserId}};
            _friendsQueuedRepository.GetKeys().Returns(friendsEnqueued);

            matchCreatorService.CreateMatches();
            Received.InOrder(() =>
            {
                _dequeueFriendMatch.Received(1).Execute(UserOne);
                _dequeueFriendMatch.Received(1).Execute(UserTwo);
                _createMatch.Received(1).Execute(Arg.Is<List<User>>(x => x.Contains(UserOne) && x.Contains(UserTwo)),
                    false);
                _createRound.Received(1).Execute(MatchId);    
            });
        }

        [Test]
        public void NotCreateFriendMatchesWhenTwoUsersDontMatch()
        {
            _createMatch.Execute(Arg.Any<List<User>>(), false).Returns(ServerMatchMother.Create(MatchId));
            _getUser.Execute(UserId).Returns(UserOne);
            _getUser.Execute(UserId2).Returns(UserTwo);
            var friendsEnqueued = new Dictionary<string, string>() {{UserId, UserId2}, {UserId2, UserId3}};
            _friendsQueuedRepository.GetKeys().Returns(friendsEnqueued);

            matchCreatorService.CreateMatches();
            _dequeueFriendMatch.DidNotReceive().Execute(UserOne);
            _dequeueFriendMatch.DidNotReceive().Execute(UserTwo);
            _createMatch.DidNotReceive().Execute(Arg.Is<List<User>>(x => x.Contains(UserOne) && x.Contains(UserTwo)),
                false);
            _createRound.DidNotReceive().Execute(MatchId);
        }

        [Test]
        public void NotCreateFriendMatchesWhenNotEnoughUsers()
        {
            _createMatch.Execute(Arg.Any<List<User>>(), false).Returns(ServerMatchMother.Create(MatchId));
            _getUser.Execute(UserId).Returns(UserOne);
            var friendsEnqueued = new Dictionary<string, string>() {{UserId, UserId2}};
            _friendsQueuedRepository.GetKeys().Returns(friendsEnqueued);

            matchCreatorService.CreateMatches();
            _dequeueFriendMatch.DidNotReceive().Execute(UserOne);
            _createMatch.DidNotReceive().Execute(Arg.Is<List<User>>(x => x.Contains(UserOne) && x.Contains(UserTwo)),
                false);
            _createRound.DidNotReceive().Execute(MatchId);
        }
        
                
        [Test]
        public void DontRemoveFromQueueWhenNotPartOfMatch()
        {
            _createMatch.Execute(Arg.Any<List<User>>(), false).Returns(ServerMatchMother.Create(MatchId));
            _getUser.Execute(UserId).Returns(UserOne);
            _getUser.Execute(UserId2).Returns(UserTwo);
            _getUser.Execute(UserId3).Returns(UserThree);
            
            var friendsEnqueued = new Dictionary<string, string>() {{UserId, UserId2},{UserId2, UserId},{UserId3, UserId2}};
            _friendsQueuedRepository.GetKeys().Returns(friendsEnqueued);

            matchCreatorService.CreateMatches();
            
            Received.InOrder(() =>
            {
                _dequeueFriendMatch.Received(1).Execute(UserOne);
                _dequeueFriendMatch.Received(1).Execute(UserTwo);
                _createMatch.Received(1).Execute(Arg.Is<List<User>>(x => x.Contains(UserOne) && x.Contains(UserTwo) && !x.Contains(UserThree)),
                    false);
                _createRound.Received(1).Execute(MatchId);    
            });
            
            _dequeueFriendMatch.DidNotReceive().Execute(UserThree);
        }
        
        [Test]
        public void CreateUserMatchesWhenTwoUsersMatch()
        {
            _createMatch.Execute(Arg.Any<List<User>>(), false).Returns(ServerMatchMother.Create(MatchId));
            _dequeueMatch.Execute(UserOne).Returns( new Tuple<User, DateTime>(UserOne, DateTime.Today));
            _dequeueMatch.Execute(UserTwo).Returns( new Tuple<User, DateTime>(UserTwo,  DateTime.Today.AddSeconds(1)));
            
            var usersEnqueued = new List<Tuple<User, DateTime>>
            {
                new (UserOne, DateTime.Today),
                new (UserTwo,  DateTime.Today.AddSeconds(1))
            };
            _friendsQueuedRepository.GetKeys().Returns(new Dictionary<string, string>());
            _usersQueuedRepository.GetAll().Returns(usersEnqueued);

            matchCreatorService.CreateMatches();
            Received.InOrder(() =>
            {
                _dequeueMatch.Received(1).Execute(UserOne);
                _dequeueMatch.Received(1).Execute(UserTwo);
                _createMatch.Received(1).Execute(Arg.Is<List<User>>(x => x.Contains(UserOne) && x.Contains(UserTwo)),
                    false);
                _createRound.Received(1).Execute(MatchId);    
            });
        }
        
        [Test]
        public void ReAddUserWhenNotEnoughCreateUserMatchesWhenTwoUsersMatch()
        {
            _createMatch.Execute(Arg.Any<List<User>>(), false).Returns(ServerMatchMother.Create(MatchId));
            _dequeueMatch.Execute(UserOne).Returns( new Tuple<User, DateTime>(UserOne, DateTime.Today));
            _dequeueMatch.Execute(UserTwo).Returns( new Tuple<User, DateTime>(UserTwo,  DateTime.Today.AddSeconds(1)));
            _dequeueMatch.Execute(UserThree).Returns( new Tuple<User, DateTime>(UserThree,  DateTime.Today.AddSeconds(2)));
            var usersEnqueued = new List<Tuple<User, DateTime>>
            {
                new (UserOne, DateTime.Today),
                new (UserTwo,  DateTime.Today.AddSeconds(1)),
                new (UserThree,  DateTime.Today.AddSeconds(2))
            };
            _usersQueuedRepository.GetAll().Returns(usersEnqueued);

            matchCreatorService.CreateMatches();
            Received.InOrder(() =>
            {
                _dequeueMatch.Received(1).Execute(UserOne);
                _dequeueMatch.Received(1).Execute(UserTwo);
                _createMatch.Received(1).Execute(Arg.Is<List<User>>(x => x.Contains(UserOne) && x.Contains(UserTwo)),
                    false);
                _createRound.Received(1).Execute(MatchId);
                _enqueueMatch.Received(1).Execute(UserThree, Arg.Any<DateTime>());
            });
        }
    }
}