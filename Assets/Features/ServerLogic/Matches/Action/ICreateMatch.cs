using System.Collections.Generic;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Matches.Action
{
    public interface ICreateMatch
    {
        ServerMatch Execute(IList<User> users, bool isBot, int botDifficulty = 0);
    }
}