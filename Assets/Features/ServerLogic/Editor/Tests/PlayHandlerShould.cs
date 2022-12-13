using System;
using System.Collections.Generic;
using Features.ServerLogic.Cards.Infrastructure;
using Features.ServerLogic.Editor.Tests.Mothers;
using Features.ServerLogic.Handlers;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Matches.Infrastructure.DTO;
using Features.ServerLogic.Users.Actions;
using Features.ServerLogic.Users.Domain;
using NSubstitute;
using NUnit.Framework;

namespace Features.ServerLogic.Editor.Tests
{
    public class PlayHandlerShould
    {
        private const string MatchId = "MatchId";
        private const string UserId = "UserId";

        private PlayHandler _playHandler;
        private IMatchesRepository _matchesRepository;
        private ICardRepository _cardRepository;
        private IRemoveUserMatch _removeUserMatch;
        private IGetMatch _getMatch;


        [SetUp]
        public void Setup()
        {
            _getMatch = Substitute.For<IGetMatch>();
            _removeUserMatch = Substitute.For<IRemoveUserMatch>();
            _playHandler = new PlayHandler(_matchesRepository, _cardRepository, _removeUserMatch, _getMatch);
        }

        [Test]
        public void ThrowsWhenGetAndNoMatch()
        {
            GivenGetMatchReturnsNoMatch();
            ThenThrows(() => WhenGet());

            void GivenGetMatchReturnsNoMatch() => _getMatch.Execute(MatchId).Returns((ServerMatch) null);
            void ThenThrows(TestDelegate testDelegate) => Assert.Throws<ApplicationException>(testDelegate);
        }

        [Test]
        public void RemoveUserWhenMatchIsFinished()
        {
            GivenGetMatchReturnsFinishedMatch();
            var response = WhenGet();
            ThenRemoveUserMatchIsCalled();

            void GivenGetMatchReturnsFinishedMatch()
            {
                _getMatch.Execute(MatchId)
                    .Returns(
                        ServerMatchMother.Create(
                            withIsFinished:true,
                            withBoard: BoardMother.Create(
                                withRoundsPlayed: new List<Round>()
                                {
                                    RoundMother.Create(
                                        new List<string>() {UserId, UserId + "2"},
                                        withPlayerCards: new Dictionary<string, PlayerCard>()
                                        {
                                            {UserId, new PlayerCard()},
                                            {UserId + "2", new PlayerCard()}
                                        },
                                        withRoundUpgradeCard: UpgradeCardMother.Create(),
                                        withPlayerReroll: new Dictionary<string, bool>()
                                        {
                                            {UserId, false},
                                            {UserId + "2", false}
                                        })
                                }),
                            withUsers: new List<User>() {UserMother.Create(UserId), UserMother.Create(UserId + "2")}
                        ));
            }

            void ThenRemoveUserMatchIsCalled() => _removeUserMatch.Received(1).Execute(UserId);
        }

        [Test]
        public void Responds()
        {
            GivenGetMatchReturnsMatch();
            var response = WhenGet();
            Assert.AreEqual("{}", response.response);
            Assert.AreEqual("", response.error);
        }

        private void GivenGetMatchReturnsMatch() => _getMatch.Execute(MatchId)
            .Returns(
                ServerMatchMother.Create(
                    withBoard: BoardMother.Create(
                        withRoundsPlayed: new List<Round>()
                        {
                            RoundMother.Create(
                                new List<string>() {UserId, UserId + "2"},
                                withPlayerCards: new Dictionary<string, PlayerCard>()
                                {
                                    {UserId, new PlayerCard()},
                                    {UserId + "2", new PlayerCard()}
                                },
                                withRoundUpgradeCard: UpgradeCardMother.Create(),
                                withPlayerReroll: new Dictionary<string, bool>()
                                {
                                    {UserId, false},
                                    {UserId + "2", false}
                                })
                        }),
                    withUsers: new List<User>() {UserMother.Create(UserId), UserMother.Create(UserId + "2")}
                ));

        private ResponseDto WhenGet(int roundNumber = 0) => _playHandler.Get(UserId, MatchId, roundNumber);
    }
}