using System;
using UniRx;

namespace Infrastructure.Services
{
    public class OfflineMatchService : IMatchService
    {
        public IObservable<Match> StartMatch(bool vsBot, bool vsFriend, string friendCode, int botDifficulty)
        {
            throw new NotImplementedException();
        }

        public IObservable<Match> GetMatch()
        {
            throw new NotImplementedException();
        }

        public IObservable<Unit> RemoveMatch()
        {
            throw new NotImplementedException();
        }

        public void StopSearch()
        {
            throw new NotImplementedException();
        }
    }
}