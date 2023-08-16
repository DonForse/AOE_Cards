using Features.Game.Scripts.Domain;
using Features.Infrastructure.Data;

namespace Features.Game.Scripts.Presentation.RoundStateStrategy
{
    public class RerollRoundStateStrategy : IRoundStateStrategy
    {
        private readonly IMatchStateRepository _matchStateRepository;
        private readonly IGameView _gameView;

        public RerollRoundStateStrategy(IGameView gameView, IMatchStateRepository matchStateRepository)
        {
            _matchStateRepository = matchStateRepository;
            _gameView = gameView;
        }

        public bool IsValid(Round round)
        {
            var matchState = _matchStateRepository.Get();
            return round.RoundState == RoundState.Reroll && matchState.IsRerollPhase();
        }

        public void Execute(Round round)
        {
            _gameView.UpdateTimer(round);

            _gameView.Log($"HasReroll:{round.HasReroll}");
            if (round.HasReroll)
            {
                _matchStateRepository.Set(GameState.SelectReroll);
                _gameView.ShowReroll();
            }
            else
            {
                _matchStateRepository.Set(GameState.StartUpgrade);
            }
        }
    }
}