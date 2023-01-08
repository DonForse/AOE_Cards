using System;
using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Editor.Tests.Mothers;
using Features.ServerLogic.Handlers;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Matches.Infrastructure.DTO;
using Features.ServerLogic.Matches.Service;
using Features.ServerLogic.Users.Actions;
using Features.ServerLogic.Users.Domain;
using Features.ServerLogic.Users.Infrastructure;
using NSubstitute;
using NUnit.Framework;

namespace Features.ServerLogic.Editor.Tests
{
    public class MatchHandlerShould
    {
        private const string UserId = "UserId";
        private const string MatchId = "MatchId";
        private MatchHandler _matchHandler;
        private IUsersQueuedRepository _usersQueuedRepository;
        private IFriendsUsersQueuedRepository _friendsQueuedRepository;
        private IUsersRepository _usersRepository;
        private IMatchCreatorService _matchCreatorService;
        private ICreateMatch _createMatch;
        private IGetUser _getUser;
        private IEnqueueFriendMatch _enqueueFriendMatch;
        private IEnqueueMatch _enqueueMatch;
        private IDequeueFriendMatch _dequeueFriendMatch;
        private IDequeueMatch _dequeueMatch;
        private IRemoveUserMatch _removeUserMatch;
        private IGetUserMatch _getUserMatch;
        private ICreateRound _createRound;


        [SetUp]
        public void Setup()
        {
            _matchCreatorService = Substitute.For<IMatchCreatorService>();
            _getUser = Substitute.For<IGetUser>();
            _createMatch = Substitute.For<ICreateMatch>();
            _enqueueFriendMatch = Substitute.For<IEnqueueFriendMatch>();
            _enqueueMatch = Substitute.For<IEnqueueMatch>();
            _dequeueFriendMatch = Substitute.For<IDequeueFriendMatch>();
            _dequeueMatch = Substitute.For<IDequeueMatch>();
            _getUserMatch = Substitute.For<IGetUserMatch>();
            _removeUserMatch = Substitute.For<IRemoveUserMatch>();
            _createRound = Substitute.For<ICreateRound>();
            _matchHandler = new MatchHandler(_getUserMatch,
                _matchCreatorService,_createMatch, _getUser, _enqueueFriendMatch,
                _enqueueMatch,_dequeueFriendMatch, _dequeueMatch ,_removeUserMatch, _createRound);
        }

        [Test]
        public void CreateMatchesWhenGet()
        {
            _matchHandler.Get(UserId);
            _matchCreatorService.Received(1).CreateMatches();
        }

        [Test]
        public void RespondsErrorWhenGetAndNoUser()
        {
            GivenUserDoesNotExists();
            var response = WhenGet();
            ThenResponseContainsError();
            void ThenResponseContainsError()
            {
                Assert.AreEqual("{\"matchId\":null,\"board\":null,\"hand\":null,\"users\":null}", response.response);
                Assert.AreEqual("user is not valid", response.error);
            }
        }

        [Test]
        public void RespondsEmptyWhenGetAndNoMatch()
        {
            GivenUser();
            var response = WhenGet();
            _getUserMatch.Execute(UserId).Returns((ServerMatch)null);
            ThenResponseIsEmptyMatchDto(response);
        }

        [Test]
        public void ReturnMatchWhenGetAndMatchExists()
        {
            GivenUser();
            GivenMatchExists();
            var response = WhenGet();
            Assert.AreEqual("{\"matchId\":\""+MatchId+"\",\"board\":{\"rounds\":[]},\"hand\":{\"units\":[],\"upgrades\":[]},\"users\":[]}", response.response);
            Assert.AreEqual("", response.error);
        }
        
        [Test]
        public void RespondsErrorWhenPostAndNoUser()
        {
            GivenUserDoesNotExists();
            var response = WhenPost(new MatchInfoDto());
            ThenResponseContainsError();
            void ThenResponseContainsError()
            {
                Assert.AreEqual("{\"matchId\":null,\"board\":null,\"hand\":null,\"users\":null}", response.response);
                Assert.AreEqual("user is not valid", response.error);
            }
        }
        
        [Test]
        public void CreateMatchWhenPostVersusBotAndReturnEmptyResponse()
        {
            GivenUser();
            GivenMatchExists();
            var expectedBotDifficulty = 5;
            var response = WhenPost(new MatchInfoDto() {vsBot = true, botDifficulty = expectedBotDifficulty});
            ThenCreateMatchIsCalled(expectedBotDifficulty);
            ThenResponseIsEmptyMatchDto(response);
        }

        [Test]
        public void CreateRoundForMatchWhenPostVersusBotAndReturnEmptyResponse()
        {
            GivenUser();
            GivenMatchExists();
            var expectedBotDifficulty = 5;
            var response = WhenPost(new MatchInfoDto() { vsBot = true, botDifficulty = expectedBotDifficulty });
            ThenCreateRoundIsCalled();
            ThenResponseIsEmptyMatchDto(response);
        }


        [Test]
        public void EnqueueUserWhenPostVsFriend()
        {
            var expectedFriendCode = "ExpectedFriendCode";
            GivenUser(friendCode: expectedFriendCode);
            var response = WhenPost(new MatchInfoDto() {vsFriend = true, friendCode = expectedFriendCode});
            _enqueueFriendMatch.Received(1).Execute(Arg.Is<User>(user=>user.Id == UserId),expectedFriendCode);
            ThenResponseIsEmptyMatchDto(response);
        }

        [Test]
        public void EnqueueUserWhenPost()
        {
            var expectedFriendCode = "ExpectedFriendCode";
            GivenUser(friendCode: expectedFriendCode);
            var response = WhenPost(new MatchInfoDto());
            _enqueueMatch.Received(1).Execute(Arg.Is<User>(user=>user.Id == UserId), Arg.Any<DateTime>());
            ThenResponseIsEmptyMatchDto(response);
        }

        [Test]
        public void RespondsErrorWhenDeleteAndNoUser()
        {
            GivenUserDoesNotExists();
            var response = WhenDelete();
            ThenResponseContainsError();
            void ThenResponseContainsError()
            {
                Assert.AreEqual("", response.response);
                Assert.AreEqual("user is not valid", response.error);
            }
        }

        [Test]
        public void DequeueFromWaitListAndRemoveMatchDataIfExists()
        {
            GivenUser(friendCode:"FriendCode");
            var response = WhenDelete();
            
            _dequeueFriendMatch.Received(1).Execute(Arg.Is<User>(user=> user.Id == UserId && user.FriendCode == "FriendCode"));
            _dequeueMatch.Received(1).Execute(Arg.Is<User>(user=> user.Id == UserId && user.FriendCode == "FriendCode"));
            _removeUserMatch.Received(1).Execute(UserId);

            ThenResponseIsExpected();
            void ThenResponseIsExpected()
            {
                Assert.AreEqual("ok", response.response);
                Assert.AreEqual("", response.error);
            }
        }


        private ResponseDto WhenDelete() => _matchHandler.Delete(UserId);

        private void GivenMatchExists() => _getUserMatch.Execute(UserId).Returns(
            ServerMatchMother.Create(MatchId,
                withBoard:BoardMother.Create(
                    withCurrentRound:null,
                    withRoundsPlayed: new List<Round>(),
                    withPlayerHands:new Dictionary<string, Hand>()
            {
                {UserId, new Hand(){UnitsCards = new List<UnitCard>(), UpgradeCards = new List<UpgradeCard>()}},
                {UserId+"2", new Hand(){UnitsCards = new List<UnitCard>(), UpgradeCards = new List<UpgradeCard>()}}
            })));

        private void GivenUser(string userId = UserId, string friendCode = "FriendCode", string password = "Password", string userName = "UserName") =>
            _getUser.Execute(UserId)
                .Returns(
                    UserMother.Create(userId,friendCode ,password , userName));


        private void GivenUserDoesNotExists() => _getUser.Execute(UserId).Returns((User)null);

        private ResponseDto WhenPost(MatchInfoDto matchInfoDto) => _matchHandler.Post(UserId, matchInfoDto);

        private ResponseDto WhenGet() => _matchHandler.Get(UserId);

        private void ThenResponseIsEmptyMatchDto(ResponseDto response)
        {
            Assert.AreEqual("{\"matchId\":null,\"board\":null,\"hand\":null,\"users\":null}", response.response);
            Assert.AreEqual("",response.error);
        }

        private void ThenCreateMatchIsCalled(int expectedBotDifficulty) =>
            _createMatch.Received(1)
                .Execute(Arg.Is<List<User>>(userList => userList.Count == 1 && userList.First().Id == UserId), true,
                    expectedBotDifficulty);

        private void ThenCreateRoundIsCalled() => _createRound.Received(1).Execute(MatchId);
    }
}