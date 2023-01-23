using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Matches.Infrastructure;

namespace Features.ServerLogic.Matches.Action
{
    public class ApplyEffectPreCalculus : IApplyEffectPostUnit
    {
        private readonly IMatchesRepository _matchesRepository;
        private readonly IGetPlayerPlayedUpgradesInMatch _getPlayerPlayedUpgradesInMatch;
        private IEnumerable<IApplicateEffectPostUnitStrategy> _postUnitStrategy;
        private readonly IEnumerable<IPreCalculusCardStrategy> _upgradeCardPreCalculusStrategies;

        public ApplyEffectPreCalculus(IMatchesRepository matchesRepository, IGetPlayerPlayedUpgradesInMatch getPlayerPlayedUpgradesInMatch)
        {
            _matchesRepository = matchesRepository;
            _getPlayerPlayedUpgradesInMatch = getPlayerPlayedUpgradesInMatch;
            _upgradeCardPreCalculusStrategies = new List<IPreCalculusCardStrategy>
            {
                new TeutonsFaithPreCalculusCardStrategy(),
                new PersianTcPreCalculusCardStrategy(_getPlayerPlayedUpgradesInMatch)
            };
        }

        public void Execute(string matchId, string userId)
        {
            var match = _matchesRepository.Get(matchId);
            var currentRound = match.Board.CurrentRound;
            foreach (var user in match.Users)
            {
                var upgrades = _getPlayerPlayedUpgradesInMatch.Execute(matchId, user.Id);

                foreach (var upgradeCardPlayed in upgrades)
                {
                    foreach (var strategy in _upgradeCardPreCalculusStrategies)
                    {
                        if (!strategy.IsValid(upgradeCardPlayed)) continue;
                        var rivalCard = currentRound.PlayerCards.First(x => x.Key != user.Id);
                        strategy.Execute(upgradeCardPlayed, currentRound.PlayerCards[user.Id].UnitCard,
                            rivalCard.Value.UnitCard, match, currentRound, user.Id);
                    }
                }
            }
            _matchesRepository.Update(match);
        }
    }
}