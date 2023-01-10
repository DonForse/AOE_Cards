using Features.ServerLogic.Cards.Actions;
using Features.ServerLogic.Cards.Infrastructure;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Matches.Service;
using Features.ServerLogic.Users.Actions;
using Features.ServerLogic.Users.Infrastructure;

namespace Features.ServerLogic
{
    public static class ServerLogicProvider
    {
        private static InMemoryUsersQueuedRepository _usersQueuedRepostiory;
        private static InMemoryFriendsUsersQueuedRepository _friendsUsersQueuedRepository;
        private static InMemoryMatchesRepository _matchesRepository;
        private static InMemoryUsersRepository _usersRepository;
        private static InMemoryCardRepository _cardRepository;
        private static InMemoryUserMatchesRepository _userMatchesRepository;



        public static IPlayUnitCard PlayUnitCard() => new PlayUnitCard(MatchesRepository(), CardRepository(),
            CalculateRoundResult(), CalculateMatchResult(), 
            CreateRound(),ApplyEffectPostUnit());
        public static IPlayUpgradeCard PlayUpgradeCard() => new PlayUpgradeCard(MatchesRepository(), CardRepository());

        public static IPlayInactiveMatch PlayInactiveMatch() =>
            new PlayInactiveMatch(PlayUnitCard(), PlayUpgradeCard(), PlayReroll());


        public static IPlayReroll PlayReroll() => new PlayReroll(GetUnitCard(), GetUpgradeCard());

        public static IMatchCreatorService MatchCreatorService() =>
            new MatchCreatorService(UsersQueuedRepository(),
                FriendsUserQueuedRepository(),
                DequeueMatch(), 
                EnqueueMatch(),
                DequeueFriendMatch(),
                GetUser(), 
                CreateMatch(),
                CreateRound(), 
                ServerConfiguration());

        public static ICreateMatch CreateMatch() => new CreateMatch(MatchesRepository(), CardRepository(),
            ServerConfiguration(), CreateBotUser(), UserMatchesRepository());


        public static ICreateBotUser CreateBotUser() => new CreateBotUser();
        public static IGetUser GetUser() => new GetUser(UsersRepository());
        public static IEnqueueFriendMatch EnqueueFriendMatch() => new EnqueueFriendMatch(FriendsUserQueuedRepository());
        public static IEnqueueMatch EnqueueMatch() => new EnqueueMatch(UsersQueuedRepository());
        public static IDequeueFriendMatch DequeueFriendMatch() => new DequeueFriendMatch(FriendsUserQueuedRepository());
        public static IDequeueMatch DequeueMatch() => new DequeueMatch(UsersQueuedRepository());
        public static IGetUserMatch GetUserMatch() => new GetUserMatch(MatchesRepository(), UserMatchesRepository());
        public static IRemoveUserMatch RemoveUserMatch() =>
            new RemoveUserMatch(MatchesRepository(), UserMatchesRepository());
        public static IGetMatch GetMatch() => new GetMatch(MatchesRepository());
        public static ICreateUser CreateUser() => new CreateUser(UsersRepository());
        public static ICreateRound CreateRound() => new CreateRound(GetMatch());
        private static IGetUnitCard GetUnitCard() => new GetUnitCard(CardRepository());
        private static IGetUpgradeCard GetUpgradeCard() => new GetUpgradeCard(CardRepository());
        private static ICalculateRoundResult CalculateRoundResult() => new CalculateRoundResult(MatchesRepository());
        private static ICalculateMatchResult CalculateMatchResult() => new CalculateMatchResult(GetMatch());
        private static IApplyEffectPostUnit ApplyEffectPostUnit() => new ApplyEffectPostUnit(GetMatch());
        private static IUsersQueuedRepository UsersQueuedRepository() =>
            _usersQueuedRepostiory ??= new InMemoryUsersQueuedRepository();

        private static IFriendsUsersQueuedRepository FriendsUserQueuedRepository() =>
            _friendsUsersQueuedRepository ??= new InMemoryFriendsUsersQueuedRepository();

        public static IMatchesRepository MatchesRepository() => _matchesRepository ??= new InMemoryMatchesRepository();

        private static IUserMatchesRepository UserMatchesRepository() =>
            _userMatchesRepository ??= new InMemoryUserMatchesRepository();

        private static IUsersRepository UsersRepository() => _usersRepository ??= new InMemoryUsersRepository();
        private static ICardRepository CardRepository() => _cardRepository ??= new InMemoryCardRepository();

        private static IServerConfiguration ServerConfiguration() => new ServerConfiguration();
    }
}