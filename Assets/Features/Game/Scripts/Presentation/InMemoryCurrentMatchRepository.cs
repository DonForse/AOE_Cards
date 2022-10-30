namespace Game
{
    public class InMemoryCurrentMatchRepository : ICurrentMatchRepository
    {
        private Match.Domain.Match _match;
        public Match.Domain.Match Get() => _match;

        public void Set(Match.Domain.Match match) => _match = match;
    }
}