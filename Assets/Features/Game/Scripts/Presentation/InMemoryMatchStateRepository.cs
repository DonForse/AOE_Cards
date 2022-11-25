using Game;

namespace Features.Game.Scripts.Presentation
{
    public class InMemoryMatchStateRepository : IMatchStateRepository
    {
        private MatchState _matchState;

        public MatchState Get() => _matchState;

        public void Set(MatchState matchState) => _matchState = matchState;
    }
}