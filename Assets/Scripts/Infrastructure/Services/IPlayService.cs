using Game;
using System;
using System.Collections.Generic;

namespace Infrastructure.Services
{
    public interface IPlayService
    {
        void GetRound(int roundNumber, Action<Round> onGetRoundComplete, Action<long, string> onError);
        void PlayUnitCard(string cardName, Action<Hand> onUnitCardFinished, Action<long, string> onError);
        void PlayUpgradeCard(string cardName, Action<Hand> onUnitCardFinished, Action<long, string> onError);
        void RerollCards(List<string> unitCards, List<string> upgradeCards, Action<Hand> onRerollFinished, Action<long, string> onError)
    }
}