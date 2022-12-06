using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Features.ServerLogic.Matches.Infrastructure
{
    public class InMemoryMatchesRepository : IMatchesRepository
    {
        public ConcurrentDictionary<string, Features.ServerLogic.Matches.Domain.ServerMatch> matches = new ConcurrentDictionary<string, Features.ServerLogic.Matches.Domain.ServerMatch>();
        public ConcurrentDictionary<string, string> userMatches = new ConcurrentDictionary<string, string>();
        public bool Add(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch)
        {
            matches.AddOrUpdate(serverMatch.Guid, serverMatch, (key, oldValue) => serverMatch);
            foreach (var user in serverMatch.Users)
            {
                userMatches.AddOrUpdate(user.Id, serverMatch.Guid, (key, oldValue) => serverMatch.Guid);
            }
            return true;
        }
        public bool Update(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch)
        {
            if (matches.ContainsKey(serverMatch.Guid))
            {
                matches[serverMatch.Guid] = serverMatch;
                return true;
            }
            return false;
        }

        public Features.ServerLogic.Matches.Domain.ServerMatch Get(string matchId)
        {
            return matches.ContainsKey(matchId) ? matches[matchId] : null;

        }

        public void Remove(string matchId)
        {
            if (matches.ContainsKey(matchId))
                matches.TryRemove(matchId, out var match);
        }

        public void RemoveByUserId(string userId)
        {
            if (!userMatches.ContainsKey(userId))
                return;

            string matchId;
            userMatches.TryRemove(userId, out matchId);

            if (!matches.ContainsKey(matchId))
                return;
            var match = matches[matchId];
            if (match.Users.All(user => !userMatches.ContainsKey(user.Id)))
                Remove(matchId);
        }

        public Features.ServerLogic.Matches.Domain.ServerMatch GetByUserId(string userId)
        {
            if (!userMatches.ContainsKey(userId))
                return null;

            var matchId = userMatches[userId];
            return matches[matchId];
        }

        public IList<Features.ServerLogic.Matches.Domain.ServerMatch> GetAll()
        {
            return matches.Values.ToList();
        }
    }
}