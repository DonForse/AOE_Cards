using System;
using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Matches.Action;
using ServerLogic.Cards.Infrastructure;
using ServerLogic.Matches.Action;
using ServerLogic.Matches.Infrastructure;
using ServerLogic.Users.Actions;
using ServerLogic.Users.Domain;

namespace ServerLogic.Matches.Service
{
    public class MatchCreator
    {
        private readonly IUsersQueuedRepository _usersQueuedRepository;
        private readonly IFriendsUsersQueuedRepository _friendsQueuedRepository;
        private readonly IMatchesRepository _matchRepository;
        private readonly ICardRepository _cardRepository;
        private readonly DequeueUser _dequeueUser;
        private readonly EnqueueUser _enqueueUser;

        private readonly DequeueFriendUser _dequeueFriendUser;
        private readonly EnqueueFriendUser _enqueueFriendUser;

        private readonly CreateMatch _createMatch;

        public MatchCreator(IMatchesRepository matchRepository,
            ICardRepository cardRepository, 
            IUsersQueuedRepository usersQueuedRepository,
            IFriendsUsersQueuedRepository friendsUsersQueuedRepository)
        {
            _matchRepository = matchRepository;
            _cardRepository = cardRepository;
            _usersQueuedRepository = usersQueuedRepository;
            _friendsQueuedRepository = friendsUsersQueuedRepository;
            _dequeueUser = new DequeueUser(_usersQueuedRepository);
            _enqueueUser = new EnqueueUser(_usersQueuedRepository);
            _dequeueFriendUser = new DequeueFriendUser(_friendsQueuedRepository);
            _enqueueFriendUser = new EnqueueFriendUser(_friendsQueuedRepository);
            _createMatch = new CreateMatch(_matchRepository, _cardRepository);
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
            var usersEnqueued = new List<Tuple<User, string>>();

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
                var user = _dequeueUser.Execute(users[0], true);
                if (usersEnqueued.Count > 0)
                {
                    var matchUsers = new List<User> { usersEnqueued[0].Item1, user.Item1 };
                    _createMatch.Execute(matchUsers, false);
                    users.Remove(user.Item1);
                    usersEnqueued.Clear();
                    continue;

                }
                usersEnqueued.Add(user);
                users.Remove(user.Item1);
            }
            while (usersEnqueued.Count > 0)
            {
                _enqueueUser.Execute(usersEnqueued[0].Item1, usersEnqueued[0].Item2);
                usersEnqueued.Remove(usersEnqueued[0]);
            }
        }

        private void CreateBotMatch(User user)
        {
            _createMatch.Execute(new List<User> { user }, true);
        }
    }
}