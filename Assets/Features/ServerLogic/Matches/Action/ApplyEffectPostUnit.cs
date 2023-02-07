using System.Collections.Generic;
using Features.ServerLogic.Game.Domain;

namespace Features.ServerLogic.Matches.Action
{
    public class ApplyEffectPostUnit : IApplyEffectPostUnit
    {
        private readonly IGetPlayerPlayedUpgradesInMatch _getPlayerPlayedUpgradesInMatch;
        private readonly IEnumerable<IApplyEffectPostUnitStrategy> _postUnitStrategy;

        public ApplyEffectPostUnit(IGetPlayerPlayedUpgradesInMatch getPlayerPlayedUpgradesInMatch,
            IEnumerable<IApplyEffectPostUnitStrategy> strategies)
        {
            _getPlayerPlayedUpgradesInMatch = getPlayerPlayedUpgradesInMatch;
            _postUnitStrategy = strategies;
        }

        public void Execute(string matchId, string userId)
        {
            var upgrades = _getPlayerPlayedUpgradesInMatch.Execute(matchId, userId);
            foreach (var upgradeCardPlayed in upgrades)
            {
                foreach (var postUnitStrategy in _postUnitStrategy)
                {
                    postUnitStrategy.Execute(upgradeCardPlayed, matchId, userId);
                }
            }
        }
    }
}