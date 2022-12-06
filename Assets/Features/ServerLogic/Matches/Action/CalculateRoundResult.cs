using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Extensions;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Matches.Action
{
    public class CalculateRoundResult
    {
        private readonly IMatchesRepository _matchesRepository;

        public CalculateRoundResult(IMatchesRepository matchesRepository)
        {
            _matchesRepository = matchesRepository;
        }

        public void Execute(string matchId)
        {
            var match = _matchesRepository.Get(matchId);
            var round = match.Board.CurrentRound;
            var playerOnePower = GetPower(round, round.PlayerCards.Keys.First(), match.Board.RoundsPlayed);
            var playerTwoPower = GetPower(round, round.PlayerCards.Keys.Last(), match.Board.RoundsPlayed);
            round.PlayerWinner = new List<User>();
            if (playerOnePower == playerTwoPower)
            {
                round.PlayerWinner.Add(match.Users.First(x => x.Id == round.PlayerCards.Keys.First()));
                round.PlayerWinner.Add(match.Users.First(x => x.Id == round.PlayerCards.Keys.Last()));
            }
            else if (playerOnePower > playerTwoPower)
            {
                round.PlayerWinner.Add(match.Users.First(x => x.Id == round.PlayerCards.Keys.First()));
            }
            else
            {
                round.PlayerWinner.Add(match.Users.First(x => x.Id == round.PlayerCards.Keys.Last()));
            }
        }

        private int GetPower(Round round, string player, IList<Round> boardRoundsPlayed)
        {
            var rivalPlayerCard = round.PlayerCards.First(x => x.Key != player).Value;
            var playerCard = round.PlayerCards[player];
            var totalPower = playerCard.UnitCard.BasePower;
            totalPower += CalculateUpgradeCardBasePower(playerCard.UpgradeCard, playerCard, rivalPlayerCard);
            totalPower += CalculateUpgradeCardBasePower(round.RoundUpgradeCard, playerCard, rivalPlayerCard);
            foreach (var previousRound in boardRoundsPlayed)
            {
                totalPower += CalculateUpgradeCardBasePower(previousRound.PlayerCards[player].UpgradeCard, playerCard,
                    rivalPlayerCard);
            }

            return totalPower;
        }

        private static int CalculateUpgradeCardBasePower(UpgradeCard upgradeCard, PlayerCard pc, PlayerCard rivalCard)
        {
            return upgradeCard.Archetypes.ContainsAnyArchetype(pc.UnitCard.Archetypes) &&
                   (upgradeCard.BonusVs == null
                    || upgradeCard.BonusVs.ContainsAnyArchetype(rivalCard.UnitCard.Archetypes))
                ? upgradeCard.BasePower
                : 0;
        }
    }
}