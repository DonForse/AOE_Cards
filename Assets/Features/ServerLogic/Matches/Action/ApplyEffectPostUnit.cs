using System.Collections;
using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Upgrades;

namespace Features.ServerLogic.Matches.Action
{
    public class ApplyEffectPostUnit : IApplyEffectPostUnit
    {
        private readonly IGetMatch _getMatch;
        private IEnumerable<IApplicateEffectPostUnitStrategy> _postUnitStrategy;

        public ApplyEffectPostUnit(IGetMatch getMatch)
        {
            _getMatch = getMatch;
            _postUnitStrategy = new List<IApplicateEffectPostUnitStrategy>
            {
                new MadrasahApplicateEffectPostUnitStrategy(),
                new FurorCelticaApplicateEffectPostUnitStrategy()
            };
        }

        public void Execute(string matchId, string userId)
        {
            var serverMatch = _getMatch.Execute(matchId);
            var upgrades = serverMatch.GetUpgradeCardsByPlayer(userId);
            var unitCard = serverMatch.Board.CurrentRound.PlayerCards[userId].UnitCard;
            foreach (var upgradeCardPlayed in upgrades)
            {
                foreach (var postUnitStrategy in _postUnitStrategy)
                {
                    if (!postUnitStrategy.IsValid(upgradeCardPlayed)) continue;
                    postUnitStrategy.Execute(serverMatch, userId, unitCard);
                }
            }
        }
    }
}