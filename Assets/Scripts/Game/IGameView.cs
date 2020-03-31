using Infrastructure.Services;

namespace Game
{
    public interface IGameView
    {
        void ShowError(string message);
        void UpgradeCardSentPlay();
        void UnitCardSentPlay();
        void OnGetRoundInfo(Round round);
        void InitializeHand(Hand hand);
        void InitializeRound(Round round);
    }
}