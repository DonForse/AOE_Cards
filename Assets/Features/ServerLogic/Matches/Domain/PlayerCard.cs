using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Cards.Domain.Upgrades;

namespace Features.ServerLogic.Matches.Domain
{
    public class PlayerCard
    {
        public UpgradeCard UpgradeCard;
        public UnitCard UnitCard;
        public int UnitCardPower;
    }
}