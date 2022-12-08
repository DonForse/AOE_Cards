using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Features.ServerLogic.Matches.Infrastructure
{
    public class InMemoryMatchesRepository : IMatchesRepository
    {
        private readonly ConcurrentDictionary<string, Domain.ServerMatch> _matches = new();
        private readonly ConcurrentDictionary<string, string> _userMatches = new();
        public bool Add(Domain.ServerMatch serverMatch)
        {
            _matches.AddOrUpdate(serverMatch.Guid, serverMatch, (key, oldValue) => serverMatch);
            foreach (var user in serverMatch.Users)
            {
                _userMatches.AddOrUpdate(user.Id, serverMatch.Guid, (key, oldValue) => serverMatch.Guid);
            }
            return true;
        }
        public bool Update(Domain.ServerMatch serverMatch)
        {
            if (_matches.ContainsKey(serverMatch.Guid))
            {
                _matches[serverMatch.Guid] = serverMatch;
                return true;
            }
            return false;
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

        public void RemoveByUserId(string userId)
        {
            if (!_userMatches.ContainsKey(userId))
                return;

            string matchId;
            _userMatches.TryRemove(userId, out matchId);

            if (!_matches.ContainsKey(matchId))
                return;
            var match = _matches[matchId];
            if (match.Users.All(user => !_userMatches.ContainsKey(user.Id)))
                Remove(matchId);
        }

        public Features.ServerLogic.Matches.Domain.ServerMatch GetByUserId(string userId)
        {
            if (!_userMatches.ContainsKey(userId))
                return null;

            var matchId = _userMatches[userId];
            return _matches[matchId];
        }

        public IList<Features.ServerLogic.Matches.Domain.ServerMatch> GetAll()
        {
            return _matches.Values.ToList();
        }
    }
}