using System;
using Infrastructure.DTOs;

namespace Token
{
    public  interface ITokenService
    {
        IObservable<UserResponseDto> RefreshToken();
    }
}