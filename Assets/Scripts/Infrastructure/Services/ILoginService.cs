using System;

namespace Infrastructure.Services
{
    public interface ILoginService
    {
        void Register(string playerName, string password, Action<UserResponseDto> onRegisterComplete,Action<string> onRegisterFailed);
        void Login(string playerName, string password, Action<UserResponseDto> onLoginComplete,Action<string> onLoginFailed);
    }
}