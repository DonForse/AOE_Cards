using Infrastructure.Services;

namespace Infrastructure
{
    public class MatchProvider
    {
        private static IMatchService _matchService;

        public static IMatchService MatchService(ICardProvider cardProvider) => _matchService ??= new MatchService(cardProvider);
    }
}