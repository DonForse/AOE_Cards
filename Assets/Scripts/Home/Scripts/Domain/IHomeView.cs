using System;
using UniRx;

namespace Home
{
    public interface IHomeView
    {
        void ShowMatchFound(Match.Domain.Match matchStatus);
        void ShowError(string message);
        void LeftQueue();
        IObservable<Unit> OnPlayMatch();
        IObservable<Unit> OnPlayVersusHardBot();
        IObservable<Unit> OnPlayVersusEasyBot();
        IObservable<Unit> OnLeaveQueue();
    }
}