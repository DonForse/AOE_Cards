using System;

namespace Infrastructure.Services
{
    public  interface ITokenService
    {
        IObservable<UserResponseDto> RefreshToken();
    }
}