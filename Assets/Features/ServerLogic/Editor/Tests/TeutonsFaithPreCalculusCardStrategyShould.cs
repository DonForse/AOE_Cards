using Features.ServerLogic.Matches;
using Features.ServerLogic.Matches.Infrastructure;
using NUnit.Framework;

namespace Features.ServerLogic.Editor.Tests
{
    public class TeutonsFaithPreCalculusCardStrategyShould
    {
        private TeutonsFaithPreCalculusCardStrategy _strategy;
        private IMatchesRepository _matchesRepository;

        [SetUp]
        public void Setup()
        {
            _strategy = new TeutonsFaithPreCalculusCardStrategy(_matchesRepository);
        }

        [Test]
        public void test()
        {
            Assert.Fail();
        }
    }
}