using Features.ServerLogic.Handlers;
using Features.ServerLogic.Matches.Action;
using NSubstitute;
using NUnit.Framework;

namespace Features.ServerLogic.Editor.Tests
{
    public class RoundHandlerShould
    {
        private RoundHandler _roundHandler;
        private IGetMatch _getMatch;

        [SetUp]
        public void Setup()
        {
            _getMatch = Substitute.For<IGetMatch>();
            _roundHandler = new RoundHandler(_getMatch);
        }

        [Test]
        public void RespondsErrorWhenNoMatch()
        {
            Assert.Fail();
        }
        
        [Test]
        public void RespondsErrorWhenNonExistingRound()
        {
            Assert.Fail();
        }
        
        [Test]
        public void RespondsRound()
        {
            Assert.Fail();
        }
    }
}