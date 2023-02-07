using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Entities;

namespace Features.ServerLogic.Cards.Domain
{
    public interface ICardRepository
    {
        IList<UnitCard> GetUnitCards();
        IList<UpgradeCard> GetUpgradeCards();

        UnitCard GetUnitCard(string cardName);
        UpgradeCard GetUpgradeCard(string cardName);
    }
}