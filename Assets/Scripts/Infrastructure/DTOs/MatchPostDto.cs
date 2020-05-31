using System;

namespace Infrastructure.Services
{
    [Serializable]
    public class MatchPostDto
    {
        public bool vsBot;
        public bool vsFriend;
        public string friendCode;
    }
}