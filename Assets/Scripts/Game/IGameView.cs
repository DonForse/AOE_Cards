using Game;
using Infrastructure.Services;

public interface IGameView
{
    void ShowRoundUpgradeCard(UpgradeCardData upgradeCard);
    void ShowPlayerHand(Game.Hand _hand);
    void ShowUnitCard();
    void ShowRoundCard();
    void ShowUpgradeCardsPlayedByPlayer(string player);
    void ShowError();
    void UpgradeCardSentPlay();
    void UnitCardSentPlay();
    void CardReveal(RoundResult roundResult);
    void InitializeHand(Hand hand);
}