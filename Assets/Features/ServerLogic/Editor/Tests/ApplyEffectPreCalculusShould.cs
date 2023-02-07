using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Entities;
using Features.ServerLogic.Editor.Tests.Mothers;
using Features.ServerLogic.Game.Domain;
using Features.ServerLogic.Matches;
using Features.ServerLogic.Matches.Action;
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

        
        private IList<IPreCalculusCardStrategy> _strategies;
        private IPreCalculusCardStrategy _strategy;

        [SetUp]
        public void Setup()
        {
            _matchesRepository = Substitute.For<IMatchesRepository>();
            _getPlayerPlayedUpgradesInMatch = Substitute.For<IGetPlayerPlayedUpgradesInMatch>();
            _strategies = new List<IPreCalculusCardStrategy>();
            _strategy = Substitute.For<IPreCalculusCardStrategy>();
            _strategies.Add(_strategy);
            _applyEffectPreCalculus = new ApplyEffectPreCalculus(_matchesRepository, _getPlayerPlayedUpgradesInMatch, _strategies);
        }

        [Test]
        public void CallStrategyForEveryUpgradeForEveryUser()
        {
            var persianTc = UpgradeCardMother.CreateFakePersianTC();
            var sm = ServerMatchMother.Create(
                MatchId, new List<User>
                {
                    UserMother.Create(UserId), UserMother.Create(UserIdTwo)
                });

            var upgradeRoundCard = UpgradeCardMother.Create();
            var upgradePlayerRoundCard = UpgradeCardMother.Create();
            var playerUpgrades = new List<UpgradeCard> {persianTc, upgradeRoundCard, upgradePlayerRoundCard};
            var secondPlayerUpgrades = new List<UpgradeCard> {persianTc, upgradeRoundCard, upgradePlayerRoundCard};
            
            _getPlayerPlayedUpgradesInMatch.Execute(MatchId,UserId).Returns(playerUpgrades);
            _getPlayerPlayedUpgradesInMatch.Execute(MatchId,UserIdTwo).Returns(secondPlayerUpgrades);
            _matchesRepository.Get(MatchId).Returns(sm);
            _applyEffectPreCalculus.Execute(MatchId);
            
            Received.InOrder(() =>
            {
                _strategy.Received(1).Execute(persianTc, MatchId, UserId);
                _strategy.Received(1).Execute(upgradeRoundCard, MatchId, UserId);
                _strategy.Received(1).Execute(upgradePlayerRoundCard, MatchId, UserId);
                
                _strategy.Received(1).Execute(persianTc, MatchId, UserIdTwo);
                _strategy.Received(1).Execute(upgradeRoundCard, MatchId, UserIdTwo);
                _strategy.Received(1).Execute(upgradePlayerRoundCard, MatchId, UserIdTwo);
            });
        }
    }
}