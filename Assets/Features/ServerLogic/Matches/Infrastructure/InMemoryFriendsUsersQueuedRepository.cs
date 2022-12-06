using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Matches.Infrastructure
{
    public class InMemoryFriendsUsersQueuedRepository : IFriendsUsersQueuedRepository
    {
        private readonly ConcurrentDictionary<string, Tuple<User,string>> connectedUsers = new ConcurrentDictionary<string, Tuple<User, string>>();


        public bool Add(User user,string friendCode)
        {
            return connectedUsers.TryAdd(user.FriendCode.ToLower(), new Tuple<User,string>(user, friendCode));
        }

        public Tuple<User,string> Get(string friendCode)
        {
            friendCode = friendCode.ToLower();
            if (connectedUsers.ContainsKey(friendCode))
                return connectedUsers[friendCode];
            return null;
        }

        public IList<Tuple<User,string>> GetAll()
        {
            return connectedUsers.Values.ToList();
        }

        public IDictionary<string, string> GetKeys()
        {
            return connectedUsers.ToDictionary(k => k.Value.Item1.FriendCode.ToLower(), v => v.Value.Item2);
        }

        public bool Remove(string friendCode)
        {
            return connectedUsers.TryRemove(friendCode.ToLower(), out var dt);
        }
    }
}