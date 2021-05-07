using Game;
using System;
using System.Collections.Generic;

namespace Infrastructure.Services
{
    public interface IPlayService
    {
        IObservable<Round> GetRound(int roundNumber);
        IObservable<Hand> PlayUnitCard(string cardName);
        IObservable<Hand> PlayUpgradeCard(string cardName);
        IObservable<Hand> RerollCards(IList<string> unitCards, IList<string> upgradeCards);
    }
}