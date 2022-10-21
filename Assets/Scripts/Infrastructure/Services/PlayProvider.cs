namespace Infrastructure.Services
{
    public static class PlayProvider
    {
        private static IPlayService _playService;

        public static IPlayService PlayService(ICardProvider cardProvider) => _playService ??= new PlayService(cardProvider);

        public static IPlayService OfflinePlayService(InMemoryCardProvider cardProvider) => _playService??=new OfflinePlayService();
    }
}