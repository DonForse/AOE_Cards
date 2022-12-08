using System;
using Features.Infrastructure.DTOs;

namespace Features.Token.Scripts.Domain
{
    public  interface ITokenService
    {
        IObservable<UserResponseDto> RefreshToken();
    }
}