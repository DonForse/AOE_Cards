using System.Linq;

namespace Features.ServerLogic.Cards.Domain.Upgrades
{
    public class PersianTCUpgradeCard : UpgradeCard
    {
        public override void ApplicateEffectPreCalculus(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch, string userId)
        {
            var currentRound = serverMatch.Board.RoundsPlayed.Last();
            var upgrades = serverMatch.GetUpgradeCardsByPlayer(currentRound, userId);
            var power = 0;
            foreach (var upgrade in upgrades) {
                if (upgrade.CardName == CardName)
                    continue;
                power += upgrade.CalculateValue(currentRound.PlayerCards[userId].UnitCard, serverMatch.GetVsUnits(currentRound, userId));
            }
            this.BasePower = power + currentRound.PlayerCards[userId].UnitCard.BasePower;
        }
    }
}

