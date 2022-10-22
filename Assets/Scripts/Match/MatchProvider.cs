using Infrastructure;
using Match.Domain;
using ServerLogic;
using ServerLogic.Controllers;

namespace Match
{
    public static class MatchProvider
    {
        private static IMatchService _matchService;

        public static IMatchService MatchService(ICardProvider cardProvider) => _matchService ??= new MatchService(cardProvider);
        public static IMatchService OfflineMatchService(ICardProvider cardProvider) => _matchService ??= new OfflineMatchService(cardProvider,
            new MatchController(ServerLogicProvider.UsersQueuedRepository(), ServerLogicProvider.FriendsUserQueuedRepository(),
                ServerLogicProvider.MatchesRepository(), ServerLogicProvider.CardRepository(),ServerLogicProvider.UsersRepository()));
    }
}