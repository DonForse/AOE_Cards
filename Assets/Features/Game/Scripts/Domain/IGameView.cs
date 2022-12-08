using System;
using System.Collections.Generic;
using Features.Infrastructure.Data;
using Features.Match.Domain;
using UniRx;

namespace Features.Game.Scripts.Domain
{
    public interface IGameView
    {
        public IObservable<(List<string> upgrades, List<string> units)> ReRoll();
        public IObservable<Unit> ApplicationRestoreFocus();
        IObservable<string> UnitCardPlayed();
        IObservable<string> UpgradeCardPlayed();
        IObservable<Unit> ShowRoundUpgradeCompleted();
        IObservable<Unit> UnitShowDownCompleted();
        IObservable<Unit> UpgradeShowDownCompleted();
        void UpdateTimer(Round round);
        void OnRerollComplete(Hand hand);
        void PlayUnitCard(string cardName);
        void PlayUpgradeCard(string cardName);
        void ShowError(string message);
        void StartRound(Round round);
        void ShowRoundUpgrade(Round round);
        void ShowReroll();
        void ShowHand(Hand hand);
        void ToggleView(HandType handType);
        void HideReroll();
        void ShowRivalWaitUpgrade();
        void ShowRivalWaitUnit();
        void EndRound(Round round);
        void Clear();
        void SetupViews(GameMatch gameMatch);
        void ShowUnitCardsPlayedRound(Round round);
        void ShowUpgradeCardsPlayedRound(Round round);
        void EndGame();
        void ClearRound();
        void Log(string s);
    }
}