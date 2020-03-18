using System.Collections.Generic;

public interface ICardProvider
{
    IList<UnitCardData> GetUnitCards();
    IList<UpgradeCardData> GetUpgradeCards();
}