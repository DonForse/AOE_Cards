using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Entities;
using Features.ServerLogic.Editor.Tests.Mothers;
using Features.ServerLogic.Game.Domain;
using Features.ServerLogic.Matches;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Infrastructure;
using NSubstitute;
using NUnit.Framework;

namespace Features.ServerLogic.Editor.Tests
{
    public class ApplyEffectPostUnitShould
    {
        private const string UserId = "UserId";
        private const string MatchId = "MatchId";
        private ApplyEffectPostUnit _applyEffectPostUnit;
        private IMatchesRepository _matchesRepository;
        private IGetPlayerPlayedUpgradesInMatch _getPlayerPlayedUpgradesInMatch;
        private List<IApplyEffectPostUnitStrategy> _strategies;
        private IApplyEffectPostUnitStrategy _strategy;

        [SetUp]
        public void Setup()
        {
            _matchesRepository = Substitute.For<IMatchesRepository>();
            _getPlayerPlayedUpgradesInMatch = Substitute.For<IGetPlayerPlayedUpgradesInMatch>();
            _strategies = new List<IApplyEffectPostUnitStrategy>();
            _strategy = Substitute.For<IApplyEffectPostUnitStrategy>();
            _strategies.Add(_strategy);
            
            _applyEffectPostUnit = new ApplyEffectPostUnit(_getPlayerPlayedUpgradesInMatch, _strategies);
        }


        [Test]
        public void CallStrategyForEveryUpgrade()
        {
            var roundUpgrade = UpgradeCardMother.Create();
            var playerUpgrade = UpgradeCardMother.Create();
            var playerUpgradePreviousRound = UpgradeCardMother.Create();
            var playerUpgradePreviousRoundTwo = UpgradeCardMother.Create();
            

            var match =ServerMatchMother.Create(MatchId);

            GivenUpgradeCards(new List<UpgradeCard>
                {playerUpgrade, playerUpgradePreviousRound, playerUpgradePreviousRoundTwo, roundUpgrade});
            _matchesRepository.Get(MatchId).Returns(match);
            _applyEffectPostUnit.Execute(MatchId, UserId);
            
            Received.InOrder(() =>
            {
                _strategy.Received(1).Execute(playerUpgrade, MatchId, UserId);
                _strategy.Received(1).Execute(playerUpgradePreviousRound, MatchId, UserId);
                _strategy.Received(1).Execute(playerUpgradePreviousRoundTwo, MatchId, UserId);
                _strategy.Received(1).Execute(roundUpgrade, MatchId, UserId);
            });
        }

        private void GivenUpgradeCards(List<UpgradeCard> cards) => _getPlayerPlayedUpgradesInMatch.Execute(MatchId, UserId).Returns(cards);
        
    }
}