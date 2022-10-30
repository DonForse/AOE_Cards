using System.Linq;

namespace ServerLogic.Cards.Domain.Upgrades
{
    public class PersianTCUpgradeCard : UpgradeCard
    {
        public override void ApplicateEffectPreCalculus(Matches.Domain.Match match, string userId)
        {
            var currentRound = match.Board.RoundsPlayed.Last();
            var upgrades = match.GetUpgradeCardsByPlayer(currentRound, userId);
            var power = 0;
            foreach (var upgrade in upgrades) {
                if (upgrade.CardName == CardName)
                    continue;
                power += upgrade.CalculateValue(currentRound.PlayerCards[userId].UnitCard, match.GetVsUnits(currentRound, userId));
            }
            this.BasePower = power + currentRound.PlayerCards[userId].UnitCard.BasePower;
        }
    }
}

