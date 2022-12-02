using System;
using System.Collections.Generic;
using System.Text;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Service;
using Features.ServerLogic.Users.Actions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerLogic.Cards.Infrastructure;
using ServerLogic.Matches.Action;
using ServerLogic.Matches.Infrastructure;
using ServerLogic.Matches.Infrastructure.DTO;
using ServerLogic.Users.Actions;
using ServerLogic.Users.Domain;
using ServerLogic.Users.Infrastructure;
using UnityEditor.PackageManager.Requests;

namespace ServerLogic.Controllers
{
    public class MatchController
    {
        private readonly IUsersQueuedRepository _usersQueuedRepository;
        private readonly IFriendsUsersQueuedRepository _friendsQueueRepository;
        private readonly IMatchesRepository _matchesRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IUsersRepository _usersRepository;
        private readonly IServerConfiguration _serverConfiguration;
        public MatchController(IUsersQueuedRepository usersQueuedRepository, 
            IFriendsUsersQueuedRepository friendsUsersQueuedRepository,
            IMatchesRepository matchesRepository,
            ICardRepository cardRepository,
            IUsersRepository usersRepository, 
            IServerConfiguration serverConfiguration)
        {
            _usersQueuedRepository = usersQueuedRepository;
            _friendsQueueRepository = friendsUsersQueuedRepository;
            _matchesRepository = matchesRepository;
            _cardRepository = cardRepository;
            _usersRepository = usersRepository;
            _serverConfiguration = serverConfiguration;
        }
        // GET api/matches/guid-guid-guid-guid
        /// <returns> no match available</returns> (retry after a few secs) -> remember to clear from memory if unused or used
        /// <returns>matchId + matchstatus</returns>
        public ResponseDto Get(string userId)
        {
            var matchCreator = new MatchCreator(_matchesRepository, _cardRepository, _usersQueuedRepository, _friendsQueueRepository, _serverConfiguration);
            matchCreator.CreateMatches();
            
            try
            {
                var getUser = new GetUser(_usersRepository);
                var user = getUser.Execute(userId);
                if (user == null)
                    throw new ApplicationException("user is not valid");

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
                var getUser = new GetUser(_usersRepository);
                var user = getUser.Execute(userId);
                if (user == null)
                    throw new ApplicationException("user is not valid");
                var matchInfo = matchInfoDto;

                if (matchInfo.vsBot)
                    return PlayBot(user, matchInfo.botDifficulty);

                if (matchInfo.vsFriend)
                    return EnqueueFriend(user, matchInfo);

                return EnqueueRandom(user);
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

        private ResponseDto PlayBot(User user, int botDifficulty)
        {
            var createMatch = new CreateMatch(_matchesRepository, _cardRepository, _serverConfiguration, new CreateBotUser());
            createMatch.Execute(new List<User> { user }, true, botDifficulty);
            var response = new ResponseDto
            {
                response = JsonConvert.SerializeObject(new MatchDto(null, user.Id)),
                error = string.Empty
            };
            return response;
        }

        private ResponseDto EnqueueRandom(User user)
        {
            var enqueueUser = new EnqueueUser(_usersQueuedRepository);
            enqueueUser.Execute(user, DateTime.Now);

            var responseDto = new ResponseDto
            {
                response = JsonConvert.SerializeObject(new MatchDto(null, user.Id)),
                error = string.Empty
            };
            return responseDto;
        }

        private ResponseDto EnqueueFriend(User user, MatchInfoDto matchInfo)
        {
            var enqueueUserFriend = new EnqueueFriendUser(_friendsQueueRepository);
            enqueueUserFriend.Execute(user, matchInfo.friendCode);

            var responseDto = new ResponseDto
            {
                response = JsonConvert.SerializeObject(new MatchDto(null, user.Id)),
                error = string.Empty
            };
            return responseDto;
        }

        public ResponseDto Delete(string userId)
        {
            try
            {
                var getUser = new GetUser(_usersRepository);
                var user = getUser.Execute(userId);
                if (user == null)
                    throw new ApplicationException("user is not valid");
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
    }
}
