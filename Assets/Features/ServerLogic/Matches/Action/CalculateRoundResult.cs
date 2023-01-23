using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Domain.Entities;
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
        private readonly IGetPlayerPlayedUpgradesInMatch _getPlayerPlayedUpgradesInMatch;

        public CalculateRoundResult(IMatchesRepository matchesRepository, IGetPlayerPlayedUpgradesInMatch getPlayerPlayedUpgradesInMatch)
        {
            _matchesRepository = matchesRepository;
            _getPlayerPlayedUpgradesInMatch = getPlayerPlayedUpgradesInMatch;
            _upgradeCardPreCalculusStrategies = new List<IPreCalculusCardStrategy>();
            _upgradeCardPreCalculusStrategies.Add(new TeutonsFaithPreCalculusCardStrategy());
            _upgradeCardPreCalculusStrategies.Add(new PersianTcPreCalculusCardStrategy(_getPlayerPlayedUpgradesInMatch));
        }

        public void Execute(string matchId)
        {
            var match = _matchesRepository.Get(matchId);
            var currentRound = match.Board.CurrentRound;

            ApplyPreCalculus(matchId, match, currentRound);

            var playerOnePower = GetPower(currentRound, currentRound.PlayerCards.Keys.First(), match.Board.RoundsPlayed);
            var playerTwoPower = GetPower(currentRound, currentRound.PlayerCards.Keys.Last(), match.Board.RoundsPlayed);
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
            
            _matchesRepository.Update(match);
        }

        private void ApplyPreCalculus(string matchId, ServerMatch match, Round currentRound)
        {
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
        }

        private int GetPower(Round currentRound, string player, IList<Round> boardRoundsPlayed)
        {
            var rivalPlayerCard = currentRound.PlayerCards.First(x => x.Key != player).Value;
            var playerCard = currentRound.PlayerCards[player];
            var totalPower = playerCard.UnitCard.basePower;

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
            playerCardUnitCard.bonusVs.ContainsAnyArchetype(rivalPlayerCard.UnitCard.archetypes)
                ? playerCardUnitCard.powerEffect
                : 0;

        private int CalculateUpgradeCardBasePower(UpgradeCard upgradeCard, PlayerCard pc, PlayerCard rivalCard)
        {

            if (upgradeCard.archetypes != null &&
                !pc.UnitCard.archetypes.Any(uArch => upgradeCard.archetypes.Any(arch => arch == uArch)))
                return 0;

            if (upgradeCard.bonusVs != null && !upgradeCard.bonusVs.Any())
                return upgradeCard.basePower;

            if (upgradeCard.bonusVs != null &&
                !upgradeCard.bonusVs.Any(bonusVs => rivalCard.UnitCard.archetypes.Any(arq => arq == bonusVs)))
                return 0;
            return upgradeCard.basePower;
        }
    }
}