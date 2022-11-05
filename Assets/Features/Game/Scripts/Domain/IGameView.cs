using System;
using System.Collections.Generic;
using Data;
using Game;
using Infrastructure.Data;

namespace Features.Game.Scripts.Domain
{
    public interface IGameView
    {
        public IObservable<string> PlayCard();
        public IObservable<(List<string> upgrades, List<string> units)> ReRoll();
        void OnGetRoundInfo(Round round);
        void OnRerollComplete(Hand hand);
        void OnUnitCardPlayed();
        void OnUpgradeCardPlayed();
        void ShowError(string message);
        IObservable<string> UnitCardPlayed();
        IObservable<string> UpgradeCardPlayed();
    }
}