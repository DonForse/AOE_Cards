using System;
using System.Collections.Generic;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Matches.Infrastructure.DTO;
using Features.ServerLogic.Matches.Service;
using Features.ServerLogic.Users.Actions;
using Features.ServerLogic.Users.Domain;
using Newtonsoft.Json;

namespace Features.ServerLogic.Handlers
{
    public class MatchHandler
    {
        private readonly IGetUserMatch _getUserMatch;
        private readonly IMatchCreatorService _matchCreatorService;
        private readonly ICreateMatch _createMatch;
        private readonly IGetUser _getUser;
        private readonly IEnqueueFriendMatch _enqueueFriendMatch;
        private readonly IEnqueueMatch _enqueueMatch;
        private readonly IDequeueFriendMatch _dequeueFriendMatch;
        private readonly IDequeueMatch _dequeueMatch;
        private readonly IRemoveUserMatch _removeUserMatch;
        public MatchHandler(IGetUserMatch getUserMatch,
            IMatchCreatorService matchCreatorService,
            ICreateMatch createMatch,
            IGetUser getUser,
            IEnqueueFriendMatch enqueueFriendMatch,
            IEnqueueMatch enqueueMatch,
            IDequeueFriendMatch dequeueFriendMatch,
            IDequeueMatch dequeueMatch,
            IRemoveUserMatch removeUserMatch)
        {
            _getUserMatch = getUserMatch;
            _matchCreatorService = matchCreatorService;
            _createMatch = createMatch;
            _getUser = getUser;
            _enqueueFriendMatch = enqueueFriendMatch;
            _enqueueMatch = enqueueMatch;
            _dequeueFriendMatch = dequeueFriendMatch;
            _dequeueMatch = dequeueMatch;
            _removeUserMatch = removeUserMatch;
        }
        // GET api/matches/guid-guid-guid-guid
        /// <returns> no match available</returns> (retry after a few secs) -> remember to clear from memory if unused or used
        /// <returns>matchId + matchstatus</returns>
        public ResponseDto Get(string userId)
        {
            _matchCreatorService.CreateMatches();
            
            try
            {
                var user = ValidateUser(userId);
                var match = _getUserMatch.Execute(userId);

                var responseDto = new ResponseDto
                {
                    response =JsonConvert.SerializeObject(new MatchDto(match, user.Id)),
                    error = string.Empty
                };
                return responseDto;

            }
            catch (Exception ex)
            {
                var responseDto = new ResponseDto
                {
                    response = JsonConvert.SerializeObject(new MatchDto(null, userId)),
                    error = ex.Message
                };
                return responseDto;
            }
        }

        public ResponseDto Post(string userId, MatchInfoDto matchInfoDto)
        {
            try
            {
                var user = ValidateUser(userId);
                if (matchInfoDto.vsBot)
                {
                    //TODO: Enqueue User to Bot queue or something like that or extrapolate create vs bot.
                    _createMatch.Execute(new List<User> { user }, true, matchInfoDto.botDifficulty); 
                    var response = new ResponseDto
                    {
                        response = JsonConvert.SerializeObject(new MatchDto(null, user.Id)),
                        error = string.Empty
                    };
                    return response;
                }

                if (matchInfoDto.vsFriend)
                {
                    _enqueueFriendMatch.Execute(user, matchInfoDto.friendCode);

                    return new ResponseDto
                    {
                        response = JsonConvert.SerializeObject(new MatchDto(null, user.Id)),
                        error = string.Empty
                    };
                }
                
                _enqueueMatch.Execute(user, DateTime.Now);

                return new ResponseDto
                {
                    response = JsonConvert.SerializeObject(new MatchDto(null, user.Id)),
                    error = string.Empty
                };
            }
            catch (Exception ex)
            { 
                var responseDto = new ResponseDto
                {
                    response = JsonConvert.SerializeObject(new MatchDto(null, userId)),
                    error = ex.Message
                };
                return responseDto;
            }
        }
        
        public ResponseDto Delete(string userId)
        {
            try
            {
                var user = ValidateUser(userId);
                _dequeueFriendMatch.Execute(user);

                _dequeueMatch.Execute(user);
                //Clear Match Data from user
                _removeUserMatch.Execute(userId);

                return new ResponseDto
                {
                    response = "ok",
                    error = string.Empty
                };
            }
            catch (Exception ex)
            {
                var responseDto = new ResponseDto
                {
                    response = "",
                    error = ex.Message
                };
                return responseDto;
            }
        }

        private User ValidateUser(string userId)
        {
            var user = _getUser.Execute(userId);
            if (user == null)
                throw new ApplicationException("user is not valid");
            return user;
        }
    }
}
