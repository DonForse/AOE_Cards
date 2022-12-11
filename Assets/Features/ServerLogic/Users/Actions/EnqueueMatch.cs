using System;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Users.Actions
{
    public class EnqueueMatch : IEnqueueMatch
    {
        private readonly IUsersQueuedRepository _usersQueuedRepository;

        public EnqueueMatch(IUsersQueuedRepository usersQueuedRepository)
        {
            _usersQueuedRepository = usersQueuedRepository;
        }

        public void Execute(User user, DateTime date) {

            if (_usersQueuedRepository.Get(user.Id) == null)
                _usersQueuedRepository.Add(user, date);
        }

    }
}