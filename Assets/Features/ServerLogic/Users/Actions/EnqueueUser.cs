using System;
using ServerLogic.Matches.Infrastructure;
using ServerLogic.Users.Domain;

namespace ServerLogic.Users.Actions
{
    internal class EnqueueUser
    {
        private readonly IUsersQueuedRepository _usersQueuedRepository;

        public EnqueueUser(IUsersQueuedRepository usersQueuedRepository)
        {
            _usersQueuedRepository = usersQueuedRepository;
        }

        public void Execute(User user, DateTime date) {

            if (_usersQueuedRepository.Get(user.Id) == null)
                _usersQueuedRepository.Add(user, date);
        }

    }
}