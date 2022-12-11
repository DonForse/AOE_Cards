using System;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Users.Actions
{
    internal class DequeueFriendMatch : IDequeueFriendMatch
    {
        private IFriendsUsersQueuedRepository _usersFriendQueuedRepository;

        public DequeueFriendMatch(IFriendsUsersQueuedRepository usersFriendQueuedRepository)
        {
            _usersFriendQueuedRepository = usersFriendQueuedRepository;
        }

        public Tuple<User,string> Execute(string friendCode, bool asd)
        {
            var dequeueUser = _usersFriendQueuedRepository.Get(friendCode.ToLowerInvariant());
            if (dequeueUser != null) 
                _usersFriendQueuedRepository.Remove(friendCode.ToLowerInvariant());
            return dequeueUser;
        }
        
        public Tuple<User,string> Execute(User user)
        {
            //
            var dequeueUser = _usersFriendQueuedRepository.Get(user.FriendCode.ToLowerInvariant());
            if (dequeueUser != null) 
                _usersFriendQueuedRepository.Remove(user.FriendCode.ToLowerInvariant());
            return dequeueUser;
        }
    }
}