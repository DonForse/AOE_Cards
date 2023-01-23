using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Editor.Tests.Mothers;
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
    public class RerollHandlerShould
    {
        private const string MatchId = "MatchId";
        private const string UserId = "UserId";

        private RerollHandler _rerollHandler;
        private IMatchesRepository _matchesRepository;
        private IPlayReroll _playReroll;

        [SetUp]
        public void Setup()
        {
            _matchesRepository = Substitute.For<IMatchesRepository>();
            _playReroll = Substitute.For<IPlayReroll>();
            _rerollHandler = new RerollHandler(_matchesRepository, _playReroll);
        }

        [Test]
        public void RespondsErrorWhenMatchIsNull()
        {
            _matchesRepository.Get(MatchId).Returns((ServerMatch) null);
            var response = WhenPost(new() {""}, new() {""});

            Assert.AreEqual("Match Not Found!", response.error);
            Assert.AreEqual("{\"units\":null,\"upgrades\":null}", response.response);
        }

        [Test]
        public void CallPlayRerollWhenPost()
        {
            _matchesRepository.Get(MatchId).Returns(ServerMatchMother.Create(MatchId));
            var response = WhenPost(new() {"test-unit"}, new() {"test-upgrade"});
            _playReroll.Received(1).Execute(MatchId, UserId,
                Arg.Is<RerollInfoDto>(x =>
                    x.unitCards.Contains("test-unit")
                    && x.upgradeCards.Contains("test-upgrade"))
            );
        }

        [Test]
        public void RespondsHand()
        {
            _matchesRepository.Get(MatchId).Returns(ServerMatchMother.Create(withBoard: BoardMother.Create(
                withPlayerHands: new Dictionary<string, Hand>()
                {
                    {
                        UserId, new Hand
                        {
                            UnitsCards = new List<UnitCard>() {UnitCardMother.Create("test-hand-unit")},
                            UpgradeCards = new List<UpgradeCard>() {UpgradeCardMother.Create("test-hand-upgrade")}
                        }
                    },
                    {UserId + "2", new Hand
                    {
                        UnitsCards = new List<UnitCard>() {UnitCardMother.Create("test-hand-unit")},
                        UpgradeCards = new List<UpgradeCard>() {UpgradeCardMother.Create("test-hand-upgrade")}
                    }},
                })));
            var response = WhenPost(new() {"test-unit"}, new() {"test-upgrade"});

            Debug.Log(response.response);

            Assert.AreEqual("", response.error);
            Assert.AreEqual("{\"units\":[\"test-hand-unit\"],\"upgrades\":[\"test-hand-upgrade\"]}", response.response);
        }

        private ResponseDto WhenPost(List<string> unitCards, List<string> upgradeCards)
        {
            return _rerollHandler.Post(UserId, MatchId,
                new RerollInfoDto() {unitCards = unitCards, upgradeCards = upgradeCards});
        }


        /*
         *  var cards = json;
                var match = _getMatch.Execute(matchId);
                if (match == null)
                    throw new ApplicationException("Match Not Found!");
                _playReroll.Execute(match, userId, cards);
                var handDto = new HandDto(_getMatch.Execute(matchId).Board.PlayersHands[userId]);

                var responseDto = new ResponseDto
                {
                    response = JsonConvert.SerializeObject(handDto),
                    error = string.Empty
                };
                return responseDto;
         */
    }
}