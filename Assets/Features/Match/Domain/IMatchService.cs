using System;
using Features.Match.Domain;
using UniRx;

namespace Match.Domain
{
    public interface IMatchService
    {
        IObservable<GameMatch> StartMatch(bool vsBot, bool vsFriend, string friendCode, int botDifficulty);
        IObservable<GameMatch> GetMatch();
        IObservable<Unit> RemoveMatch();
        void StopSearch();
    }
}