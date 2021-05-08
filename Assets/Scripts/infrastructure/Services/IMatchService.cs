using System;
using System.Net.Http;
using UniRx;

namespace Infrastructure.Services
{
    public interface IMatchService
    {
        IObservable<Match> StartMatch(bool vsBot, bool vsFriend, string friendCode, int botDifficulty);
        IObservable<Match> GetMatch();
        IObservable<Unit> RemoveMatch();
        void StopSearch();
    }
}