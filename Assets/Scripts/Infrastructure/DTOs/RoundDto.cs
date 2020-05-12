using System;
using System.Collections.Generic;

namespace Infrastructure.Services
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