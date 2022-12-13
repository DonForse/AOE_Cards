using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Users.Actions
{
    public interface IGetUserMatch
    {
        ServerMatch Execute(string userId);
    }
}