using System;
using Features.Match.Domain;
using UniRx;

namespace Features.Home.Scripts.Domain
{
    public interface IHomeView
    {
        void ShowMatchFound(GameMatch gameMatchStatus);
        void ShowError(string message);
        void LeftQueue();
        void StartSearchingForMatch();
        IObservable<Unit> OnPlayMatch();
        IObservable<Unit> OnPlayVersusHardBot();
        IObservable<Unit> OnPlayVersusEasyBot();
        IObservable<Unit> OnLeaveQueue();
        IObservable<string> OnPlayVersusFriend();

    }
}