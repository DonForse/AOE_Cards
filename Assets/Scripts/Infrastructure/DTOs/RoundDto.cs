using System;
using System.Collections.Generic;

namespace Infrastructure.Services
{
    [Serializable]
    public class RoundDto
    {
        public int roundnumber;
        public PlayerCardDto[] cardsplayed;
        public string[] winnerplayer;
        public string upgradecardround;
    }
}