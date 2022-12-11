using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Users.Actions
{
    public class EnqueueFriendMatch : IEnqueueFriendMatch
    {
        private readonly IFriendsUsersQueuedRepository _usersQueuedRepository;

        public EnqueueFriendMatch(IFriendsUsersQueuedRepository usersQueuedRepository)
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