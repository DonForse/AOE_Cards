using Infrastructure.Services;
using System.Collections;

namespace Game
{
    public interface IGameView
    {
        void ShowError(string message);
        void UpgradeCardSentPlay();
        void UnitCardSentPlay(Hand hand);
        void OnGetRoundInfo(Round round);
        void InitializeGame(Match match);
    }
}