using Infrastructure.Services;

namespace Home
{
    public interface IHomeView
    {
        void OnMatchFound(Match matchStatus);
        void OnStartLookingForMatch(bool vsBot);
        void OnQueueLeft();
        void OnError(string message);
    }
}