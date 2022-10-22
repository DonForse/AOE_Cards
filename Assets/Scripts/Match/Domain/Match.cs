using Game;
using Infrastructure.Data;

namespace Match.Domain
{
    public class Match
    {
        public string Id;
        public Hand Hand;
        public Board Board;
        public string[] Users;
    }
}