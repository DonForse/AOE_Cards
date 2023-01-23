using System.Collections.Generic;
using System.Drawing.Drawing2D;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Editor.Tests.Mothers;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure.DTO;
using NSubstitute;
using NUnit.Framework;

namespace Features.ServerLogic.Editor.Tests
{
    public class PlayInactiveMatchShould
    {
        private const string UserIdOne = "1";
        private const string UserIdTwo = "2";
        private string MatchId = "MatchId";
        private PlayInactiveMatch _playInactiveMatch;
        private IPlayUpgradeCard _playUpgradeCard;
        private IPlayUnitCard _playUnitCard;
        private IPlayReroll _playReroll;

        [SetUp]
        public void Setup()
        {
            _playUnitCard = Substitute.For<IPlayUnitCard>();
            _playUpgradeCard = Substitute.For<IPlayUpgradeCard>();
            _playReroll = Substitute.For<IPlayReroll>();
            _playInactiveMatch = new PlayInactiveMatch(_playUnitCard, _playUpgradeCard, _playReroll);
        }

        [Test]
        public void PlayUnitIfRoundIsInUnitPhase()
        {
            string expectedCardName = "card1";
            string expectedCardNameTwo = "card2";

            var playerCards = new Dictionary<string, PlayerCard>()
            {
                {UserIdOne, new PlayerCard()},
                {UserIdTwo, new PlayerCard()},
            };

            var playerOneHand = new Hand()
            {
                UnitsCards = new List<UnitCard>()
                {
                    UnitCardMother.Create(expectedCardName),
                    UnitCardMother.Create(""),
                }
            };
            var playerTwoHand = new Hand()
            {
                UnitsCards = new List<UnitCard>()
                {
                    UnitCardMother.Create(expectedCardNameTwo),
                    UnitCardMother.Create(""),
                }
            };

            var playerHands = new Dictionary<string, Hand>()
            {
                {UserIdOne, playerOneHand},
                {UserIdTwo, playerTwoHand}
            };
            var board = BoardMother.Create(withPlayerHands: playerHands);
            var serverMatch = ServerMatchMother.Create(withId: MatchId, withBoard: board);
            var round = RoundMother.Create(withUsers: new List<string>() {UserIdOne, UserIdTwo},
                withPlayerCards: playerCards);

            GivenRoundInUnitState(round);
            WhenExecute(serverMatch, round);
            Received.InOrder(() =>
            {
                ThenPlayedCardForUserOne();
                ThenPlayedCardForUserTwo();
            });

            void ThenPlayedCardForUserOne() => _playUnitCard.Received(1).Execute(MatchId, UserIdOne, expectedCardName);

            void ThenPlayedCardForUserTwo() =>
                _playUnitCard.Received(1).Execute(MatchId, UserIdTwo, expectedCardNameTwo);
        }

        [Test]
        public void NotPlayUnitIfRoundIsInUnitPhaseButPlayerAlreadyPlayedUnit()
        {
            string expectedCardName = "card1";
            string expectedCardNameTwo = "card2";

            var playerCards = new Dictionary<string, PlayerCard>()
            {
                {UserIdOne, new PlayerCard()},
                {UserIdTwo, new PlayerCard() {UnitCard = UnitCardMother.Create("someCard")}},
            };

            var playerOneHand = new Hand()
            {
                UnitsCards = new List<UnitCard>()
                {
                    UnitCardMother.Create(expectedCardName),
                    UnitCardMother.Create(""),
                }
            };
            var playerTwoHand = new Hand()
            {
                UnitsCards = new List<UnitCard>()
                {
                    UnitCardMother.Create(expectedCardNameTwo),
                    UnitCardMother.Create(""),
                }
            };

            var playerHands = new Dictionary<string, Hand>()
            {
                {UserIdOne, playerOneHand},
                {UserIdTwo, playerTwoHand}
            };
            var board = BoardMother.Create(withPlayerHands: playerHands);
            var serverMatch = ServerMatchMother.Create(withId: MatchId, withBoard: board);
            var round = RoundMother.Create(withUsers: new List<string>() {UserIdOne, UserIdTwo},
                withPlayerCards: playerCards);

            GivenRoundInUnitState(round);
            WhenExecute(serverMatch, round);

            ThenPlayedCardForUserOne();
            ThenDidNotPlayCardForUserTwo();

            void ThenPlayedCardForUserOne() => _playUnitCard.Received(1).Execute(MatchId, UserIdOne, expectedCardName);

            void ThenDidNotPlayCardForUserTwo() =>
                _playUnitCard.DidNotReceive().Execute(MatchId, UserIdTwo, Arg.Any<string>());
        }

        [Test]
        public void PlayUpgradeIfRoundIsInUpgradePhase()
        {
            string expectedCardName = "card1";
            string expectedCardNameTwo = "card2";

            var playerCards = new Dictionary<string, PlayerCard>()
            {
                {UserIdOne, new PlayerCard()},
                {UserIdTwo, new PlayerCard()},
            };

            var playerOneHand = new Hand()
            {
                UpgradeCards = new List<UpgradeCard>()
                {
                    UpgradeCardMother.Create(expectedCardName),
                    UpgradeCardMother.Create(""),
                }
            };
            var playerTwoHand = new Hand()
            {
                UpgradeCards = new List<UpgradeCard>()
                {
                    UpgradeCardMother.Create(expectedCardNameTwo),
                    UpgradeCardMother.Create(""),
                }
            };

            var playerHands = new Dictionary<string, Hand>()
            {
                {UserIdOne, playerOneHand},
                {UserIdTwo, playerTwoHand}
            };
            var board = BoardMother.Create(withPlayerHands: playerHands);
            var serverMatch = ServerMatchMother.Create(withId: MatchId, withBoard: board);
            var round = RoundMother.Create(withUsers: new List<string>() {UserIdOne, UserIdTwo},
                withPlayerCards: playerCards);

            GivenRoundInUpgradeState(round);
            WhenExecute(serverMatch, round);
            Received.InOrder(() =>
            {
                ThenPlayedCardForUserOne();
                ThenPlayedCardForUserTwo();
            });

            void ThenPlayedCardForUserOne() =>
                _playUpgradeCard.Received(1).Execute(MatchId, UserIdOne, expectedCardName);

            void ThenPlayedCardForUserTwo() =>
                _playUpgradeCard.Received(1).Execute(MatchId, UserIdTwo, expectedCardNameTwo);
        }

        [Test]
        public void NotPlayUpgradeIfRoundIsInUpgradePhaseButPlayerAlreadyPlayedUpgrade()
        {
            string expectedCardName = "card1";
            string expectedCardNameTwo = "card2";

            var playerCards = new Dictionary<string, PlayerCard>()
            {
                {UserIdOne, new PlayerCard()},
                {UserIdTwo, new PlayerCard(){UpgradeCard = UpgradeCardMother.Create("SomeCard")}},
            };

            var playerOneHand = new Hand()
            {
                UpgradeCards = new List<UpgradeCard>()
                {
                    UpgradeCardMother.Create(expectedCardName),
                    UpgradeCardMother.Create(""),
                }
            };
            var playerTwoHand = new Hand()
            {
                UpgradeCards = new List<UpgradeCard>()
                {
                    UpgradeCardMother.Create(expectedCardNameTwo),
                    UpgradeCardMother.Create(""),
                }
            };

            var playerHands = new Dictionary<string, Hand>()
            {
                {UserIdOne, playerOneHand},
                {UserIdTwo, playerTwoHand}
            };
            var board = BoardMother.Create(withPlayerHands: playerHands);
            var serverMatch = ServerMatchMother.Create(withId: MatchId, withBoard: board);
            var round = RoundMother.Create(withUsers: new List<string>() {UserIdOne, UserIdTwo},
                withPlayerCards: playerCards);

            GivenRoundInUpgradeState(round);
            WhenExecute(serverMatch, round);
            ThenPlayedCardForUserOne();
            ThenPlayedCardForUserTwo();

            void ThenPlayedCardForUserOne() =>
                _playUpgradeCard.Received(1).Execute(MatchId, UserIdOne, expectedCardName);

            void ThenPlayedCardForUserTwo() =>
                _playUpgradeCard.DidNotReceive().Execute(MatchId, UserIdTwo, Arg.Any<string>());
        }

        [Test]
        public void PlayRerollIfRoundIsInRerollPhase()
        {
            var playerCards = new Dictionary<string, PlayerCard>()
            {
                {UserIdOne, new PlayerCard()},
                {UserIdTwo, new PlayerCard()},
            };

            var board = BoardMother.Create();
            var serverMatch = ServerMatchMother.Create(MatchId, withBoard: board);
            var round = RoundMother.Create(withUsers: new List<string>() {UserIdOne, UserIdTwo},
                withPlayerCards: playerCards,
                withPlayerReroll: new Dictionary<string, bool>() {{UserIdOne, false}, {UserIdTwo, false}});

            GivenRoundInRerollState(round);
            WhenExecute(serverMatch, round);
            Received.InOrder(() =>
            {
                ThenPlayRerollForUserOne();
                ThenPlayRerollForUserTwo();
            });

            void ThenPlayRerollForUserOne() => _playReroll.Received(1).Execute(MatchId, UserIdOne, Arg.Is<RerollInfoDto>(x=>x.unitCards.Count == 0 && x.upgradeCards.Count == 0));
            void ThenPlayRerollForUserTwo() => _playReroll.Received(1).Execute(MatchId, UserIdTwo, Arg.Is<RerollInfoDto>(x=>x.unitCards.Count == 0 && x.upgradeCards.Count == 0));
        }
        
        [Test]
        public void NotPlayRerollIfRoundIsInRerollPhaseButPlayerAlreadyRerolled()
        {
            var playerCards = new Dictionary<string, PlayerCard>()
            {
                {UserIdOne, new PlayerCard()},
                {UserIdTwo, new PlayerCard()},
            };

            var board = BoardMother.Create();
            var serverMatch = ServerMatchMother.Create(MatchId, withBoard: board);
            var round = RoundMother.Create(withUsers: new List<string>() {UserIdOne, UserIdTwo},
                withPlayerCards: playerCards,
                withPlayerReroll: new Dictionary<string, bool>() {{UserIdOne, true}, {UserIdTwo, false}});

            GivenRoundInRerollState(round);
            WhenExecute(serverMatch, round);

            ThenDidNotPlayRerollForUserOne();
                ThenPlayRerollForUserTwo();

                void ThenDidNotPlayRerollForUserOne() => _playReroll.DidNotReceive()
                    .Execute(MatchId, UserIdOne, Arg.Any<RerollInfoDto>());
            void ThenPlayRerollForUserTwo() => _playReroll.Received(1).Execute(MatchId, UserIdTwo, Arg.Is<RerollInfoDto>(x=>x.unitCards.Count == 0 && x.upgradeCards.Count == 0));
        }

        private void WhenExecute(ServerMatch serverMatch, Round round) =>
            _playInactiveMatch.Execute(serverMatch, round);
        private static void GivenRoundInRerollState(Round round) => round.ChangeRoundState(RoundState.Reroll);
        private static void GivenRoundInUnitState(Round round) => round.ChangeRoundState(RoundState.Unit);
        private static void GivenRoundInUpgradeState(Round round) => round.ChangeRoundState(RoundState.Upgrade);
    }
}