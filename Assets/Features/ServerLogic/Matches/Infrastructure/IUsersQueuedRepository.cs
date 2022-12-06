using System;
using System.Collections.Generic;
using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Matches.Infrastructure
{
    public interface IUsersQueuedRepository
    {
        bool Add(User user, DateTime date);
        Tuple<User, DateTime> Get(string userId);
        bool Remove(string userId);
        IList<Tuple<User, DateTime>> GetAll();
    }
}