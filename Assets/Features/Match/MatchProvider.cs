using Features.Infrastructure;
using Features.Match.Domain;
using Features.ServerLogic;
using Features.ServerLogic.Handlers;

namespace Features.Match
{
    public static class MatchProvider
    {
        private static IMatchService _matchService;

        public static IMatchService MatchService(ICardProvider cardProvider) => _matchService ??= new MatchService(cardProvider);
        public static IMatchService OfflineMatchService(ICardProvider cardProvider) => _matchService ??= new OfflineMatchService(cardProvider,
            new MatchHandler(ServerLogicProvider.GetUserMatch(),
                ServerLogicProvider.MatchCreatorService(), ServerLogicProvider.CreateMatch(),
                ServerLogicProvider.GetUser(),
                ServerLogicProvider.EnqueueFriendMatch(),
                ServerLogicProvider.EnqueueMatch(),
                ServerLogicProvider.DequeueFriendMatch(),
                ServerLogicProvider.DequeueMatch(), 
                ServerLogicProvider.RemoveUserMatch()));
    }
}