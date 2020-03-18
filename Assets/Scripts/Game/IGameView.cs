public interface IGameView
{
    void ShowRoundUpgradeCard(UpgradeCardData upgradeCard);
    void ShowHand();
    void ShowUnitCard();
    void ShowRoundCard();
    void ShowUpgradeCardsPlayedByPlayer(string player);
    void ShowError();
    void UpgradeCardSentPlay();
    void UnitCardSentPlay();
}