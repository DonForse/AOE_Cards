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
        private readonly List<IPreCalculusCardStrategy> _upgradeCardPreCalculusStrategies;

        public CalculateRoundResult(IMatchesRepository matchesRepository)
        {
            _matchesRepository = matchesRepository;
            _upgradeCardPreCalculusStrategies = new List<IPreCalculusCardStrategy>();
            _upgradeCardPreCalculusStrategies.Add(new TeutonsFaithPreCalculusCardStrategy());
            _upgradeCardPreCalculusStrategies.Add(new PersianTcPreCalculusCardStrategy());
        }

        public void Execute(string matchId)
        {
            var match = _matchesRepository.Get(matchId);
            var currentRound = match.Board.CurrentRound;

            foreach (var user in match.Users)
            {
                foreach (var upgradeCardPlayed in match.GetUpgradeCardsByPlayer(user.Id))
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

            var playerOnePower = GetPower(currentRound, currentRound.PlayerCards.Keys.First(), match.Board.RoundsPlayed,
                match);
            var playerTwoPower = GetPower(currentRound, currentRound.PlayerCards.Keys.Last(), match.Board.RoundsPlayed,
                match);
            currentRound.PlayerWinner = new List<User>();
            if (playerOnePower == playerTwoPower)
            {
                currentRound.PlayerWinner.Add(match.Users.First(x => x.Id == currentRound.PlayerCards.Keys.First()));
                currentRound.PlayerWinner.Add(match.Users.First(x => x.Id == currentRound.PlayerCards.Keys.Last()));
            }
            else if (playerOnePower > playerTwoPower)
            {
                currentRound.PlayerWinner.Add(match.Users.First(x => x.Id == currentRound.PlayerCards.Keys.First()));
            }
            else
            {
                currentRound.PlayerWinner.Add(match.Users.First(x => x.Id == currentRound.PlayerCards.Keys.Last()));
            }

            //hotfix:
            currentRound.PlayerCards[currentRound.PlayerCards.Keys.First()].UnitCardPower = playerOnePower;
            currentRound.PlayerCards[currentRound.PlayerCards.Keys.Last()].UnitCardPower = playerTwoPower;
        }

        private int GetPower(Round currentRound, string player, IList<Round> boardRoundsPlayed, ServerMatch serverMatch)
        {
            var rivalPlayerCard = currentRound.PlayerCards.First(x => x.Key != player).Value;
            var playerCard = currentRound.PlayerCards[player];
            var totalPower = playerCard.UnitCard.BasePower;

            totalPower += CalculateUnitVsPower(playerCard.UnitCard, rivalPlayerCard);
            totalPower += CalculateUpgradeCardBasePower(playerCard.UpgradeCard, playerCard, rivalPlayerCard);
            totalPower += CalculateUpgradeCardBasePower(currentRound.RoundUpgradeCard, playerCard, rivalPlayerCard);
            foreach (var previousRound in boardRoundsPlayed)
            {
                totalPower += CalculateUpgradeCardBasePower(previousRound.PlayerCards[player].UpgradeCard, playerCard,
                    rivalPlayerCard);
            }

            return totalPower;
        }

        private int CalculateUnitVsPower(UnitCard playerCardUnitCard, PlayerCard rivalPlayerCard) =>
            playerCardUnitCard.BonusVs.ContainsAnyArchetype(rivalPlayerCard.UnitCard.Archetypes)
                ? playerCardUnitCard.PowerEffect
                : 0;

        private int CalculateUpgradeCardBasePower(UpgradeCard upgradeCard, PlayerCard pc, PlayerCard rivalCard)
        {
            var power = 0;

            if (upgradeCard.Archetypes != null &&
                !pc.UnitCard.Archetypes.Any(uArch => upgradeCard.Archetypes.Any(arch => arch == uArch)))
                return 0 + power;

            if (upgradeCard.BonusVs != null && upgradeCard.BonusVs.Count == 0)
                return upgradeCard.BasePower + power;

            if (upgradeCard.BonusVs != null &&
                !upgradeCard.BonusVs.Any(bonusVs => rivalCard.UnitCard.Archetypes.Any(arq => arq == bonusVs)))
                return 0;
            return upgradeCard.BasePower + power;
        }
    }
}