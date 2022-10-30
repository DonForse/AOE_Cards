namespace Game
{
    public interface ICurrentMatchRepository
    {
        Match.Domain.Match Get();
        void Set(Match.Domain.Match match);
    }
}