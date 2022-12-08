using Features.Game.Scripts.Domain;
using Features.Infrastructure.Data;

namespace Features.Game.Scripts.Presentation.RoundStateStrategy
{
    public class UpgradeRoundStateStrategy : IRoundStateStrategy
    {
        private readonly IMatchStateRepository _matchStateRepository;
        private readonly IGameView _gameView;

        public UpgradeRoundStateStrategy(IGameView gameView, IMatchStateRepository matchStateRepository)
        {
            _matchStateRepository = matchStateRepository;
            _gameView = gameView;
        }

        public bool IsValid(Round round) => round.RoundState == RoundState.Upgrade;

        public void Execute(Round round)
        {
            var matchState = _matchStateRepository.Get();
            
            if (round.RivalReady) 
                _gameView.ShowRivalWaitUpgrade();
            
            if (matchState.IsUpgradePhase())
                return;
            
            if (matchState.IsRerollPhase())
            {
                _matchStateRepository.Set(GameState.StartUpgrade);
                _gameView.HideReroll();
            }
        }
    }
}