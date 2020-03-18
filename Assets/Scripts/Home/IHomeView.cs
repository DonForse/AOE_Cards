using Infrastructure.Services;

namespace Home
{
    public interface IHomeView
    {
        void OnMatchFound(MatchStatus matchStatus);
        void OnStartLookingForMatch();
    }
}