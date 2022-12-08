using System.Collections.Generic;
using Features.Data;

namespace Features.Infrastructure
{
    public interface ICardProvider
    {
        IList<UnitCardData> GetUnitCards();
        IList<UpgradeCardData> GetUpgradeCards();

        UnitCardData GetUnitCard(string name);
        UpgradeCardData GetUpgradeCard(string name);
    }
}