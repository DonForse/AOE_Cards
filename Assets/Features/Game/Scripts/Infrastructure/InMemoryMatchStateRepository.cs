using Features.Game.Scripts.Domain;

namespace Features.Game.Scripts.Infrastructure
{
    public class InMemoryMatchStateRepository : IMatchStateRepository
    {
        private GameState _gameState;

        public GameState Get() => _gameState;

        public void Set(GameState gameState) => _gameState = gameState;
    }
}