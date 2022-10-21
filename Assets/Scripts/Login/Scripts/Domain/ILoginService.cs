using System;
using Infrastructure.Services;

namespace Login.Scripts.Domain
{
    public interface ILoginService
    {
        IObservable<UserResponseDto> Register(string playerName, string password);
        IObservable<UserResponseDto> Login(string playerName, string password);
    }
}