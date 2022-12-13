using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Users.Actions
{
    public interface IRemoveUserMatch
    {
        void Execute(string userId);
    }
}