using System;
using UniRx;

namespace Home
{
    public interface IHomeView
    {
        void ShowMatchFound(Match.Domain.Match matchStatus);
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