using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Users.Infrastructure
{
    public interface IUsersRepository
    {
        bool Add(User user);
        User Get(string userName);
        User GetById(string userId);
        bool Remove(string username);
    }
}