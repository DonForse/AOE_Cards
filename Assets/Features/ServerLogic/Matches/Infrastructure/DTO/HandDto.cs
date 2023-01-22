using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Matches.Domain;
using NSubstitute.ReturnsExtensions;

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
            if (hand == null)
                return;
            units = hand.UnitsCards.Select(unit => unit.CardName).ToList();
            upgrades = hand.UpgradeCards.Select(upgrade => upgrade.cardName).ToList();
        }
    }
}