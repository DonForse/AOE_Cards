using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Entities;

namespace Features.ServerLogic.Matches.Action
{
    public interface IGetUpgradeCard
    {
        IList<UpgradeCard> Execute();
        UpgradeCard Execute(string cardName);
    }
}