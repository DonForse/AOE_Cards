using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Entities;

namespace Features.ServerLogic.Game.Domain.Entities
{
    public class Hand
    {
        public IList<UnitCard> UnitsCards;
        public IList<UpgradeCard> UpgradeCards;
    }
}

