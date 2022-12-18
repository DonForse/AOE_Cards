using System;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Infrastructure.DTO;
using Newtonsoft.Json;

namespace Features.ServerLogic.Handlers
{
    
    public class RerollHandler
    {
        private readonly IGetMatch _getMatch;
        private readonly IPlayReroll _playReroll;

        public RerollHandler(IGetMatch getMatch, IPlayReroll playReroll)
        {
            _getMatch = getMatch;
            _playReroll = playReroll;
        }

        // POST api/play/matchid/playerId
        public ResponseDto Post(string userId, string matchId, RerollInfoDto json)
        {
            try
            {
                var cards = json;
                var match = _getMatch.Execute(matchId);
                if (match == null)
                    throw new ApplicationException("Match Not Found!");
                _playReroll.Execute(match, userId, cards);
                var handDto = new HandDto(_getMatch.Execute(matchId).Board.PlayersHands[userId]);

                var responseDto = new ResponseDto
                {
                    response = JsonConvert.SerializeObject(handDto),
                    error = string.Empty
                };
                return responseDto;
            }
            catch (Exception ex)
            {
                var responseDto = new ResponseDto
                {
                    response = JsonConvert.SerializeObject(new HandDto(null)), 
                    error = ex.Message
                };
                return responseDto;
            }
                
        }
    }
}
