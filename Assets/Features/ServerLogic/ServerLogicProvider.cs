using Features.ServerLogic.Matches.Service;
using ServerLogic.Cards.Infrastructure;
using ServerLogic.Matches.Infrastructure;
using ServerLogic.Users.Infrastructure;

namespace Features.ServerLogic
{
    public static class ServerLogicProvider
    {
        private static InMemoryUsersQueuedRepository _usersQueuedRepostiory;
        private static InMemoryFriendsUsersQueuedRepository _friendsUsersQueuedRepository;
        private static InMemoryMatchesRepository _matchesRepository;
        private static InMemoryUsersRepository _usersRepository;
        private static InMemoryCardRepository _cardRepository;

        public static IUsersQueuedRepository UsersQueuedRepository() => _usersQueuedRepostiory ??= new InMemoryUsersQueuedRepository();
        public static IFriendsUsersQueuedRepository FriendsUserQueuedRepository() => _friendsUsersQueuedRepository ??= new InMemoryFriendsUsersQueuedRepository();
        public static IMatchesRepository MatchesRepository() => _matchesRepository ??= new InMemoryMatchesRepository();
        public static IUsersRepository UsersRepository() => _usersRepository ??= new InMemoryUsersRepository();
        public static ICardRepository CardRepository() => _cardRepository ??= new InMemoryCardRepository();

        public static IServerConfiguration ServerConfiguration() => new ServerConfiguration();

    }
}