using Features.Game.Scripts.Domain;
using Infrastructure.Data;

namespace Features.Game.Scripts.Presentation.GameStateStrategy
{
    public class StartRerollStateStrategy : IGameStateStrategy
    {
        private readonly IMatchStateRepository _matchStateRepository;
        private readonly IGameView _view;

        public StartRerollStateStrategy(IGameView view, IMatchStateRepository matchStateRepository)
        {
            _view = view;
            _matchStateRepository = matchStateRepository;
        }

        public bool IsValid() => _matchStateRepository.Get() == GameState.StartReroll;

        public void Execute(Round round)
        {
            _matchStateRepository.Set(GameState.SelectReroll);
            _view.ShowReroll();
        }
    }
}