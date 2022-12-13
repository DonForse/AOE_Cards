using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Features.ServerLogic.Matches.Infrastructure
{
    public class InMemoryMatchesRepository : IMatchesRepository
    {
        private readonly ConcurrentDictionary<string, Domain.ServerMatch> _matches = new();
        // private readonly ConcurrentDictionary<string, string> _userMatches = new();
        public bool Add(Domain.ServerMatch serverMatch)
        {
            _matches.AddOrUpdate(serverMatch.Guid, serverMatch, (key, oldValue) => serverMatch);
            return true;
        }
        public bool Update(Domain.ServerMatch serverMatch)
        {
            if (!_matches.ContainsKey(serverMatch.Guid)) return false;
            
            _matches[serverMatch.Guid] = serverMatch;
            return true;
        }

        public Domain.ServerMatch Get(string matchId)
        {
            return _matches.ContainsKey(matchId) ? _matches[matchId] : null;
        }

        public void Remove(string matchId)
        {
            if (_matches.ContainsKey(matchId))
                _matches.TryRemove(matchId, out var match);
        }

        public IEnumerable<Domain.ServerMatch> GetAll()
        {
            return _matches.Values.ToList();
        }
    }
}