using System;
using ServerLogic.Matches.Infrastructure;
using ServerLogic.Users.Domain;

namespace ServerLogic.Users.Actions
{
    internal class DequeueUser
    {
        private readonly IUsersQueuedRepository _usersQueuedRepository;

        public DequeueUser(IUsersQueuedRepository usersQueuedRepository)
        {
            _usersQueuedRepository = usersQueuedRepository;
        }

        public Tuple<User,DateTime> Execute(User user, bool remove) {

            var dequeueUser = _usersQueuedRepository.Get(user.Id);
            if (dequeueUser != null)
            {
                if (remove)
                    _usersQueuedRepository.Remove(user.Id);
            }
            return dequeueUser;
        }

    }
}