using System.Collections.Generic;
using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Matches.Domain
{
    public class ServerMatch
    {
        public string Guid;
        public Board Board;
        public IEnumerable<User> Users;
        public User MatchWinner;
        public bool IsTie;
        public bool IsFinished;
        public bool IsBot;
        public int BotDifficulty;
    }
}

