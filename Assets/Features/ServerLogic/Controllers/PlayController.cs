using System;
using Features.ServerLogic.Cards.Infrastructure;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Matches.Infrastructure.DTO;
using Newtonsoft.Json;

namespace Features.ServerLogic.Controllers
{
    
    public class PlayController
    {
        private readonly IMatchesRepository _matchesRepository;
        private readonly ICardRepository _cardRepository;

        public PlayController(IMatchesRepository matchesRepository, ICardRepository cardRepository)
        {
            _matchesRepository = matchesRepository;
            _cardRepository = cardRepository;
        }

        // GET api/play/matchid(guid-guid-guid-guid)
        /// <returns> no match available</returns>
        /// <returns>matchId</returns>
        public ResponseDto Get(string userId, string matchId, int roundNumber)
        {
            var getMatch = new GetMatch(_matchesRepository);
            var match = getMatch.Execute(matchId);
            if (match == null)
                throw new ApplicationException("Match not found");

            try
            {
                if (match.IsFinished)
                    _matchesRepository.RemoveByUserId(userId);
                var round = new RoundDto(match.Board.RoundsPlayed[roundNumber], match.Users, userId);

                var responseDto = new ResponseDto
                {
                    response = JsonConvert.SerializeObject(round),
                    error = string.Empty
                };
                return responseDto;
            }
            catch (Exception ex){
                var responseDto = new ResponseDto
                {
                    response = JsonConvert.SerializeObject(new RoundDto(match.Board.RoundsPlayed[roundNumber], match.Users, userId)),
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
                    var playUpgrade = new PlayUpgradeCard(_matchesRepository, _cardRepository);
                    playUpgrade.Execute(matchId, userId, postCardData.cardname);
                }
                else
                {
                    var playUnit = new PlayUnitCard(_matchesRepository, _cardRepository);
                    playUnit.Execute(matchId, userId, postCardData.cardname);
                }
                var handDto = new HandDto(_matchesRepository.Get(matchId).Board.PlayersHands[userId]);

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
                    response = JsonConvert.SerializeObject(new HandDto(_matchesRepository.Get(matchId).Board.PlayersHands[userId])),
                    error = ex.Message
                };
                return responseDto;
            }
            
        }

    }
}
