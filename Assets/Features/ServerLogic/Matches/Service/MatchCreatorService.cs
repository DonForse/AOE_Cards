using System;
using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Infrastructure;
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
        private readonly IMatchesRepository _matchRepository;
        private readonly ICardRepository _cardRepository;
        private readonly DequeueMatch _dequeueMatch;
        private readonly EnqueueMatch _enqueueMatch;

        private readonly DequeueFriendMatch _dequeueFriendUser;
        private readonly EnqueueFriendMatch _enqueueFriendMatch;

        private readonly ICreateMatch _createMatch;
        private readonly IGetUserMatch _getUserMatch;
        private readonly ICreateRound _createRound;

        public MatchCreatorService(IMatchesRepository matchRepository,
            ICardRepository cardRepository, 
            IUsersQueuedRepository usersQueuedRepository,
            IFriendsUsersQueuedRepository friendsUsersQueuedRepository,
            IServerConfiguration serverConfiguration,
            ICreateRound createRound,
            IGetUserMatch getUserMatch)
        {
            _matchRepository = matchRepository;
            _cardRepository = cardRepository;
            _usersQueuedRepository = usersQueuedRepository;
            _friendsQueuedRepository = friendsUsersQueuedRepository;
            _dequeueMatch = new DequeueMatch(_usersQueuedRepository);
            _enqueueMatch = new EnqueueMatch(_usersQueuedRepository);
            _dequeueFriendUser = new DequeueFriendMatch(_friendsQueuedRepository);
            _enqueueFriendMatch = new EnqueueFriendMatch(_friendsQueuedRepository);
            _createMatch = ServerLogicProvider.CreateMatch();
        }

        public void CreateMatches()
        {
            CreateQueuedUsersMatch();
            CreateFriendsQueuedUsersMatch();
        }

        private void CreateFriendsQueuedUsersMatch()
        {
            var usersKeys = _friendsQueuedRepository.GetKeys();
            var users = usersKeys.ToList();

            while (users.Count > 0)
            {
                if (usersKeys.ContainsKey(users[0].Value) && usersKeys[users[0].Value] == users[0].Key)
                {
                    var user1 = _dequeueFriendUser.Execute(users[0].Key, true);
                    var user2 = _dequeueFriendUser.Execute(users[0].Value, true);
                    var matchUsers = new List<User> { user1.Item1, user2.Item1 };
                    _createMatch.Execute(matchUsers, false);
                    users.Remove(users.FirstOrDefault(u => u.Key == users[0].Value));
                    users.Remove(users[0]);

                    var match = _getUserMatch.Execute(users[0].Key);
                    _createRound.Execute(match.Guid);
                    continue;
                }
                users.Remove(users[0]);
            }
        }

        private void CreateQueuedUsersMatch()
        {
            var users = _usersQueuedRepository.GetAll().OrderBy(item => item.Item2).Select(item => item.Item1).ToList();
            var usersEnqueued = new List<Tuple<User, DateTime>>();
            while (users.Count > 0)
            {
                var user = _dequeueMatch.Execute(users[0]);
                if (usersEnqueued.Count > 0)
                {
                    var matchUsers = new List<User> { usersEnqueued[0].Item1, user.Item1 };
                    _createMatch.Execute(matchUsers, false);
                    users.Remove(user.Item1);
                    usersEnqueued.Clear();

                    var match = _getUserMatch.Execute(user.Item1.Id);
                    _createRound.Execute(match.Guid);
                    continue;
                }
                usersEnqueued.Add(user);
                users.Remove(user.Item1);
            }
            while (usersEnqueued.Count > 0)
            {
                _enqueueMatch.Execute(usersEnqueued[0].Item1, usersEnqueued[0].Item2);
                usersEnqueued.Remove(usersEnqueued[0]);
            }
        }

        private void CreateBotMatch(User user)
        {
            _createMatch.Execute(new List<User> { user }, true);
            var match = _getUserMatch.Execute(user.Id);
            _createRound.Execute(match.Guid);
        }
    }
}