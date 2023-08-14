using System.Collections.Generic;

namespace Features.ServerLogic.Matches.Infrastructure.DTO
{
    public class BoardDto
    {
        public IList<RoundDto> rounds;
        public RoundDto currentRound;
    }
}

