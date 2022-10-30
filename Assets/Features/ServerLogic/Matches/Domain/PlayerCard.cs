using ServerLogic.Cards.Domain.Units;
using ServerLogic.Cards.Domain.Upgrades;

namespace ServerLogic.Matches.Domain
{
    public class PlayerCard
    {
        public UpgradeCard UpgradeCard;
        public UnitCard UnitCard;
        public int UnitCardPower;
    }
}