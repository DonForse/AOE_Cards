using Features.ServerLogic.Matches;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Infrastructure;
using NUnit.Framework;

namespace Features.ServerLogic.Editor.Tests
{
    public class PersianTcPreCalculusCardStrategyShould
    {
        private PersianTcPreCalculusCardStrategy _strategy;
        private IMatchesRepository _matchesRepository;
        private IGetPlayerPlayedUpgradesInMatch _getPlayerPlayedUpgradesInMatch;

        [SetUp]
        public void Setup()
        {
            _strategy = new PersianTcPreCalculusCardStrategy(_getPlayerPlayedUpgradesInMatch, _matchesRepository);
        }

        [Test]
        public void test()
        {
            Assert.Fail();
        }
    }
}