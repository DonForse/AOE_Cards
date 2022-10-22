using System.Collections.Generic;

namespace ServerLogic.Matches.Infrastructure
{
    public interface IMatchesRepository
    {
        bool Add(Domain.Match matchStatus);
        bool Update(Domain.Match matchStatus);
        Domain.Match Get(string matchId);
        Domain.Match GetByUserId(string userId);
        void Remove(string matchId);
        void RemoveByUserId(string userId);
        IList<Domain.Match> GetAll();
    }
}