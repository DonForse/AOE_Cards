using Infrastructure.Data;

namespace Features.Game.Scripts.Presentation.RoundStateStrategy
{
    public interface IRoundStateStrategy
    {
        bool IsValid();
        void Execute(Round round);
    }
}