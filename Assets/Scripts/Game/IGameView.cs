using Infrastructure.Services;

namespace Game
{
    public interface IGameView
    {
        void InitializeGame(Match match);

        void UpgradeCardSentPlay();
        void UnitCardSentPlay(Hand hand);
        void OnGetRoundInfo(Round round);
        
        void OnRerollComplete(Hand hand);


        void ShowError(string message);
    }
}