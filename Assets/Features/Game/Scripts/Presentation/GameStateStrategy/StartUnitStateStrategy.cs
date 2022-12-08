using Features.Game.Scripts.Domain;
using Features.Infrastructure.Data;

namespace Features.Game.Scripts.Presentation.GameStateStrategy
{
    public class StartUnitStateStrategy : IGameStateStrategy
    {
        private readonly IMatchStateRepository _matchStateRepository;
        private readonly IGameView _view;
        private readonly ICurrentMatchRepository _matchRepository;

        public StartUnitStateStrategy(IGameView view, IMatchStateRepository matchStateRepository, ICurrentMatchRepository matchRepository)
        {
            _view = view;
            _matchStateRepository = matchStateRepository;
            _matchRepository = matchRepository;
        }

        public bool IsValid() => _matchStateRepository.Get() == GameState.StartUnit;

        public void Execute(Round round)
        {
            if (round.RoundState == RoundState.Unit)
                _matchStateRepository.Set(GameState.SelectUnit);
            _view.ShowHand(_matchRepository.Get().Hand);
            _view.ToggleView(HandType.Unit);
        }
    }
}