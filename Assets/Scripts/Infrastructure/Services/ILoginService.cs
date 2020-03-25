using Infrastructure.Services;
using System;

public interface ILoginService
{
    void Register(string playerName, string password, Action<UserResponseDto> onRegisterComplete);
    void Login(string playerName, string password, Action<UserResponseDto> onLoginComplete);
}