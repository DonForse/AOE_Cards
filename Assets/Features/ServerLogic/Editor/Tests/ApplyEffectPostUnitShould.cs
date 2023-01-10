using Features.ServerLogic.Matches.Action;
using NSubstitute;
using NUnit.Framework;

namespace Features.ServerLogic.Editor.Tests
{
    public class ApplyEffectPostUnitShould
    {
        private ApplyEffectPostUnit _applyEffectPostUnit;
        private IGetMatch _getMatch;

        [SetUp]
        public void Setup()
        {
            _getMatch = Substitute.For<IGetMatch>();
            _applyEffectPostUnit = new ApplyEffectPostUnit(_getMatch);
        }

        [Test]
        public void Asd()
        {
            Assert.Fail();
        }
    }
}