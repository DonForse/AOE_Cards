namespace Features.ServerLogic.Matches.Infrastructure
{
    public interface IUserMatchesRepository
    {
        void Add(string userId, string matchId);
        string GetMatchId(string userId);
        void Remove(string userId);

    }
}