using System.Collections.Generic;
using Features.ServerLogic.Editor.Tests.Mothers;
using Features.ServerLogic.Game.Domain.Entities;
using Features.ServerLogic.Handlers;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Matches.Infrastructure.DTO;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Features.ServerLogic.Editor.Tests
{
    public class RoundHandlerShould
    {
        private const string UserId = "UserId";
        private const string MatchId = "MatchId";
        private RoundHandler _roundHandler;
        private IMatchesRepository _matchesRepository;

        [SetUp]
        public void Setup()
        {
            _matchesRepository = Substitute.For<IMatchesRepository>();
            _roundHandler = new RoundHandler(_matchesRepository);
        }

        [Test]
        public void RespondsErrorWhenNoMatch()
        {
            GivenGetMatchReturnsNull();
            var response = WhenGet();
            ThenRespondsError();

            void GivenGetMatchReturnsNull() => _matchesRepository.Get(Arg.Any<string>()).Returns((ServerMatch) null);

            void ThenRespondsError()
            {
                Assert.AreEqual("Match not found", response.error);
                Assert.AreEqual(
                    "{\"finished\":false,\"cardsplayed\":null,\"winnerplayer\":null,\"upgradecardround\":null," +
                    "\"roundnumber\":0,\"rivalready\":false,\"hasReroll\":false,\"roundState\":0,\"roundTimer\":0}",
                    response.response);
            }
        }

        [Test]
        public void RespondsErrorWhenNonExistingRound()
        {
            GivenGetMatchReturnsEmptyMatch();
            var response = WhenGet(5);
            ThenRespondsError();

            void GivenGetMatchReturnsEmptyMatch() => _matchesRepository.Get(Arg.Any<string>()).Returns(
                ServerMatchMother.Create(withBoard:
                    BoardMother.Create(withRoundsPlayed: new List<Round>(), withCurrentRound:null)));

            void ThenRespondsError()
            {
                Assert.AreEqual("Round not found", response.error);
                Assert.AreEqual(
                    "{\"finished\":false,\"cardsplayed\":null,\"winnerplayer\":null,\"upgradecardround\":null," +
                    "\"roundnumber\":0,\"rivalready\":false,\"hasReroll\":false,\"roundState\":0,\"roundTimer\":0}",
                    response.response);
            }
        }

        [Test]
        public void RespondsRound()
        {
            GivenGetMatchReturnsEmptyMatch();
            var response = WhenGet();
            ThenRespondsError();

            void GivenGetMatchReturnsEmptyMatch() => _matchesRepository.Get(Arg.Any<string>()).Returns(
                ServerMatchMother.Create(withBoard: BoardMother.Create(withRoundsPlayed: new List<Round>()
                {
                    RoundMother.Create(
                        withUsers: new List<string>() {UserId, UserId + "2"},
                        withPlayerReroll: new Dictionary<string, bool>() {{UserId, false}, {UserId + "2", false}},
                        withPlayerCards: new Dictionary<string, PlayerCard>()
                        {
                            {UserId, new PlayerCard()},
                            {UserId + "2", new PlayerCard()}
                        }
                        )
                })));


            void ThenRespondsError()
            {
                Assert.AreEqual("", response.error);
                Assert.AreEqual(
                    "{\"finished\":false,\"cardsplayed\":[{\"player\":null,\"upgradecard\":\"\",\"unitcard\":\"\"," +
                    "\"unitcardpower\":0},{\"player\":null,\"upgradecard\":\"\",\"unitcard\":\"\",\"unitcardpower\":0}]," +
                    "\"winnerplayer\":[],\"upgradecardround\":null,\"roundnumber\":0,\"rivalready\":false,\"hasReroll\":true,\"roundState\":0,\"roundTimer\":40}",
                    response.response);
            }
        }

        private ResponseDto WhenGet(int roundNumber = 0) => _roundHandler.Get(UserId, MatchId, roundNumber);
    }
}