using System;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Users.Actions
{
    public class DequeueMatch : IDequeueMatch
    {
        private readonly IUsersQueuedRepository _usersQueuedRepository;

        public DequeueMatch(IUsersQueuedRepository usersQueuedRepository)
        {
            _usersQueuedRepository = usersQueuedRepository;
        }

        public Tuple<User,DateTime> Execute(User user) {

            var dequeueUser = _usersQueuedRepository.Get(user.Id);
            if (dequeueUser != null) 
                _usersQueuedRepository.Remove(user.Id);
            return dequeueUser;
        }

    }
}