using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Extensions;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Matches.Action
{
    public class CalculateRoundResult : ICalculateRoundResult
    {
        private readonly IMatchesRepository _matchesRepository;
        private readonly List<IUpgradeCardStrategy> _upgradeCardPreCalculusStrategies;

        public CalculateRoundResult(IMatchesRepository matchesRepository)
        {
            _matchesRepository = matchesRepository;
            _upgradeCardPreCalculusStrategies = new List<IUpgradeCardStrategy>();
            _upgradeCardPreCalculusStrategies.Add(new TeutonsFaithUpgradeCardStrategy());
            _upgradeCardPreCalculusStrategies.Add(new PersianTCUpgradeCardStrategy());
        }

        public void Execute(string matchId)
        {
            var match = _matchesRepository.Get(matchId);
            var round = match.Board.CurrentRound;
            var playerOnePower = GetPower(round, round.PlayerCards.Keys.First(), match.Board.RoundsPlayed, match );
            var playerTwoPower = GetPower(round, round.PlayerCards.Keys.Last(), match.Board.RoundsPlayed, match);
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

        private int GetPower(Round round, string player, IList<Round> boardRoundsPlayed, ServerMatch serverMatch)
        {
            var rivalPlayerCard = round.PlayerCards.First(x => x.Key != player).Value;
            var playerCard = round.PlayerCards[player];
            var totalPower = playerCard.UnitCard.BasePower;
            
            totalPower += CalculateUnitVsPower(playerCard.UnitCard, rivalPlayerCard);
            totalPower += CalculateUpgradeCardBasePower(playerCard.UpgradeCard, playerCard, rivalPlayerCard, serverMatch, round, player);
            totalPower += CalculateUpgradeCardBasePower(round.RoundUpgradeCard, playerCard, rivalPlayerCard, serverMatch, round, player);
            foreach (var previousRound in boardRoundsPlayed)
            {
                totalPower += CalculateUpgradeCardBasePower(previousRound.PlayerCards[player].UpgradeCard, playerCard, rivalPlayerCard, serverMatch, round, player);
            }

            return totalPower;
        }

        private int CalculateUnitVsPower(UnitCard playerCardUnitCard, PlayerCard rivalPlayerCard) => 
            playerCardUnitCard.BonusVs.ContainsAnyArchetype(rivalPlayerCard.UnitCard.Archetypes) ? playerCardUnitCard.PowerEffect : 0;

        private int CalculateUpgradeCardBasePower(UpgradeCard upgradeCard, PlayerCard pc, PlayerCard rivalCard, ServerMatch serverMatch, Round round, string userId )
        {
            var power = 0;
            foreach (var strategy in _upgradeCardPreCalculusStrategies)
            {
                if (!strategy.IsValid(upgradeCard))continue;
                power = strategy.Execute(upgradeCard, pc.UnitCard, rivalCard.UnitCard, serverMatch, round, userId);
            }

            if (upgradeCard.Archetypes != null && !pc.UnitCard.Archetypes.Any(uArch => upgradeCard.Archetypes.Any(arch => arch == uArch)))
                return 0 + power;

            if (upgradeCard.BonusVs != null && upgradeCard.BonusVs.Count == 0)
                return upgradeCard.BasePower +power;

            if (upgradeCard.BonusVs != null && !upgradeCard.BonusVs.Any(bonusVs => rivalCard.UnitCard.Archetypes.Any(arq => arq == bonusVs)))
                return 0;
            return upgradeCard.BasePower + power;
        }
    }
}