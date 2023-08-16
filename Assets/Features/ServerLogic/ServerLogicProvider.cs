using System.Collections.Generic;
using Features.ServerLogic.Bot.Domain.Entities;
using Features.ServerLogic.Cards.Actions;
using Features.ServerLogic.Cards.Domain;
using Features.ServerLogic.Cards.Infrastructure;
using Features.ServerLogic.Game.Domain;
using Features.ServerLogic.Matches;
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


        public static IPlayReroll PlayReroll() => new PlayReroll(MatchesRepository(),GetUnitCard(), GetUpgradeCard() );

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
        public static ICreateUser CreateUser() => new CreateUser(UsersRepository());

        public static HardBot HardBot() => 
            new HardBot(PlayUpgradeCard(), PlayUnitCard(), GetPlayerPlayedUpgradesInMatch());

        public static Bots.Domain.Entities.Bot EasyBot() => new Bots.Domain.Entities.Bot(PlayUpgradeCard(), PlayUnitCard());
        public static ICreateNewRound CreateRound() => new CreateNewRound(MatchesRepository());

        private static IGetUnitCard GetUnitCard() => new GetUnitCard(CardRepository());

        private static IGetUpgradeCard GetUpgradeCard() => new GetUpgradeCard(CardRepository());

        private static ICalculateRoundResult CalculateRoundResult() => new CalculateRoundResult(MatchesRepository(), ApplyEffectPreCalculus());

        private static ICalculateMatchResult CalculateMatchResult() => new CalculateMatchResult(MatchesRepository());

        private static IApplyEffectPostUnit ApplyEffectPostUnit() => new ApplyEffectPostUnit(GetPlayerPlayedUpgradesInMatch(),ApplyEffectPostUnitStrategies());

        private static IEnumerable<IApplyEffectPostUnitStrategy> ApplyEffectPostUnitStrategies()
        {
            return new List<IApplyEffectPostUnitStrategy>
            {
                new MadrasahApplyEffectPostUnitStrategy(MatchesRepository()),
                new FurorCelticaApplyEffectPostUnitStrategy(MatchesRepository())
            };
        }


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


        private static IGetPlayerPlayedUpgradesInMatch GetPlayerPlayedUpgradesInMatch() =>
            new GetPlayerPlayedUpgradesInMatch(MatchesRepository());

        private static IApplyEffectPreCalculus ApplyEffectPreCalculus() =>
            new ApplyEffectPreCalculus(MatchesRepository(), GetPlayerPlayedUpgradesInMatch(), PreCalculusCardStrategies());

        private static IEnumerable<IPreCalculusCardStrategy> PreCalculusCardStrategies()
        {
            return new List<IPreCalculusCardStrategy>
            {
                new TeutonsFaithPreCalculusCardStrategy(MatchesRepository()),
                new PersianTcPreCalculusCardStrategy(GetPlayerPlayedUpgradesInMatch(), MatchesRepository())
            };
        }
    }
}