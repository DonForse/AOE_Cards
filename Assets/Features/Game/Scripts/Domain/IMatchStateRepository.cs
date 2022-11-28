namespace Features.Game.Scripts.Domain
{
    public interface IMatchStateRepository
    {
        MatchState Get();
        void Set(MatchState matchState);
    }
}