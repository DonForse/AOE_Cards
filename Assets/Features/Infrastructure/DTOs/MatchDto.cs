using System;

namespace Features.Infrastructure.DTOs
{
    [Serializable]
    public class MatchDto
    {
        public string matchId;
        public BoardDto board;
        public HandDto hand;
        public string[] users;
    }
}