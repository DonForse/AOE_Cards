using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Domain.Upgrades;

namespace Features.ServerLogic.Matches.Action
{
    public class GetPlayerPlayedUpgradesInMatch : IGetPlayerPlayedUpgradesInMatch
    {
        private readonly IGetMatch _getMatch;
        public GetPlayerPlayedUpgradesInMatch(IGetMatch getMatch)
        {
            _getMatch = getMatch;
        }

        public IEnumerable<UpgradeCard> Execute(string matchId, string userId)
        {
            var match = _getMatch.Execute(matchId);
            var upgradeCards = match.Board.RoundsPlayed.SelectMany(r => r.PlayerCards
                    .Where(pc => pc.Key == userId && pc.Value.UpgradeCard != null)
                    .Select(pc => pc.Value.UpgradeCard))
                .Concat(new[] { match.Board.CurrentRound.PlayerCards[userId].UpgradeCard })
                .Where(upgrade => upgrade != null)
                .ToList();
            upgradeCards.Add(match.Board.CurrentRound.RoundUpgradeCard);
            return upgradeCards;
        }
    }
}