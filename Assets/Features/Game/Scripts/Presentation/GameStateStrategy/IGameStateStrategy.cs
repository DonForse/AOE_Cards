using Infrastructure.Data;

namespace Features.Game.Scripts.Presentation.GameStateStrategy
{
    public interface IGameStateStrategy
    {
        bool IsValid();
        void Execute(Round round);
    }
}