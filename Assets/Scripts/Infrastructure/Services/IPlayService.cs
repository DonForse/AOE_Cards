using Game;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public interface IPlayService
    {
        void GetRound(int roundNumber, Action<Round> onGetRoundComplete, Action<string> onError);
        void PlayUnitCard(string cardName, Action<Hand> onUnitCardFinished, Action<string> onError);
        void PlayUpgradeCard(string cardName, Action<Hand> onUnitCardFinished, Action<string> onError);
    }
}