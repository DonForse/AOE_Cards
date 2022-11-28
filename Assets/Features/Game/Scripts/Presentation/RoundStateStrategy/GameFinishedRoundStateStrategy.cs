using Features.Game.Scripts.Domain;
using Infrastructure.Data;

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

        public bool IsValid(Round round) => round.RoundState == RoundState.GameFinished;

        public void Execute(Round round)
        {
            var matchState = _matchStateRepository.Get();
            
            if (matchState.IsUnitPhase())
            {
                _matchStateRepository.Set(GameState.RoundResultReveal);
                _gameView.ShowUnitCardsPlayedRound(round, () => { _matchStateRepository.Set(GameState.StartRound); });
                return;
            }
            _gameView.EndRound(round);
            _gameView.EndGame();
        }
    }
}