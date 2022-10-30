using System.Collections.Generic;
using ServerLogic.Cards.Domain.Units;
using ServerLogic.Cards.Domain.Upgrades;

namespace ServerLogic.Cards.Infrastructure
{
    public interface ICardRepository
    {
        IList<UnitCard> GetUnitCards();
        IList<UpgradeCard> GetUpgradeCards();

        UnitCard GetUnitCard(string cardName);
        UpgradeCard GetUpgradeCard(string cardName);
    }
}