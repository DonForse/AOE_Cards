using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Users.Actions
{
    public interface IEnqueueFriendMatch
    {
        void Execute(User user, string friendCode);
    }
}