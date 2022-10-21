using System;
using System.Collections.Generic;
using Game;

namespace Infrastructure.Services
{
    public class OfflinePlayService : IPlayService
    {
        public IObservable<Round> GetRound(int roundNumber)
        {
            throw new NotImplementedException();
        }

        public IObservable<Hand> PlayUnitCard(string cardName)
        {
            throw new NotImplementedException();
        }

        public IObservable<Hand> PlayUpgradeCard(string cardName)
        {
            throw new NotImplementedException();
        }

        public IObservable<Hand> RerollCards(IList<string> unitCards, IList<string> upgradeCards)
        {
            throw new NotImplementedException();
        }
    }
}