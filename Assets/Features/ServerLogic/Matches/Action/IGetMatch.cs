namespace Features.ServerLogic.Matches.Action
{
    public interface IGetMatch
    {
        Domain.ServerMatch Execute(string matchId);
    }
}