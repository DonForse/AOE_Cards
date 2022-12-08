using Features.Game.Scripts.Domain;
using Features.Infrastructure.Data;

namespace Features.Game.Scripts.Presentation.GameStateStrategy
{
    public class StartGameUpgradeRevealStateStrategy : IGameStateStrategy
    {
        private readonly IMatchStateRepository _matchStateRepository;
        private readonly IGameView _view;

        public StartGameUpgradeRevealStateStrategy(IGameView view, IMatchStateRepository matchStateRepository)
        {
            _view = view;
            _matchStateRepository = matchStateRepository;
        }

        public bool IsValid() => _matchStateRepository.Get() == GameState.StartRoundUpgradeReveal;

        public void Execute(Round round)
        {
            _matchStateRepository.Set(GameState.WaitRoundUpgradeReveal);
            _view.ShowRoundUpgrade(round);
        }
    }
}