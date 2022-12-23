using System;
using System.Collections.Generic;
using Features.ServerLogic.Cards.Actions;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Cards.Infrastructure;
using Features.ServerLogic.Editor.Tests.Mothers;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure.DTO;
using NSubstitute;
using NUnit.Framework;

namespace Features.ServerLogic.Editor.Tests
{
    public class PlayRerollShould
    {
        private const string MatchId = "MatchId";
        private const string UserId = "UserId";
        private ICardRepository _cardRepository;
        private PlayReroll _playReroll;
        private IGetUpgradeCard _getUpgradeCard;
        private IGetUnitCard _getUnitCard;


        [SetUp]
        public void Setup()
        {
            _getUnitCard = Substitute.For<IGetUnitCard>();
            _getUpgradeCard = Substitute.For<IGetUpgradeCard>();
            _playReroll = new PlayReroll(_getUnitCard, _getUpgradeCard);
        }

        [Test]
        public void ThrowsWhenNoRounds()
        {
            var match = ServerMatchMother.Create(MatchId, withBoard: BoardMother.Create());
            ThenThrows(() => WhenReroll(match));
        }


        [Test]
        public void ThrowsWhenNotInReroll()
        {
            var match = AMatch(RoundState.Unit);
            ThenThrows(() => WhenReroll(match));
        }

        [Test]
        public void ThrowsWhenUpgradeCardAlreadyPlayedForThatRound()
        {
            var match = AMatch();
            ThenThrows(() => WhenReroll(match));
        }
        
        [Test]
        public void ThrowsWhenUnitCardSelectedForRerollDoesNotExists()
        {
            var cardName = "some card";
            _getUnitCard.Execute(cardName).Returns((UnitCard) null);
            var match = AMatch();
            ThenThrows(() => WhenReroll(match));

        }
        
        [Test]
        public void ThrowsWhenUnitCardSelectedForRerollIsNotInPlayerHand()
        {
            Assert.Fail();
        }

        [Test]
        public void ThrowsWhenUpgradeCardSelectedForRerollDoesNotExists()
        {
            Assert.Fail();
        }
        
        
        [Test]
        public void ThrowsWhenUpgradeCardSelectedForRerollIsNotInPlayerHand()
        {
            Assert.Fail();
        }
        
        [Test]
        public void IgnoreVillagerCardReroll()
        {
            Assert.Fail();
        }
        
        
        [Test]
        public void SetPlayerRerollToTrue()
        {
            Assert.Fail();
        }

        
        [Test]
        public void NotChangeNotRerolledUnitHandCards()
        {
            Assert.Fail();
        }
        [Test]
        public void ChangeRerolledUnitHandCards()
        {
            Assert.Fail();
        }
        [Test]
        public void AddRerolledUnitCardsToDeck()
        {
            Assert.Fail();
        }
        
        [Test]
        public void NotChangeNotRerolledUpgradeHandCards()
        {
            Assert.Fail();
        }
        [Test]
        public void ChangeRerolledUpgradeHandCards()
        {
            Assert.Fail();
        }
        [Test]
        public void AddRerolledUpgradeCardsToDeck()
        {
            Assert.Fail();
        }
        private static ServerMatch AMatch(RoundState withRoundState = RoundState.Reroll,Dictionary<string,PlayerCard> withPlayerCards = null) => 
            ServerMatchMother.Create(MatchId, withBoard:
                BoardMother.Create(withRoundsPlayed:
                    new List<Round>()
                    {
                        RoundMother.Create(withRoundState: withRoundState,
                            withPlayerCards: withPlayerCards),
                    }));

        private void WhenReroll(ServerMatch match) => _playReroll.Execute(match, UserId, new RerollInfoDto());
        private void ThenThrows(TestDelegate when) => Assert.Throws<ApplicationException>(when);

        /*
         var round = serverMatch.Board.RoundsPlayed.LastOrDefault();
            if (round.RoundState != RoundState.Reroll || round.PlayerReroll.ContainsKey(userId) && round.PlayerReroll[userId] )
                throw new ApplicationException("Reroll Not Available");

            if (IsRoundAlradyPlayed(round, userId))
                throw new ApplicationException("Reroll Not Available");

            var unitCards = new List<UnitCard>();
            GetUnitCards(serverMatch, userId, cards, unitCards);

            var upgradeCards = new List<UpgradeCard>();
            GetUpgradeCards(serverMatch, userId, cards, upgradeCards);

            bool revert = false;
            List<UpgradeCard> upgrades = GetNewHandUpgrades(serverMatch, userId, upgradeCards, ref revert);
            List<UnitCard> units = GetNewHandUnits(serverMatch, userId, unitCards, ref revert);

            if (revert)
                throw new ApplicationException("Invalid Reroll");

            round.PlayerReroll[userId] = true;
            AddNewUpgradeCards(serverMatch, userId, upgradeCards, upgrades);
            AddNewUnitCards(serverMatch, userId, unitCards, units);

            if (round.PlayerReroll.Values.Count(v=>v) == 2)
                serverMatch.Board.RoundsPlayed.LastOrDefault().ChangeRoundState(RoundState.Upgrade);
         */
    }
}