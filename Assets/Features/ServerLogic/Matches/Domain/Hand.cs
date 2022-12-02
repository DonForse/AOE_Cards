using System.Collections.Generic;
using ServerLogic.Cards.Domain.Units;
using ServerLogic.Cards.Domain.Upgrades;

namespace Features.ServerLogic.Matches.Domain
{
    public class Hand
    {
        public IList<UnitCard> UnitsCards;
        public IList<UpgradeCard> UpgradeCards;

        public void PlayCard(ServerMatch serverMatch, string userId) {

        }
    }
}

