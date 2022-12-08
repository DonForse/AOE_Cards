using Features.Game.Scripts.Domain;
using Features.Infrastructure.Data;

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