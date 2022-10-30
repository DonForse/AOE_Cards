using ServerLogic.Matches.Infrastructure;
using ServerLogic.Users.Domain;

namespace ServerLogic.Users.Actions
{
    internal class EnqueueFriendUser
    {
        private readonly IFriendsUsersQueuedRepository _usersQueuedRepository;

        public EnqueueFriendUser(IFriendsUsersQueuedRepository usersQueuedRepository)
        {
            _usersQueuedRepository = usersQueuedRepository;
        }

        public void Execute(User user, string friendCode)
        {
            if (_usersQueuedRepository.Get(user.FriendCode) == null)
                _usersQueuedRepository.Add(user, friendCode.ToLower());
        }
    }
}