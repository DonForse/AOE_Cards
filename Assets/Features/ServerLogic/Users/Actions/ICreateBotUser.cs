using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Users.Actions
{
    public interface ICreateBotUser
    {
        User Execute();
    }
}