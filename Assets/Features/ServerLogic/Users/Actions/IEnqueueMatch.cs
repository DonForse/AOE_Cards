using System;
using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Users.Actions
{
    public interface IEnqueueMatch
    {
        void Execute(User user, DateTime date);
    }
}