using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Users.Actions
{
    public interface ICreateUser
    {
        User Execute(string username, string password);
    }
}