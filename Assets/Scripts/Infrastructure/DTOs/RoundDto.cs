using System;
using System.Collections.Generic;

namespace Infrastructure.Services
{
    [Serializable]
    public class RoundDto
    {
        public IList<PlayerCardDto> cardsplayed;
        public string winnerplayer;
        public string upgradecardround;
    }
}