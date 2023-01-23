using System;
using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Users.Actions;
using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Matches.Service
{
    public class MatchCreatorService : IMatchCreatorService
    {
        private readonly IUsersQueuedRepository _usersQueuedRepository;
        private readonly IFriendsUsersQueuedRepository _friendsQueuedRepository;
        private readonly IDequeueMatch _dequeueMatch;
        private readonly IEnqueueMatch _enqueueMatch;
        private readonly IDequeueFriendMatch _dequeueFriendMatch;
        private readonly IGetUser _getUser;
        private readonly IServerConfiguration _serverConfiguration;

        private readonly ICreateMatch _createMatch;
        private readonly ICreateNewRound _createNewRound;

        public MatchCreatorService(
            IUsersQueuedRepository usersQueuedRepository,
            IFriendsUsersQueuedRepository friendsUsersQueuedRepository,
            IDequeueMatch dequeueMatch,
            IEnqueueMatch enqueueMatch,
            IDequeueFriendMatch dequeueFriendMatch,
            IGetUser getUser,
            ICreateMatch createMatch,
            ICreateNewRound createNewRound,
            IServerConfiguration serverConfiguration)
        {
            _usersQueuedRepository = usersQueuedRepository;
            _friendsQueuedRepository = friendsUsersQueuedRepository;
            _dequeueMatch = dequeueMatch;
            _enqueueMatch = enqueueMatch;
            _dequeueFriendMatch = dequeueFriendMatch;
            _getUser = getUser;
            _createMatch = createMatch;
            _createNewRound = createNewRound;
            _serverConfiguration = serverConfiguration;
        }

        public void CreateMatches()
        {
            CreateQueuedUsersMatch();
            CreateFriendsQueuedUsersMatch();
        }

        private void CreateFriendsQueuedUsersMatch()
        {
            var usersKeys = _friendsQueuedRepository.GetKeys();
            //todo: create action
            var users = usersKeys.ToList();

            while (users.Count > 0)
            {
                if (usersKeys.ContainsKey(users[0].Value) && usersKeys[users[0].Value] == users[0].Key)
                {
                    var userFirst = _getUser.Execute(users[0].Key);
                    var userSecond = _getUser.Execute(users[0].Value);
                    
                    users.Remove(users.FirstOrDefault(u => u.Key == userSecond.Id));
                    users.Remove(users[0]);
                    
                    _dequeueFriendMatch.Execute(userFirst);
                    _dequeueFriendMatch.Execute(userSecond);
                    
                    var matchUsers = new List<User> { userFirst, userSecond };
                    CreateMatchBetweenPlayers(matchUsers);

                    continue;
                }

                users.Remove(users[0]);
            }
        }
        

        private void CreateQueuedUsersMatch()
        {
            var users = _usersQueuedRepository.GetAll().OrderBy(item => item.Item2).Select(item => item.Item1).ToList();
            var usersInMatchToBeCreated = new List<Tuple<User, DateTime>>();
            while (users.Count > 0)
            {
                var user = _dequeueMatch.Execute(users[0]);
                if (CanAddPlayersToMatch())
                {
                    AddPlayerToFutureMatch(user);
                    users.Remove(user.Item1);
                    continue;
                }

                var matchUsers = new List<User> { usersInMatchToBeCreated[0].Item1, user.Item1 };
                CreateMatchBetweenPlayers(matchUsers);

                users.Remove(user.Item1);
                usersInMatchToBeCreated.Clear();
            }

            while (AreUsersLeftInMatchButNotEnoughToPair())
            {
                _enqueueMatch.Execute(usersInMatchToBeCreated[0].Item1, usersInMatchToBeCreated[0].Item2);
                usersInMatchToBeCreated.Remove(usersInMatchToBeCreated[0]);
            }

            bool CanAddPlayersToMatch() =>
                usersInMatchToBeCreated.Count +1 < _serverConfiguration.GetAmountOfPlayersInMatch();

            void AddPlayerToFutureMatch(Tuple<User, DateTime> user) => usersInMatchToBeCreated.Add(user);

            bool AreUsersLeftInMatchButNotEnoughToPair() => usersInMatchToBeCreated.Count > 0;
        }
        private void CreateMatchBetweenPlayers(List<User> matchUsers)
        {
            var match = _createMatch.Execute(matchUsers, false);
            _createNewRound.Execute(match.Guid);
        }
    }
}