using System;
using UniRx;

namespace Match.Domain
{
    public interface IMatchService
    {
        IObservable<Match> StartMatch(bool vsBot, bool vsFriend, string friendCode, int botDifficulty);
        IObservable<Match> GetMatch();
        IObservable<Unit> RemoveMatch();
        void StopSearch();
    }
}