namespace Features.ServerLogic.Matches.Action
{
    public interface ICalculateMatchResult
    {
        bool Execute(string matchId);
    }
}