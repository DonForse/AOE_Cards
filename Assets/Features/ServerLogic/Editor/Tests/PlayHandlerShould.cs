using Features.ServerLogic.Cards.Infrastructure;
using Features.ServerLogic.Handlers;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Users.Actions;
using NUnit.Framework;

namespace Features.ServerLogic.Editor.Tests
{
    public class PlayHandlerShould
    {
        private PlayHandler _playHandler;
        private IMatchesRepository _matchesRepository;
        private ICardRepository _cardRepository;
        private IRemoveUserMatch _removeUserMatch;

        [SetUp]
        public void Setup()
        {
            _playHandler = new PlayHandler(_matchesRepository, _cardRepository, _removeUserMatch);
        }

        [Test]
        public void asd()
        {Assert.Fail();
        }
    }
}