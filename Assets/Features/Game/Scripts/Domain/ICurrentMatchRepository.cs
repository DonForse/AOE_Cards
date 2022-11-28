using Infrastructure.Data;

namespace Features.Game.Scripts.Domain
{
    public interface ICurrentMatchRepository
    {
        Match.Domain.GameMatch Get();
        void Set(Match.Domain.GameMatch gameMatch);
        void Set(Hand hand);

        void SetRounds(Round round);
    }
}