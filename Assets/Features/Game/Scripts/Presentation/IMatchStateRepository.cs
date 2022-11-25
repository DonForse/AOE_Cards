using Game;

namespace Features.Game.Scripts.Presentation
{
    public interface IMatchStateRepository
    {
        MatchState Get();
        void Set(MatchState matchState);
    }
}