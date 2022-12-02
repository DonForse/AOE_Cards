using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Matches.Infrastructure.DTO
{
    public class HandDto
    {
        public IList<string> units;
        public IList<string> upgrades;

        public HandDto() {

        }
        public HandDto(Hand hand)
        {
            units = hand.UnitsCards.Select(unit => unit.CardName).ToList();
            upgrades = hand.UpgradeCards.Select(upgrade => upgrade.CardName).ToList();
        }
    }
}