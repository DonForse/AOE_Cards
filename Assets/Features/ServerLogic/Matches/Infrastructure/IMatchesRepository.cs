using System.Collections.Generic;

namespace Features.ServerLogic.Matches.Infrastructure
{
    public interface IMatchesRepository
    {
        bool Add(Features.ServerLogic.Matches.Domain.ServerMatch serverMatchStatus);
        bool Update(Features.ServerLogic.Matches.Domain.ServerMatch serverMatchStatus);
        Features.ServerLogic.Matches.Domain.ServerMatch Get(string matchId);
        Features.ServerLogic.Matches.Domain.ServerMatch GetByUserId(string userId);
        void Remove(string matchId);
        void RemoveByUserId(string userId);
        IList<Features.ServerLogic.Matches.Domain.ServerMatch> GetAll();
    }
}