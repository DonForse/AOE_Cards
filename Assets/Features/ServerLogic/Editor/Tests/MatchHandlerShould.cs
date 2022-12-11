using System;
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
        private IMatchesRepository _matchesRepository;
        private IUsersRepository _usersRepository;
        private IMatchCreatorService _matchCreatorService;
        private ICreateMatch _createMatch;
        private IGetUser _getUser;


        [SetUp]
        public void Setup()
        {
            _matchCreatorService = Substitute.For<IMatchCreatorService>();
            _matchesRepository = Substitute.For<IMatchesRepository>();
            _getUser = Substitute.For<IGetUser>();
            _matchHandler = new MatchHandler(_usersQueuedRepository, _friendsQueuedRepository, _matchesRepository, _usersRepository, _matchCreatorService,_createMatch, _getUser );
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
            _matchesRepository.GetByUserId(UserId).Returns((ServerMatch)null);
            ThenResponseIsEmptyMatchDto(response);
        }

        [Test]
        public void ReturnMatchWhenGetAndMatchExists()
        {
            GivenUser();
            GivenMatchExists();
            var response = WhenGet();
            Assert.AreEqual("{\"matchId\":"+MatchId+"\"null,\"board\":null,\"hand\":null,\"users\":null}", response.response);
            Assert.AreEqual("", response.error);
        }

        private void GivenMatchExists() => _matchesRepository.GetByUserId(UserId).Returns(
            ServerMatchMother.Create(MatchId,withBoard:BoardMother.Create()));
        private void GivenUser() =>
            _getUser.Execute(UserId)
                .Returns(
                    UserMother.Create(UserId, "FriendCode", "Password", "UserName"));

        private void GivenUserDoesNotExists() => _getUser.Execute(UserId).Returns((User)null);

        private ResponseDto WhenGet() => _matchHandler.Get(UserId);

        private void ThenResponseIsEmptyMatchDto(ResponseDto response)
        {
            Assert.AreEqual("{\"matchId\":null,\"board\":null,\"hand\":null,\"users\":null}", response.response);
            Assert.AreEqual("",response.error);
        }
    }
}