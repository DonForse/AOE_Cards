using Features.Game.Scripts.Domain;
using Game;
using Infrastructure.Data;

namespace Features.Game.Scripts.Presentation.RoundStateStrategy
{
    public class StartRoundUpgradeRevealStateStrategy : IRoundStateStrategy
    {
        private readonly IMatchStateRepository _matchStateRepository;
        private readonly IGameView _view;

        public StartRoundUpgradeRevealStateStrategy(IGameView view, IMatchStateRepository matchStateRepository)
        {
            _view = view;
            _matchStateRepository = matchStateRepository;
        }

        public bool IsValid() => _matchStateRepository.Get() == MatchState.StartRoundUpgradeReveal;

        public void Execute(Round round)
        {
            _matchStateRepository.Set(MatchState.WaitRoundUpgradeReveal);
            _view.ShowRoundUpgrade(round);
        }
    }
}