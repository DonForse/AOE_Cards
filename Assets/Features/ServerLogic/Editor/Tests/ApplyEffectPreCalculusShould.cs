using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Entities;
using Features.ServerLogic.Editor.Tests.Mothers;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Users.Domain;
using NSubstitute;
using NUnit.Framework;

namespace Features.ServerLogic.Editor.Tests
{
    public class ApplyEffectPreCalculusShould
    {
        private const string UserId = "UserId";
        private const string UserIdTwo = "UserId-2";
        private const string MatchId = "MatchId";
        private ApplyEffectPreCalculus _applyEffectPreCalculus;
        private IMatchesRepository _matchesRepository;
        private IGetPlayerPlayedUpgradesInMatch _getPlayerPlayedUpgradesInMatch;


        static IRoundResultTestCaseSource[] _roundsCases =
        {
            new CavalryWinWhenTeutonsFaith(),
            new WinWithPersianTC()
        };

        [SetUp]
        public void Setup()
        {
            _matchesRepository = Substitute.For<IMatchesRepository>();
            _getPlayerPlayedUpgradesInMatch = Substitute.For<IGetPlayerPlayedUpgradesInMatch>();
            _applyEffectPreCalculus = new ApplyEffectPreCalculus(_matchesRepository, _getPlayerPlayedUpgradesInMatch);
        }

        [Test]
        public void JustFail()
        {
            // var normalUpgrade = UpgradeCardMother.Create();
            // var teutonsFaith = UpgradeCardMother.CreateFakeTeutonsFaithCard()();
            var persianTc = UpgradeCardMother.CreateFakePersianTC();
            var sm = ServerMatchMother.Create(
                MatchId,
                new List<User>{ UserMother.Create(UserId), UserMother.Create(UserIdTwo) },
                BoardMother.Create(
                        withCurrentRound:
                            RoundMother.Create()));

            var upgrades = new List<UpgradeCard> {persianTc};
            // var upgrades = new List<UpgradeCard>(){};
            _getPlayerPlayedUpgradesInMatch.Execute(MatchId,UserId).Returns(upgrades);
            _matchesRepository.Get(MatchId).Returns(sm);
            
            _applyEffectPreCalculus.Execute(MatchId);
/*
 * var match = _matchesRepository.Get(matchId);
            var currentRound = match.Board.CurrentRound;
            foreach (var user in match.Users)
            {
                var upgrades = _getPlayerPlayedUpgradesInMatch.Execute(matchId, user.Id);

                foreach (var upgradeCardPlayed in upgrades)
                {
                    foreach (var strategy in _upgradeCardPreCalculusStrategies)
                    {
                        if (!strategy.IsValid(upgradeCardPlayed)) continue;
                        var rivalCard = currentRound.PlayerCards.First(x => x.Key != user.Id);
                        strategy.Execute(upgradeCardPlayed, currentRound.PlayerCards[user.Id].UnitCard,
                            rivalCard.Value.UnitCard, match, currentRound, user.Id);
                    }
                }
            }
 */
            Assert.Fail();
        }
    }
}