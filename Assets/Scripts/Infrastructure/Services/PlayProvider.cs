using AoeCards.Controllers;
using ServerLogic;
using ServerLogic.Controllers;

namespace Infrastructure.Services
{
    public static class PlayProvider
    {
        private static IPlayService _playService;

        public static IPlayService PlayService(ICardProvider cardProvider) => _playService ??= new PlayService(cardProvider);

        public static IPlayService OfflinePlayService(InMemoryCardProvider cardProvider) => _playService??= 
            new OfflinePlayService(new RoundController(ServerLogicProvider.MatchesRepository(), ServerLogicProvider.CardRepository()),
                new PlayController(ServerLogicProvider.MatchesRepository(), ServerLogicProvider.CardRepository()),
                cardProvider,new RerollController(ServerLogicProvider.MatchesRepository(), ServerLogicProvider.CardRepository()));
    }
}