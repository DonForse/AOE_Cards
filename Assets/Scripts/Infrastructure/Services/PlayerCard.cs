namespace Infrastructure.Services
{
    public class PlayerCard
    {
        public string Player;
        public UpgradeCardData UpgradeCardData;
        public UnitCardData UnitCardData;

        public int UnitCardPower { get; internal set; }
    }
}