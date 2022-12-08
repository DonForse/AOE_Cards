using System;
using Features.Infrastructure.DTOs;

namespace Features.Login.Scripts.Domain
{
    public interface ILoginService
    {
        IObservable<UserResponseDto> Register(string playerName, string password);
        IObservable<UserResponseDto> Login(string playerName, string password);
    }
}