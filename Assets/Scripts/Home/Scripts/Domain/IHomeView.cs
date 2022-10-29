using System;
using UniRx;

namespace Home
{
    public interface IHomeView
    {
        void OnMatchFound(Match.Domain.Match matchStatus);
        void OnError(string message);
        IObservable<Unit> OnPlayMatch();
        IObservable<Unit> OnPlayVersusHardBot();
        IObservable<Unit> OnPlayVersusEasyBot();
    }
}