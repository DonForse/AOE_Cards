using Newtonsoft.Json;
using System;
using ServerLogic.Cards.Infrastructure;
using ServerLogic.Matches.Action;
using ServerLogic.Matches.Infrastructure;
using ServerLogic.Matches.Infrastructure.DTO;

namespace AoeCards.Controllers
{
    public class RoundController
    {
        private readonly IMatchesRepository _matchesRepository;
        private readonly ICardRepository _cardRepository;

        public RoundController(IMatchesRepository matchesRepository, ICardRepository cardRepository)
        {
            _matchesRepository = matchesRepository;
            _cardRepository = cardRepository;
        }

        // GET api/play/matchid(guid-guid-guid-guid)
        /// <returns> no match available</returns>
        /// <returns>matchId</returns>
        public ResponseDto Get(string userId, string matchId, int roundNumber)
        {
            try
            {
                var getMatch = new GetMatch(_matchesRepository);
                var match = getMatch.Execute(matchId);
                if (match == null)
                    throw new ApplicationException("Match not found");

                var round = new RoundDto(null, null, userId);
                if (match.Board.RoundsPlayed.Count > roundNumber)
                    round = new RoundDto(match.Board.RoundsPlayed[roundNumber], match.Users, userId);

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
    }
}
