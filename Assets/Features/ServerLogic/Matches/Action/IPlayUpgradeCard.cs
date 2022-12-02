namespace Features.ServerLogic.Matches.Action
{
    public interface IPlayUpgradeCard
    {
        void Execute(string matchId,string userId, string cardname);
    }
}