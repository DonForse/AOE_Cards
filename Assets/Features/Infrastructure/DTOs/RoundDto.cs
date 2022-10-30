using System;
using Infrastructure.Data;

namespace Infrastructure.DTOs
{
    [Serializable]
    public class RoundDto
    {
        public bool finished;
        public int roundnumber;
        public PlayerCardDto[] cardsplayed;
        public string[] winnerplayer;
        public string upgradecardround;
        public bool rivalready;
        public RoundState roundState;
        public bool hasReroll;
        public int roundTimer;
    }
}