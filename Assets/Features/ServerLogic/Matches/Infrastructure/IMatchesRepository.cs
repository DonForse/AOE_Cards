using System.Collections.Generic;

namespace Features.ServerLogic.Matches.Infrastructure
{
    public interface IMatchesRepository
    {
        bool Add(Domain.ServerMatch serverMatchStatus);
        bool Update(Domain.ServerMatch serverMatchStatus);
        Domain.ServerMatch Get(string matchId);
        void Remove(string matchId);
        IEnumerable<Domain.ServerMatch> GetAll();
    }
}