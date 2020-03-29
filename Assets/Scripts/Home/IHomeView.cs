using Infrastructure.Services;

namespace Home
{
    public interface IHomeView
    {
        void OnMatchFound(Match matchStatus);
        void OnStartLookingForMatch();
        void OnError(string message);
    }
}