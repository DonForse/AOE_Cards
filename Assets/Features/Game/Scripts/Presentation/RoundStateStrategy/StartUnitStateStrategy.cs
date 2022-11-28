using Features.Game.Scripts.Domain;
using Game;
using Infrastructure.Data;

namespace Features.Game.Scripts.Presentation.RoundStateStrategy
{
    public class StartUnitStateStrategy : IRoundStateStrategy
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

        public bool IsValid() => _matchStateRepository.Get() == MatchState.StartUnit;

        public void Execute(Round round)
        {
            if (round.RoundState == RoundState.Unit)
                _matchStateRepository.Set(MatchState.SelectUnit);
            _view.ShowHand(_matchRepository.Get().Hand);
            _view.ToggleView(HandType.Unit);
        }
    }
}