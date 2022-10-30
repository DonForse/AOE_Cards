using ServerLogic.Users.Domain;

namespace ServerLogic.Users.Infrastructure
{
    public interface IUsersRepository
    {
        bool Add(User user);
        User Get(string userName);
        User GetById(string userId);
        bool Remove(string username);
    }
}