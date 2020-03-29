using System;
using System.Collections.Generic;

namespace Infrastructure.Services
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