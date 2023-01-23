using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Entities;

namespace Features.ServerLogic.Cards.Actions
{
    public interface IGetUnitCard
    {
        IList<UnitCard> Execute(bool withVillager);
        UnitCard Execute(string cardName);
    }
}