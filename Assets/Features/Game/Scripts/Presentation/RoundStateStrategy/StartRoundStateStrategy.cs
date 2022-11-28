using Features.Game.Scripts.Domain;
using Game;
using Infrastructure.Data;

namespace Features.Game.Scripts.Presentation.RoundStateStrategy
{
    public class StartRoundStateStrategy : IRoundStateStrategy
    {
        private readonly IMatchStateRepository _matchStateRepository;
        private readonly IGameView _view;

        public StartRoundStateStrategy(IGameView view, IMatchStateRepository matchStateRepository)
        {
            _view = view;
            _matchStateRepository = matchStateRepository;
        }

        public bool IsValid() => _matchStateRepository.Get() == MatchState.StartRound;

        public void Execute(Round round)
        {
            _view.StartRound(round);
            _matchStateRepository.Set(MatchState.StartRoundUpgradeReveal);
        }
    }
}