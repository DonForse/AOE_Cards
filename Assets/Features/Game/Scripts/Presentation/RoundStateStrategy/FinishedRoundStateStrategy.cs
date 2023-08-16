using System.Linq;
using Features.Game.Scripts.Domain;
using Features.Infrastructure.Data;

namespace Features.Game.Scripts.Presentation.RoundStateStrategy
{
    public class FinishedRoundStateStrategy : IRoundStateStrategy
    {
        private readonly IMatchStateRepository _matchStateRepository;
        private readonly ICurrentMatchRepository _currentMatchRepository;
        private readonly IGameView _gameView;

        public FinishedRoundStateStrategy(IGameView gameView, IMatchStateRepository matchStateRepository,
            ICurrentMatchRepository currentMatchRepository)
        {
            _matchStateRepository = matchStateRepository;
            _currentMatchRepository = currentMatchRepository;
            _gameView = gameView;
        }

        public bool IsValid(Round round)
        {
            return round.RoundState == RoundState.Finished;
        }

        public void Execute(Round round)
        {
            _gameView.UpdateTimer(round);
            _matchStateRepository.Set(GameState.RoundResultReveal);
            _gameView.ShowUnitCardsPlayedRound(round);
        }
    }
}