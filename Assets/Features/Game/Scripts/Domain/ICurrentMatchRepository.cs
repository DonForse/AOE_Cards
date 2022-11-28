using Game;

namespace Features.Game.Scripts.Presentation
{
    public interface ICurrentMatchRepository
    {
        Match.Domain.GameMatch Get();
        void Set(Match.Domain.GameMatch gameMatch);
        void Set(Hand hand);
    }
}