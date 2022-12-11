using System;
using System.Collections.Generic;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Matches.Infrastructure.DTO;
using Features.ServerLogic.Matches.Service;
using Features.ServerLogic.Users.Actions;
using Features.ServerLogic.Users.Domain;
using Features.ServerLogic.Users.Infrastructure;
using Newtonsoft.Json;

namespace Features.ServerLogic.Handlers
{
    public class MatchHandler
    {
        private readonly IUsersQueuedRepository _usersQueuedRepository;
        private readonly IFriendsUsersQueuedRepository _friendsQueueRepository;
        private readonly IMatchesRepository _matchesRepository;
        private readonly IUsersRepository _usersRepository;
        private readonly IMatchCreatorService _matchCreatorService;
        private readonly ICreateMatch _createMatch;
        private readonly IGetUser _getUser;

        public MatchHandler(IUsersQueuedRepository usersQueuedRepository, 
            IFriendsUsersQueuedRepository friendsUsersQueuedRepository,
            IMatchesRepository matchesRepository,
            IUsersRepository usersRepository,
            IMatchCreatorService matchCreatorService,
            ICreateMatch createMatch,
            IGetUser getUser)
        {
            _usersQueuedRepository = usersQueuedRepository;
            _friendsQueueRepository = friendsUsersQueuedRepository;
            _matchesRepository = matchesRepository;
            _usersRepository = usersRepository;
            _matchCreatorService = matchCreatorService;
            _createMatch = createMatch;
            _getUser = getUser;
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
                var match = _matchesRepository.GetByUserId(userId);

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

                var matchInfo = matchInfoDto;

                if (matchInfo.vsBot)
                {
                    _createMatch.Execute(new List<User> { user }, true, matchInfo.botDifficulty);
                    var response = new ResponseDto
                    {
                        response = JsonConvert.SerializeObject(new MatchDto(null, user.Id)),
                        error = string.Empty
                    };
                    return response;
                }

                if (matchInfo.vsFriend)
                {
                    var enqueueUserFriend = new EnqueueFriendUser(_friendsQueueRepository);
                    enqueueUserFriend.Execute(user, matchInfo.friendCode);

                    return new ResponseDto
                    {
                        response = JsonConvert.SerializeObject(new MatchDto(null, user.Id)),
                        error = string.Empty
                    };
                }

                var enqueueUser = new EnqueueUser(_usersQueuedRepository);
                enqueueUser.Execute(user, DateTime.Now);

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
                _friendsQueueRepository.Remove(user.FriendCode);

                _usersQueuedRepository.Remove(userId);
                _matchesRepository.RemoveByUserId(userId);

                var responseDto = new ResponseDto
                {
                    response = "ok",
                    error = string.Empty
                };
                return responseDto;

            }
            catch (Exception ex)
            {
                var responseDto = new ResponseDto
                {
                    response = "error",
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
