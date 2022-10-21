using System;
using Infrastructure.Services;
using Login.Scripts.Domain;
using UniRx;

namespace Login.Scripts.Infrastructure
{
    public class OfflineLoginService : ILoginService
    {
        public IObservable<UserResponseDto> Register(string playerName, string password)
        {
            return Observable.Return(new UserResponseDto
            {
                guid = Guid.NewGuid().ToString(),
                username = Guid.NewGuid().ToString(),
                accessToken = string.Empty,
                refreshToken = string.Empty,
                friendCode = string.Empty
            });
        }

        public IObservable<UserResponseDto> Login(string playerName, string password)
        {
            return Observable.Return(new UserResponseDto
            {
                guid = Guid.NewGuid().ToString(),
                username = Guid.NewGuid().ToString(),
                accessToken = string.Empty,
                refreshToken = string.Empty,
                friendCode = string.Empty
            });
        }
    }
}