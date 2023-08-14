using Features.Game.Scripts.Domain;
using Features.Infrastructure.Data;

namespace Features.Game.Scripts.Presentation.RoundStateStrategy
{
    public class UnitRoundStateStrategy : IRoundStateStrategy
    {
        private readonly IMatchStateRepository _matchStateRepository;
        private readonly IGameView _gameView;

        public UnitRoundStateStrategy(IGameView gameView, IMatchStateRepository matchStateRepository)
        {
            _matchStateRepository = matchStateRepository;
            _gameView = gameView;
        }

        public bool IsValid(Round round)
        {
            var matchState = _matchStateRepository.Get();

            return round.RoundState == RoundState.Unit && !matchState.IsUnitPhase();
        }

        public void Execute(Round round)
        {
            _gameView.UpdateTimer(round);

            var matchState = _matchStateRepository.Get();
            if (matchState.IsUpgradePhase())
            {
                _matchStateRepository.Set(GameState.UpgradeReveal);
                //en callback de coroutina de la vista
                _gameView.ShowUpgradeCardsPlayedRound(round);
                return;
            }

            if (round.RivalReady)
            {
                _gameView.ShowRivalWaitUnit();
            }
        }
    }
}