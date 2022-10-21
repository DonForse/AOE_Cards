using Login.Scripts.Domain;
using Login.Scripts.Infrastructure;

namespace Login
{
    public class LoginProvider
    {
        private static ILoginService _loginService;

        public static ILoginService LoginService() => _loginService ??= new LoginService();
    }
}