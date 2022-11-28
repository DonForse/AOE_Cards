using Features.Game.Scripts.Domain;

namespace Features.Game.Scripts.Infrastructure
{
    public class InMemoryMatchStateRepository : IMatchStateRepository
    {
        private MatchState _matchState;

        public MatchState Get() => _matchState;

        public void Set(MatchState matchState) => _matchState = matchState;
    }
}