using Features.Game.Scripts.Domain;
using Infrastructure.Data;

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

        public bool IsValid(Round round) => round.RoundState == RoundState.Reroll;

        public void Execute(Round round)
        {
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