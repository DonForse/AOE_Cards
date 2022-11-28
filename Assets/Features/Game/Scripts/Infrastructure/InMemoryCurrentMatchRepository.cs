using Features.Game.Scripts.Presentation;
using Features.Match.Domain;

namespace Game
{
    public class InMemoryCurrentMatchRepository : ICurrentMatchRepository
    {
        private GameMatch _gameMatch;
        public GameMatch Get() => _gameMatch;

        public void Set(GameMatch gameMatch) => _gameMatch = gameMatch;
        public void Set(Hand hand)
        {
            _gameMatch.Hand = hand;
        }
    }
}