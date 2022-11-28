using Features.Game.Scripts.Domain;
using Infrastructure.Data;

namespace Features.Game.Scripts.Presentation.RoundStateStrategy
{
    public class StartUpgradeStateStrategy : IRoundStateStrategy
    {
        private readonly IMatchStateRepository _matchStateRepository;
        private readonly IGameView _view;
        private readonly ICurrentMatchRepository _matchRepository;

        public StartUpgradeStateStrategy(IGameView view, IMatchStateRepository matchStateRepository, ICurrentMatchRepository matchRepository)
        {
            _view = view;
            _matchStateRepository = matchStateRepository;
            _matchRepository = matchRepository;
        }

        public bool IsValid() => _matchStateRepository.Get() == MatchState.StartUpgrade;

        public void Execute(Round round)
        {
            //TODO: this is necessary to change it being valid so it only calls once... change. 
            if (round.RoundState == RoundState.Upgrade)
                _matchStateRepository.Set(MatchState.SelectUpgrade);
            _view.ShowHand(_matchRepository.Get().Hand);
            _view.ToggleView(HandType.Upgrade);
        }
    }
}