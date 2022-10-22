using System;
using Infrastructure;
using Match.Domain;
using UniRx;

namespace Match
{
    public class OfflineMatchService : IMatchService
    {
        public OfflineMatchService(ICardProvider cardProvider)
        {
            throw new NotImplementedException();
        }

        public IObservable<Domain.Match> StartMatch(bool vsBot, bool vsFriend, string friendCode, int botDifficulty)
        {
            throw new NotImplementedException();
        }

        public IObservable<Domain.Match> GetMatch()
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