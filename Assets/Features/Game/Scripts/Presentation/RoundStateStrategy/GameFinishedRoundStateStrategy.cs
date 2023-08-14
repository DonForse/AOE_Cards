using Features.Game.Scripts.Domain;
using Features.Infrastructure.Data;

namespace Features.Game.Scripts.Presentation.RoundStateStrategy
{
    public class GameFinishedRoundStateStrategy : IRoundStateStrategy
    {
        private readonly IMatchStateRepository _matchStateRepository;
        private readonly ICurrentMatchRepository _currentMatchRepository;
        private readonly IGameView _gameView;

        public GameFinishedRoundStateStrategy(IGameView gameView, IMatchStateRepository matchStateRepository,
            ICurrentMatchRepository currentMatchRepository)
        {
            _matchStateRepository = matchStateRepository;
            _currentMatchRepository = currentMatchRepository;
            _gameView = gameView;
        }

        public bool IsValid(Round round)
        {
            var matchState = _matchStateRepository.Get();

            return round.RoundState == RoundState.GameFinished && !matchState.IsWaiting();
        }

        public void Execute(Round round)
        {
            var matchState = _matchStateRepository.Get();
            
            if (matchState.IsUnitPhase())
            {
                _matchStateRepository.Set(GameState.RoundResultReveal);
                _gameView.ShowUnitCardsPlayedRound(round);
                return;
            }
            _gameView.EndRound(round);
            _gameView.EndGame();
        }
    }
}