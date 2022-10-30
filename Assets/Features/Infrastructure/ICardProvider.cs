using System.Collections.Generic;
using Data;

namespace Infrastructure
{
    public interface ICardProvider
    {
        IList<UnitCardData> GetUnitCards();
        IList<UpgradeCardData> GetUpgradeCards();

        UnitCardData GetUnitCard(string name);
        UpgradeCardData GetUpgradeCard(string name);
    }
}