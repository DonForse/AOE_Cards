using Features.ServerLogic;
using Features.ServerLogic.Handlers;

namespace Features.Infrastructure.Services
{
    public static class PlayProvider
    {
        private static IPlayService _playService;

        public static IPlayService PlayService(ICardProvider cardProvider) => _playService ??= new PlayService(cardProvider);

        public static IPlayService OfflinePlayService(InMemoryCardProvider cardProvider) => _playService??= 
            new OfflinePlayService(new RoundHandler(ServerLogicProvider.MatchesRepository(), ServerLogicProvider.CardRepository()),
                new PlayHandler(ServerLogicProvider.MatchesRepository(), ServerLogicProvider.CardRepository()),
                cardProvider,new RerollHandler(ServerLogicProvider.MatchesRepository(), ServerLogicProvider.CardRepository()));
    }
}