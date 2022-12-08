using System;
using Features.ServerLogic.Cards.Infrastructure;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Matches.Infrastructure.DTO;
using Newtonsoft.Json;

namespace Features.ServerLogic.Handlers
{
    
    public class RerollHandler
    {
        private readonly IMatchesRepository _matchesRepository;
        private readonly ICardRepository _cardRepository;

        public RerollHandler(IMatchesRepository matchesRepository, ICardRepository cardRepository)
        {
            _matchesRepository = matchesRepository;
            _cardRepository = cardRepository;
        }

        // POST api/play/matchid/playerId
        public ResponseDto Post(string userId, string matchId, RerollInfoDto json)
        {
            try
            {
                var cards = json;
                var getMatch = new GetMatch(_matchesRepository);
                var match = getMatch.Execute(matchId);
                if (match == null)
                    throw new ApplicationException("Match Not Found!");
                var rerollHand = new PlayReroll(_cardRepository);
                rerollHand.Execute(match, userId, cards);
                var handDto = new HandDto(_matchesRepository.Get(matchId).Board.PlayersHands[userId]);

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
                    response = JsonConvert.SerializeObject(new HandDto(_matchesRepository.Get(matchId).Board.PlayersHands[userId])), 
                    error = ex.Message
                };
                return responseDto;
            }
                
        }
    }
}
