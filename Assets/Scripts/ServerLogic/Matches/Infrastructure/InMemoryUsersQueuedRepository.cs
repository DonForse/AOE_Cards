using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ServerLogic.Users.Domain;

namespace ServerLogic.Matches.Infrastructure
{
    public class InMemoryUsersQueuedRepository : IUsersQueuedRepository
    {
        private readonly ConcurrentDictionary<string, Tuple<User,DateTime>> connectedUsers = new ConcurrentDictionary<string, Tuple<User, DateTime>>();


        public bool Add(User user, DateTime date)
        {
            return connectedUsers.TryAdd(user.Id, new Tuple<User,DateTime>(user,date));
        }

        public Tuple<User,DateTime> Get(string userId)
        {
            if (connectedUsers.ContainsKey(userId))
                return connectedUsers[userId];
            return null;
        }

        public IList<Tuple<User, DateTime>> GetAll()
        {
            return connectedUsers.Values.ToList();
        }

        public bool Remove(string userId)
        {
            return connectedUsers.TryRemove(userId, out var dt);
        }
    }
}