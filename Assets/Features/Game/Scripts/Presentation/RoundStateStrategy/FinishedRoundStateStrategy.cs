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
            var matchState = _matchStateRepository.Get();

            return round.RoundState == RoundState.Finished && !matchState.IsWaiting();
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

            _matchStateRepository.Set(GameState.StartRound);
            //TODO: need to manually add round, because of how the get round works
            var match = _currentMatchRepository.Get();
            var rn = match.Board.Rounds.Last().RoundNumber;
            match.Board.Rounds.Add(new Round() {RoundNumber = rn + 1});
            _currentMatchRepository.Set(match);
        }
    }
}