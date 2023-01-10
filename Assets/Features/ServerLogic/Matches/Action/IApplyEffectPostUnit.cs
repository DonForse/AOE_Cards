namespace Features.ServerLogic.Matches.Action
{
    public interface IApplyEffectPostUnit
    {
        void Execute(string matchId, string userId);
    }
}