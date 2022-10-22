using System;
using System.Collections.Generic;
using ServerLogic.Users.Domain;

namespace ServerLogic.Matches.Infrastructure
{
    public interface IFriendsUsersQueuedRepository
    {
        bool Add(User user, string friendCode);
        Tuple<User,string> Get(string userId);
        bool Remove(string userId);
        IList<Tuple<User,string>> GetAll();
        IDictionary<string, string> GetKeys();
    }
}