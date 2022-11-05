using System.Collections.Generic;
using ServerLogic.Cards.Domain.Units;
using ServerLogic.Cards.Domain.Upgrades;

namespace ServerLogic.Matches.Domain
{
    public class Hand
    {
        public IList<UnitCard> UnitsCards;
        public IList<UpgradeCard> UpgradeCards;

        public void PlayCard(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch, string userId) {

        }
    }
}

