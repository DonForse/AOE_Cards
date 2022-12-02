namespace Features.ServerLogic.Matches.Action
{
    public interface IPlayUnitCard
    {
        void Execute(string matchId,string userId, string cardname);
    }
}