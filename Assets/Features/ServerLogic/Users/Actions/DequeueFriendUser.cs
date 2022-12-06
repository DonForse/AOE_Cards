using System;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Users.Actions
{
    internal class DequeueFriendUser
    {
        private IFriendsUsersQueuedRepository _usersFriendQueuedRepository;

        public DequeueFriendUser(IFriendsUsersQueuedRepository usersFriendQueuedRepository)
        {
            _usersFriendQueuedRepository = usersFriendQueuedRepository;
        }

        internal Tuple<User,string> Execute(string key, bool remove)
        {
            var dequeueUser = _usersFriendQueuedRepository.Get(key);
            if (dequeueUser != null)
            {
                if (remove)
                    _usersFriendQueuedRepository.Remove(key);
            }
            return dequeueUser;
        }
    }
}