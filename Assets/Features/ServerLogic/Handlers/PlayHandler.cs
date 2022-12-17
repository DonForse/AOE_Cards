using System;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Infrastructure.DTO;
using Features.ServerLogic.Users.Actions;
using Newtonsoft.Json;

namespace Features.ServerLogic.Handlers
{
    
    public class PlayHandler
    {
        private readonly IRemoveUserMatch _removeUserMatch;
        private readonly IGetMatch _getMatch;
        private readonly IPlayUpgradeCard _playUpgradeCard;
        private readonly IPlayUnitCard _playUnitCard;

        public PlayHandler(IRemoveUserMatch removeUserMatch, IGetMatch getMatch, IPlayUnitCard playUnitCard, IPlayUpgradeCard playUpgradeCard)
        {
            _removeUserMatch = removeUserMatch;
            _getMatch = getMatch;
            _playUnitCard = playUnitCard;
            _playUpgradeCard = playUpgradeCard;
        }

        // GET api/play/matchid(guid-guid-guid-guid)
        /// <returns> no match available</returns>
        /// <returns>matchId</returns>
        public ResponseDto Get(string userId, string matchId, int roundNumber)
        {
            try
            {
                var match = _getMatch.Execute(matchId);
                if (match == null)
                    throw new ApplicationException("Match not found");
                if (match.Board.RoundsPlayed.Count < roundNumber)
                    throw new ApplicationException("Round does not exists");
                    
                if (match.IsFinished)
                    _removeUserMatch.Execute(userId);
                var round = new RoundDto(match.Board.RoundsPlayed[roundNumber], match.Users, userId);

                var responseDto = new ResponseDto
                {
                    response = JsonConvert.SerializeObject(round),
                    error = string.Empty
                };
                return responseDto;
            }
            catch (Exception ex)
            {
                var responseDto = new ResponseDto
                {
                    response = JsonConvert.SerializeObject(new RoundDto(null, null, userId)),
                    error = ex.Message
                };
                return responseDto;
            }
        }

        // POST api/play/matchid/playerId
        public ResponseDto Post(string userId, string matchId, CardInfoDto card)
        {
            try
            {
                var postCardData = card;
                if (postCardData.type == "upgrade")
                {
                    _playUpgradeCard.Execute(matchId, userId, postCardData.cardname);
                }
                else
                {
                    _playUnitCard.Execute(matchId, userId, postCardData.cardname);
                }
                var handDto = new HandDto(_getMatch.Execute(matchId).Board.PlayersHands[userId]);

                var responseDto = new ResponseDto
                {
                    response = JsonConvert.SerializeObject(handDto),
                    error = string.Empty
                };
                return responseDto;
            }
            catch (Exception ex){
                var responseDto = new ResponseDto
                {
                    response = JsonConvert.SerializeObject(new HandDto(_getMatch.Execute(matchId).Board.PlayersHands[userId])),
                    error = ex.Message
                };
                return responseDto;
            }
        }
    }
}
