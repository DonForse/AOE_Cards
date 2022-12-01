using Features.ServerLogic;
using Login.Scripts.Domain;
using Login.Scripts.Infrastructure;
using ServerLogic;
using ServerLogic.Controllers;

namespace Login
{
    public class LoginProvider
    {
        private static ILoginService _loginService;

        public static ILoginService LoginService() => _loginService ??= new LoginService();

        public static ILoginService OfflineLoginService() => _loginService ??= new OfflineLoginService(new UserController(ServerLogicProvider.UsersRepository()));
    }
}