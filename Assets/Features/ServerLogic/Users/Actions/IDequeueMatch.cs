using System;
using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Users.Actions
{
    public interface IDequeueMatch
    {
        Tuple<User,DateTime> Execute(User user);
    }
}