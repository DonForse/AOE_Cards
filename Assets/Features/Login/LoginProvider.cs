using Features.Login.Scripts.Domain;
using Features.Login.Scripts.Infrastructure;
using Features.ServerLogic;
using Features.ServerLogic.Controllers;

namespace Features.Login
{
    public class LoginProvider
    {
        private static ILoginService _loginService;

        public static ILoginService LoginService() => _loginService ??= new LoginService();

        public static ILoginService OfflineLoginService() => _loginService ??= new OfflineLoginService(new UserController(ServerLogicProvider.UsersRepository()));
    }
}