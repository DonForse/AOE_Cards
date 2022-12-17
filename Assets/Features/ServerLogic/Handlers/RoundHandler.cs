﻿using System;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Infrastructure.DTO;
using Newtonsoft.Json;

namespace Features.ServerLogic.Handlers
{
    public class RoundHandler
    {
        private readonly IGetMatch _getMatch;

        public RoundHandler(IGetMatch getMatch)
        {
            _getMatch = getMatch;
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
                    throw new ApplicationException("Round not found");

                var responseDto = new ResponseDto
                {
                    response = JsonConvert.SerializeObject(new RoundDto(match.Board.RoundsPlayed[roundNumber], match.Users, userId)),
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
