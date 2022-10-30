using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ServerLogic.Matches.Infrastructure
{
    public class InMemoryMatchesRepository : IMatchesRepository
    {
        public ConcurrentDictionary<string, Domain.Match> matches = new ConcurrentDictionary<string, Domain.Match>();
        public ConcurrentDictionary<string, string> userMatches = new ConcurrentDictionary<string, string>();
        public bool Add(Domain.Match match)
        {
            matches.AddOrUpdate(match.Guid, match, (key, oldValue) => match);
            foreach (var user in match.Users)
            {
                userMatches.AddOrUpdate(user.Id, match.Guid, (key, oldValue) => match.Guid);
            }
            return true;
        }
        public bool Update(Domain.Match match)
        {
            if (matches.ContainsKey(match.Guid))
            {
                matches[match.Guid] = match;
                return true;
            }
            return false;
        }

        public Domain.Match Get(string matchId)
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

        public Domain.Match GetByUserId(string userId)
        {
            if (!userMatches.ContainsKey(userId))
                return null;

            var matchId = userMatches[userId];
            return matches[matchId];
        }

        public IList<Domain.Match> GetAll()
        {
            return matches.Values.ToList();
        }
    }
}