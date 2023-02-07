using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Matches.Infrastructure;

namespace Features.ServerLogic.Matches.Action
{
    public class ApplyEffectPreCalculus : IApplyEffectPreCalculus
    {
        private readonly IMatchesRepository _matchesRepository;
        private readonly IGetPlayerPlayedUpgradesInMatch _getPlayerPlayedUpgradesInMatch;
        private readonly IEnumerable<IPreCalculusCardStrategy> _upgradeCardPreCalculusStrategies;

        public ApplyEffectPreCalculus(IMatchesRepository matchesRepository, IGetPlayerPlayedUpgradesInMatch getPlayerPlayedUpgradesInMatch, IEnumerable<IPreCalculusCardStrategy> strategies)
        {
            _matchesRepository = matchesRepository;
            _getPlayerPlayedUpgradesInMatch = getPlayerPlayedUpgradesInMatch;
            _upgradeCardPreCalculusStrategies = strategies;
        }

        public void Execute(string matchId)
        {
            var match = _matchesRepository.Get(matchId);
            foreach (var user in match.Users)
            {
                var upgrades = _getPlayerPlayedUpgradesInMatch.Execute(matchId, user.Id);

                foreach (var upgradeCardPlayed in upgrades)
                {
                    foreach (var strategy in _upgradeCardPreCalculusStrategies)
                    {
                        strategy.Execute(upgradeCardPlayed, matchId, user.Id);
                    }
                }
            }
            _matchesRepository.Update(match);
        }
    }
}