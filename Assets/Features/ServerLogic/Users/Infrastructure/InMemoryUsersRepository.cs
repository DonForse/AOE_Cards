using System;
using System.Collections.Concurrent;
using System.Linq;
using ServerLogic.Users.Domain;

namespace ServerLogic.Users.Infrastructure
{
    public class InMemoryUsersRepository : IUsersRepository
    {
        private readonly ConcurrentDictionary<string, User> _users = new ConcurrentDictionary<string, User>();

        public bool Add(User user)
        {
            return _users.TryAdd(user.UserName.ToLower(), user);
        }
        public User Get(string username) {
            return _users.ContainsKey(username.ToLower()) ? _users[username.ToLower()] : null;
        }

        public User GetById(string userId)
        {
            return _users.Values.FirstOrDefault(c => c.Id == userId);
        }

        public bool Remove(string username) {
            throw new NotImplementedException();
        }
    }
}