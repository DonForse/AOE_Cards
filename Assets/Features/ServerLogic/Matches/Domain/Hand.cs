using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Cards.Domain.Upgrades;

namespace Features.ServerLogic.Matches.Domain
{
    public class Hand
    {
        public IList<UnitCard> UnitsCards;
        public IList<UpgradeCard> UpgradeCards;
    }
}

