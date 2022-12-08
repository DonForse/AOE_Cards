using System;
using System.Collections.Generic;
using Features.Game.Scripts.Domain;
using Features.Infrastructure.Data;

namespace Features.Infrastructure.Services
{
    public interface IPlayService
    {
        IObservable<Round> GetRound(int roundNumber);
        IObservable<Hand> PlayUnitCard(string cardName);
        IObservable<Hand> PlayUpgradeCard(string cardName);
        IObservable<Hand> ReRollCards(IList<string> unitCards, IList<string> upgradeCards);
    }
}