using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Cards.Domain.Upgrades;

namespace Features.ServerLogic.Cards.Infrastructure
{
    public interface ICardRepository
    {
        IList<UnitCard> GetUnitCards();
        IList<UpgradeCard> GetUpgradeCards();

        UnitCard GetUnitCard(string cardName);
        UpgradeCard GetUpgradeCard(string cardName);
    }
}