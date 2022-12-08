using System;
using UniRx;

namespace Features.Match.Domain
{
    public interface IMatchService
    {
        IObservable<GameMatch> StartMatch(bool vsBot, bool vsFriend, string friendCode, int botDifficulty);
        IObservable<GameMatch> GetMatch();
        IObservable<Unit> RemoveMatch();
        void StopSearch();
    }
}