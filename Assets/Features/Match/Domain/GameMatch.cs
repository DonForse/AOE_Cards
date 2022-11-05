using Game;
using Infrastructure.Data;

namespace Features.Match.Domain
{
    public class GameMatch
    {
        public string Id;
        public Hand Hand;
        public Board Board;
        public string[] Users;
    }
}