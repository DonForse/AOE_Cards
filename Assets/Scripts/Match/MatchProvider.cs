using Infrastructure.Services;

namespace Infrastructure
{
    public static class MatchProvider
    {
        private static IMatchService _matchService;

        public static IMatchService MatchService(ICardProvider cardProvider) => _matchService ??= new MatchService(cardProvider);
        public static IMatchService OfflineMatchService(ICardProvider cardProvider) => _matchService ??= new OfflineMatchService(cardProvider);
    }
}