using System.Collections.Generic;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Editor.Tests.Mothers
{
    public static class ServerMatchMother
    {
        public static ServerMatch Create(string withId = null,
            IEnumerable<User> withUsers = null,
            Board withBoard = null,
            bool withIsBot = false,
            int withBotDifficulty = 0,
            bool withIsFinished = false, bool withIsTie = false,
            User withMatchWinner = null)
        {
            withUsers ??= new List<User>();
            withBoard ??= new Board();
            return new ServerMatch()
            {
                Guid = withId,
                Users = withUsers,
                Board = withBoard,
                BotDifficulty = withBotDifficulty,
                IsBot = withIsBot,
                IsFinished = withIsFinished,
                IsTie = withIsTie,
                MatchWinner = withMatchWinner,
            };
        }
    }
}