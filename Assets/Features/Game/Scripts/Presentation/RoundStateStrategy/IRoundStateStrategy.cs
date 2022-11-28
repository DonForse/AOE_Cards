using Infrastructure.Data;

namespace Features.Game.Scripts.Presentation.RoundStateStrategy
{
    public interface IRoundStateStrategy
    {
        bool IsValid(Round round);
        void Execute(Round round);
    }
}