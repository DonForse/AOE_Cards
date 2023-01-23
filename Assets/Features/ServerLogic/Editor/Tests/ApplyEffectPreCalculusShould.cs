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
        private const string MatchId = "MatchId";
        private ApplyEffectPreCalculus _applyEffectPreCalculus;
        private IMatchesRepository _matchesRepository;
        private IGetPlayerPlayedUpgradesInMatch _getPlayerPlayedUpgradesInMatch;

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
            Assert.Fail();
        }
    }
}