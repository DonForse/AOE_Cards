using System;

namespace Infrastructure.DTOs
{
    [Serializable]
    public class MatchPostDto
    {
        public int botDifficulty;
        public bool vsBot;
        public bool vsFriend;
        public string friendCode;
    }
}