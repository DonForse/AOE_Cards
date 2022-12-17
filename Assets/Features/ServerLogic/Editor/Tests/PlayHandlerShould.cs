using System;
using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Cards.Domain.Upgrades;
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
using UnityEngine;

namespace Features.ServerLogic.Editor.Tests
{
    public class PlayHandlerShould
    {
        private const string MatchId = "MatchId";
        private const string UserId = "UserId";

        private PlayHandler _playHandler;
        private ICardRepository _cardRepository;
        private IRemoveUserMatch _removeUserMatch;
        private IGetMatch _getMatch;
        private IPlayUnitCard _playUnitCard;
        private IPlayUpgradeCard _playUpgradeCard;


        [SetUp]
        public void Setup()
        {
            _getMatch = Substitute.For<IGetMatch>();
            _removeUserMatch = Substitute.For<IRemoveUserMatch>();
            _playUnitCard = Substitute.For<IPlayUnitCard>();
            _playUpgradeCard = Substitute.For<IPlayUpgradeCard>();
            _playHandler = new PlayHandler(_removeUserMatch, _getMatch, _playUnitCard, _playUpgradeCard);
        }

        [Test]
        public void RespondsErrorWhenGetAndNoMatch()
        {
            GivenGetMatchReturnsNoMatch();
            var response = WhenGet();
            Assert.AreEqual("{\"finished\":false,\"cardsplayed\":null,\"winnerplayer\":null," +
                            "\"upgradecardround\":null,\"roundnumber\":0,\"rivalready\":false,\"hasReroll\":false,\"roundState\":0," +
                            "\"roundTimer\":0}", response.response);
            Assert.AreEqual("Match not found", response.error);

            void GivenGetMatchReturnsNoMatch() => _getMatch.Execute(MatchId).Returns((ServerMatch) null);
        }

        [Test]
        public void RespondsErrorWhenUnexistingRound()
        {
            GivenGetMatchReturnsMatch();
            var response = WhenGet(5);
            Assert.AreEqual("{\"finished\":false,\"cardsplayed\":null,\"winnerplayer\":null," +
                            "\"upgradecardround\":null,\"roundnumber\":0,\"rivalready\":false,\"hasReroll\":false,\"roundState\":0," +
                            "\"roundTimer\":0}", response.response);
            Assert.AreEqual("Round does not exists", response.error);
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
        public void RespondsGivenRoundInfo()
        {
            GivenGetMatchReturnsMatch();
            var response = WhenGet();
            Assert.AreEqual("{\"finished\":false,\"cardsplayed\":[{\"player\":null,\"upgradecard\":\"\",\"unitcard\":\"\",\"unitcardpower\":0}," +
                            "{\"player\":null,\"upgradecard\":\"\",\"unitcard\":\"\",\"unitcardpower\":0}],\"winnerplayer\":[]," +
                            "\"upgradecardround\":\"unit-card\",\"roundnumber\":0,\"rivalready\":false,\"hasReroll\":true,\"roundState\":0," +
                            "\"roundTimer\":40}", response.response);
            Assert.AreEqual("", response.error);
        }

        [Test]
        public void PlayUpgradeCardWhenPostAndSendUpgrade()
        {
            var expectedCardName = "test";
            var cardType = "upgrade";
            
            GivenGetMatchReturnsMatch(withPlayerHands:new Dictionary<string, Hand>()
            {
                {UserId, new Hand()
                {
                    UnitsCards = new List<UnitCard>(),
                    UpgradeCards = new List<UpgradeCard>()
                }},
                {UserId + "2", new Hand(){}}
            });
            WhenPost(new CardInfoDto() {cardname = expectedCardName, type = cardType});
            ThenPlayUpgradeCardIsCalled();

            void ThenPlayUpgradeCardIsCalled() => _playUpgradeCard.Received(1).Execute(MatchId, UserId, expectedCardName);
        }

        [Test]
        public void PlayUnitCardWhenPostAndSendUnit()
        {
            var expectedCardName = "test";
            var cardType = "unit";
            GivenGetMatchReturnsMatch(withPlayerHands:new Dictionary<string, Hand>()
            {
                {UserId, new Hand()
                {
                    UnitsCards = new List<UnitCard>(),
                    UpgradeCards = new List<UpgradeCard>()
                }},
                {UserId + "2", new Hand(){}}
            });
            WhenPost(new CardInfoDto() {cardname = expectedCardName, type = cardType});
            ThenPlayUpgradeCardIsCalled();

            void ThenPlayUpgradeCardIsCalled() => _playUnitCard.Received(1).Execute(MatchId, UserId, expectedCardName);
        }

        private void GivenGetMatchReturnsMatch(Dictionary<string, PlayerCard> withPlayerCards = null, Dictionary<string, Hand> withPlayerHands = null)
        {
            _getMatch.Execute(MatchId)
                .Returns(
                    ServerMatchMother.Create(
                        withBoard: BoardMother.Create(
                            withRoundsPlayed: new List<Round>()
                            {
                                RoundMother.Create(
                                    new List<string>() {UserId, UserId + "2"},
                                    withPlayerCards: withPlayerCards ?? new Dictionary<string, PlayerCard>()
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
                            },
                            withPlayerHands:withPlayerHands),
                        withUsers: new List<User>()
                        {
                            UserMother.Create(UserId), UserMother.Create(UserId + "2")
                        }
                    ));
        }

        private ResponseDto WhenPost(CardInfoDto cardInfoDto) => _playHandler.Post(UserId, MatchId, cardInfoDto);

        private ResponseDto WhenGet(int roundNumber = 0) => _playHandler.Get(UserId, MatchId, roundNumber);
    }
}