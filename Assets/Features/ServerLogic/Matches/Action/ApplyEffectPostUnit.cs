using System.Collections;
using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Matches.Infrastructure;

namespace Features.ServerLogic.Matches.Action
{
    public class ApplyEffectPostUnit : IApplyEffectPostUnit
    {
        private readonly IMatchesRepository _matchesRepository;
        private readonly IGetPlayerPlayedUpgradesInMatch _getPlayerPlayedUpgradesInMatch;
        private IEnumerable<IApplicateEffectPostUnitStrategy> _postUnitStrategy;

        public ApplyEffectPostUnit(IMatchesRepository matchesRepository, IGetPlayerPlayedUpgradesInMatch getPlayerPlayedUpgradesInMatch)
        {
            _matchesRepository = matchesRepository;
            _getPlayerPlayedUpgradesInMatch = getPlayerPlayedUpgradesInMatch;
            _postUnitStrategy = new List<IApplicateEffectPostUnitStrategy>
            {
                new MadrasahApplicateEffectPostUnitStrategy(),
                new FurorCelticaApplicateEffectPostUnitStrategy()
            };
        }

        public void Execute(string matchId, string userId)
        {
            var serverMatch = _matchesRepository.Get(matchId);
            var upgrades = _getPlayerPlayedUpgradesInMatch.Execute(matchId, userId);
            var unitCard = serverMatch.Board.CurrentRound.PlayerCards[userId].UnitCard;
            foreach (var upgradeCardPlayed in upgrades)
            {
                foreach (var postUnitStrategy in _postUnitStrategy)
                {
                    if (!postUnitStrategy.IsValid(upgradeCardPlayed)) continue;
                    postUnitStrategy.Execute(serverMatch, userId, unitCard);
                }
            }

            _matchesRepository.Update(serverMatch);
        }
    }
}