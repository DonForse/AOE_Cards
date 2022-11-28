using Features.Game.Scripts.Domain;
using Infrastructure.Data;

namespace Features.Game.Scripts.Presentation.GameStateStrategy
{
    public class StartGameStateStrategy : IGameStateStrategy
    {
        private readonly IMatchStateRepository _matchStateRepository;
        private readonly IGameView _view;

        public StartGameStateStrategy(IGameView view, IMatchStateRepository matchStateRepository)
        {
            _view = view;
            _matchStateRepository = matchStateRepository;
        }

        public bool IsValid() => _matchStateRepository.Get() == GameState.StartRound;

        public void Execute(Round round)
        {
            _matchStateRepository.Set(GameState.StartRoundUpgradeReveal);
            _view.StartRound(round);
        }
    }
}