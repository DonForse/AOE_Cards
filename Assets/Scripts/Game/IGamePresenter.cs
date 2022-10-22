using System.Collections.Generic;
using Infrastructure.Services;

namespace Game
{
    public interface IGamePresenter
    {
        void SetMatch(Match.Domain.Match match);
        void GetRound();
        void PlayUnitCard(string cardName);
        void PlayUpgradeCard(string cardName);
        void SendReroll(IList<string> upgradeCards, IList<string> unitCards);
        void StartNewRound();
    }
}