using Features.Infrastructure;
using Features.Match.Domain;
using Features.ServerLogic;
using Features.ServerLogic.Controllers;

namespace Features.Match
{
    public static class MatchProvider
    {
        private static IMatchService _matchService;

        public static IMatchService MatchService(ICardProvider cardProvider) => _matchService ??= new MatchService(cardProvider);
        public static IMatchService OfflineMatchService(ICardProvider cardProvider) => _matchService ??= new OfflineMatchService(cardProvider,
            new MatchController(ServerLogicProvider.UsersQueuedRepository(), ServerLogicProvider.FriendsUserQueuedRepository(),
                ServerLogicProvider.MatchesRepository(), ServerLogicProvider.CardRepository(),ServerLogicProvider.UsersRepository(), ServerLogicProvider.ServerConfiguration()));
    }
}