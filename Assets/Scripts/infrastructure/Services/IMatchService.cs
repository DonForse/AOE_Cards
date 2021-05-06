using System;
using UniRx;

namespace Infrastructure.Services
{
    public interface IMatchService
    {
        IObservable<Match> StartMatch(bool vsBot, bool vsFriend, string friendCode, int botDifficulty);
        void GetMatch(Action<Match> onStartMatchComplete, Action<long, string> onError);
        IObservable<Unit> RemoveMatch();
    }
}