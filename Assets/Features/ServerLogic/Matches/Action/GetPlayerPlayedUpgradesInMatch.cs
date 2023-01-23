using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Matches.Infrastructure;

namespace Features.ServerLogic.Matches.Action
{
    public class GetPlayerPlayedUpgradesInMatch : IGetPlayerPlayedUpgradesInMatch
    {
        private readonly IMatchesRepository _matchesRepository;
        public GetPlayerPlayedUpgradesInMatch(IMatchesRepository matchesRepository)
        {
            _matchesRepository = matchesRepository;
        }

        public IEnumerable<UpgradeCard> Execute(string matchId, string userId)
        {
            var match = _matchesRepository.Get(matchId);
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