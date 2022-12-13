using System.Collections.Concurrent;
using UnityEngine;

namespace Features.ServerLogic.Matches.Infrastructure
{
    public class InMemoryUserMatchesRepository : IUserMatchesRepository
    {
        private readonly ConcurrentDictionary<string, string> _userMatches = new();
        public void Add(string userId, string matchId)
        {
            if (!_userMatches.ContainsKey(userId))
                _userMatches[userId] = matchId;
        }

        public string GetMatchId(string userId)
        {
            return _userMatches.ContainsKey(userId) ? _userMatches[userId] : null;
        }

        public void Remove(string userId)
        {
            _userMatches.TryRemove(userId, out var matchId);
        }
    }
}