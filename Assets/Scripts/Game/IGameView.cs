using Game;
using Infrastructure.Services;

public interface IGameView
{
    void ShowUpgradeCardsPlayedByPlayer(string player);
    void ShowError(string message);
    void UpgradeCardSentPlay();
    void UnitCardSentPlay();
    void OnGetRoundInfo(Round round);
    void InitializeHand(Hand hand);
    void InitializeRound(Round round);
}