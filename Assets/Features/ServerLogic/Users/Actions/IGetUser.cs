using System;
using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Users.Actions
{
    public interface IGetUser
    {
        User Execute(string username, string encodedPassword, DateTime dt);
        User Execute(string userId);
    }
}