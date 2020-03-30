using System.Collections.Generic;

public interface ICardProvider
{
    IList<UnitCardData> GetUnitCards();
    IList<UpgradeCardData> GetUpgradeCards();

    UnitCardData GetUnitCard(string name);
    UpgradeCardData GetUpgradeCard(string name);
}