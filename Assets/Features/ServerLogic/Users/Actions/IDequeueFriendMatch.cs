using System;
using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Users.Actions
{
    public interface IDequeueFriendMatch
    {
        Tuple<User,string> Execute(User user);
    }
}