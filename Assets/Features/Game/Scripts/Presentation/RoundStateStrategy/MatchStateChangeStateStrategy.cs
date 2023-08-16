using Features.Game.Scripts.Domain;
using Features.Infrastructure.Data;

namespace Features.Game.Scripts.Presentation.RoundStateStrategy
{
    public class MatchStateChangeStateStrategy : IRoundStateStrategy
    {
        private readonly IMatchStateRepository _matchStateRepository;
        private readonly IGameView _gameView;

        public MatchStateChangeStateStrategy(IGameView gameView, IMatchStateRepository matchStateRepository)
        {
            _matchStateRepository = matchStateRepository;
            _gameView = gameView;
        }

        public bool IsValid(Round round)
        {
            var matchState = _matchStateRepository.Get();

            return (round.RoundState == RoundState.Unit && !matchState.IsUnitPhase()) || 
                   (round.RoundState == RoundState.Upgrade && !matchState.IsUpgradePhase());
        }

        public void Execute(Round round)
        {
            _gameView.UpdateTimer(round);
            
            var matchState = _matchStateRepository.Get();

            if (round.RoundState == RoundState.Upgrade && matchState.IsRerollPhase())
            {
                _matchStateRepository.Set(GameState.StartUpgrade);
                _gameView.HideReroll();
                return;
            }
            
            if (round.RoundState == RoundState.Unit && matchState.IsUpgradePhase())
            {
                _matchStateRepository.Set(GameState.UpgradeReveal);
                //en callback de coroutina de la vista
                _gameView.ShowUpgradeCardsPlayedRound(round);
                return;
            }
        }
    }
}